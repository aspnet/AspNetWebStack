// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc.Async
{
    /// <summary>
    /// When an action method returns either Task or Task{T} the TaskAsyncActionDescriptor provides information about the action.
    /// </summary>
    public class TaskAsyncActionDescriptor : AsyncActionDescriptor, IMethodInfoActionDescriptor
    {
        /// <summary>
        /// dictionary to hold methods that can read Task{T}.Result
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _taskValueExtractors = new ConcurrentDictionary<Type, Func<object, object>>();
        private readonly string _actionName;
        private readonly ControllerDescriptor _controllerDescriptor;
        private readonly Lazy<string> _uniqueId;
        private ParameterDescriptor[] _parametersCache;

        public TaskAsyncActionDescriptor(MethodInfo taskMethodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : this(taskMethodInfo, actionName, controllerDescriptor, validateMethod: true)
        {
        }

        internal TaskAsyncActionDescriptor(MethodInfo taskMethodInfo, string actionName, ControllerDescriptor controllerDescriptor, bool validateMethod)
        {
            if (taskMethodInfo == null)
            {
                throw new ArgumentNullException("taskMethodInfo");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            if (validateMethod)
            {
                string taskFailedMessage = VerifyActionMethodIsCallable(taskMethodInfo);
                if (taskFailedMessage != null)
                {
                    throw new ArgumentException(taskFailedMessage, "taskMethodInfo");
                }
            }

            TaskMethodInfo = taskMethodInfo;
            _actionName = actionName;
            _controllerDescriptor = controllerDescriptor;
            _uniqueId = new Lazy<string>(CreateUniqueId);
        }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public MethodInfo TaskMethodInfo { get; private set; }

        public override ControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
        }

        public MethodInfo MethodInfo
        {
            get { return TaskMethodInfo; }
        }

        public override string UniqueId
        {
            get { return _uniqueId.Value; }
        }

        private string CreateUniqueId()
        {
            var builder = new StringBuilder(base.UniqueId);
            DescriptorUtil.AppendUniqueId(builder, MethodInfo);
            return builder.ToString();
        }

        [SuppressMessage("Microsoft.Web.FxCop", "MW1201:DoNotCallProblematicMethodsOnTask", Justification = "This is commented in great detail.")]
        public override IAsyncResult BeginExecute(ControllerContext controllerContext, IDictionary<string, object> parameters, AsyncCallback callback, object state)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            ParameterInfo[] parameterInfos = TaskMethodInfo.GetParameters();
            var rawParameterValues = from parameterInfo in parameterInfos
                                     select ExtractParameterFromDictionary(parameterInfo, parameters, TaskMethodInfo);
            object[] parametersArray = rawParameterValues.ToArray();

            CancellationTokenSource tokenSource = null;
            bool disposedTimer = false;
            Timer taskCancelledTimer = null;
            bool taskCancelledTimerRequired = false;

            int timeout = GetAsyncManager(controllerContext.Controller).Timeout;

            for (int i = 0; i < parametersArray.Length; i++)
            {
                if (default(CancellationToken).Equals(parametersArray[i]))
                {
                    tokenSource = new CancellationTokenSource();
                    parametersArray[i] = tokenSource.Token;

                    // If there is a timeout we will create a timer to cancel the task when the
                    // timeout expires.
                    taskCancelledTimerRequired = timeout > Timeout.Infinite;
                    break;
                }
            }

            ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(TaskMethodInfo);

            if (taskCancelledTimerRequired)
            {
                taskCancelledTimer = new Timer(_ =>
                {
                    lock (tokenSource)
                    {
                        if (!disposedTimer)
                        {
                            tokenSource.Cancel();
                        }
                    }
                }, state: null, dueTime: timeout, period: Timeout.Infinite);
            }

            Task taskUser = dispatcher.Execute(controllerContext.Controller, parametersArray) as Task;
            Action cleanupAtEndExecute = () =>
            {
                // Cleanup code that's run in EndExecute, after we've waited on the task value.

                if (taskCancelledTimer != null)
                {
                    // Timer callback may still fire after Dispose is called.
                    taskCancelledTimer.Dispose();
                }

                if (tokenSource != null)
                {
                    lock (tokenSource)
                    {
                        disposedTimer = true;
                        tokenSource.Dispose();
                        if (tokenSource.IsCancellationRequested)
                        {
                            // Give Timeout exceptions higher priority over other exceptions, mainly OperationCancelled exceptions
                            // that were signaled with out timeout token.
                            throw new TimeoutException();
                        }
                    }
                }
            };

            TaskWrapperAsyncResult result = new TaskWrapperAsyncResult(taskUser, state, cleanupAtEndExecute);

            // if user supplied a callback, invoke that when their task has finished running.
            if (callback != null)
            {
                if (taskUser.IsCompleted)
                {
                    // If the underlying task is already finished, from our caller's perspective this is just
                    // a synchronous completion.
                    result.CompletedSynchronously = true;
                    callback(result);
                }
                else
                {
                    // If the underlying task isn't yet finished, from our caller's perspective this will be
                    // an asynchronous completion. We'll use ContinueWith instead of Finally for two reasons:
                    //
                    // - Finally propagates the antecedent Task's exception, which we don't need to do here.
                    //   Out caller will eventually call EndExecute, which correctly observes the
                    //   antecedent Task's exception anyway if it faulted.
                    //
                    // - Finally invokes the callback on the captured SynchronizationContext, which is
                    //   unnecessary when using APM (Begin / End). APM assumes that the callback is invoked
                    //   on an arbitrary ThreadPool thread with no SynchronizationContext set up, so
                    //   ContinueWith gets us closer to the desired semantic.
                    result.CompletedSynchronously = false;
                    taskUser.ContinueWith(_ =>
                    {
                        callback(result);
                    });
                }
            }

            return result;
        }

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
        {
            string errorMessage = String.Format(CultureInfo.CurrentCulture, MvcResources.TaskAsyncActionDescriptor_CannotExecuteSynchronously,
                                                ActionName);

            throw new InvalidOperationException(errorMessage);
        }

        public override object EndExecute(IAsyncResult asyncResult)
        {
            TaskWrapperAsyncResult wrapperResult = (TaskWrapperAsyncResult)asyncResult;

            // Throw an exception with the correct call stack
            try
            {
                wrapperResult.Task.ThrowIfFaulted();
            }
            finally
            {
                if (wrapperResult.CleanupThunk != null)
                {
                    wrapperResult.CleanupThunk();
                }
            }

            // Extract the result of the task if there is a result
            return _taskValueExtractors.GetOrAdd(TaskMethodInfo.ReturnType, CreateTaskValueExtractor)(wrapperResult.Task);
        }

        private static Func<object, object> CreateTaskValueExtractor(Type taskType)
        {
            // Task<T>?
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // lambda = arg => (object)(((Task<T>)arg).Result)
                var arg = Expression.Parameter(typeof(object));
                var castArg = Expression.Convert(arg, taskType);
                var fieldAccess = Expression.Property(castArg, "Result");
                var castResult = Expression.Convert(fieldAccess, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(castResult, arg);
                return lambda.Compile();
            }

            // Any exceptions should be thrown before getting the task value so just return null.
            return theTask =>
            {
                return null;
            };
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(TaskMethodInfo, inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(TaskMethodInfo, attributeType, inherit);
        }

        public override ParameterDescriptor[] GetParameters()
        {
            return ActionDescriptorHelper.GetParameters(this, TaskMethodInfo, ref _parametersCache);
        }

        public override ICollection<ActionSelector> GetSelectors()
        {
            return ActionDescriptorHelper.GetSelectors(TaskMethodInfo);
        }

        internal override ICollection<ActionNameSelector> GetNameSelectors()
        {
            return ActionDescriptorHelper.GetNameSelectors(TaskMethodInfo);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.IsDefined(TaskMethodInfo, attributeType, inherit);
        }

        public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            if (useCache && GetType() == typeof(TaskAsyncActionDescriptor))
            {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetMethodFilterAttributes(TaskMethodInfo);
            }
            return base.GetFilterAttributes(useCache);
        }
    }
}
