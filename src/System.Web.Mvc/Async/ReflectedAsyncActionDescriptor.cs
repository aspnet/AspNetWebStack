// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System.Web.Mvc.Async
{
    public class ReflectedAsyncActionDescriptor : AsyncActionDescriptor, IMethodInfoActionDescriptor
    {
        private readonly object _executeTag = new object();

        private readonly string _actionName;
        private readonly ControllerDescriptor _controllerDescriptor;
        private readonly Lazy<string> _uniqueId;
        private ParameterDescriptor[] _parametersCache;

        public ReflectedAsyncActionDescriptor(MethodInfo asyncMethodInfo, MethodInfo completedMethodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : this(asyncMethodInfo, completedMethodInfo, actionName, controllerDescriptor, true /* validateMethods */)
        {
        }

        internal ReflectedAsyncActionDescriptor(MethodInfo asyncMethodInfo, MethodInfo completedMethodInfo, string actionName, ControllerDescriptor controllerDescriptor, bool validateMethods)
        {
            if (asyncMethodInfo == null)
            {
                throw new ArgumentNullException("asyncMethodInfo");
            }
            if (completedMethodInfo == null)
            {
                throw new ArgumentNullException("completedMethodInfo");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            if (validateMethods)
            {
                string asyncFailedMessage = VerifyActionMethodIsCallable(asyncMethodInfo);
                if (asyncFailedMessage != null)
                {
                    throw new ArgumentException(asyncFailedMessage, "asyncMethodInfo");
                }

                string completedFailedMessage = VerifyActionMethodIsCallable(completedMethodInfo);
                if (completedFailedMessage != null)
                {
                    throw new ArgumentException(completedFailedMessage, "completedMethodInfo");
                }
            }

            AsyncMethodInfo = asyncMethodInfo;
            CompletedMethodInfo = completedMethodInfo;
            _actionName = actionName;
            _controllerDescriptor = controllerDescriptor;
            _uniqueId = new Lazy<string>(CreateUniqueId);
        }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public MethodInfo AsyncMethodInfo { get; private set; }

        public MethodInfo CompletedMethodInfo { get; private set; }

        public override ControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
        }

        public MethodInfo MethodInfo
        {
            get { return AsyncMethodInfo; }
        }

        public override string UniqueId
        {
            get { return _uniqueId.Value; }
        }

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

            AsyncManager asyncManager = GetAsyncManager(controllerContext.Controller);

            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState)
            {
                // call the XxxAsync() method
                ParameterInfo[] parameterInfos = AsyncMethodInfo.GetParameters();
                var rawParameterValues = from parameterInfo in parameterInfos
                                         select ExtractParameterFromDictionary(parameterInfo, parameters, AsyncMethodInfo);
                object[] parametersArray = rawParameterValues.ToArray();

                TriggerListener listener = new TriggerListener();
                SimpleAsyncResult asyncResult = new SimpleAsyncResult(asyncState);

                // hook the Finished event to notify us upon completion
                Trigger finishTrigger = listener.CreateTrigger();
                asyncManager.Finished += delegate
                {
                    finishTrigger.Fire();
                };
                asyncManager.OutstandingOperations.Increment();

                // to simplify the logic, force the rest of the pipeline to execute in an asynchronous callback
                listener.SetContinuation(() => ThreadPool.QueueUserWorkItem(_ => asyncResult.MarkCompleted(false /* completedSynchronously */, asyncCallback)));

                // the inner operation might complete synchronously, so all setup work has to be done before this point
                ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(AsyncMethodInfo);
                dispatcher.Execute(controllerContext.Controller, parametersArray); // ignore return value from this method

                // now that the XxxAsync() method has completed, kick off any pending operations
                asyncManager.OutstandingOperations.Decrement();
                listener.Activate();
                return asyncResult;
            };

            EndInvokeDelegate<object> endDelegate = delegate(IAsyncResult asyncResult)
            {
                // call the XxxCompleted() method
                ParameterInfo[] completionParametersInfos = CompletedMethodInfo.GetParameters();
                var rawCompletionParameterValues = from parameterInfo in completionParametersInfos
                                                   select ExtractParameterOrDefaultFromDictionary(parameterInfo, asyncManager.Parameters);
                object[] completionParametersArray = rawCompletionParameterValues.ToArray();

                ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(CompletedMethodInfo);
                object actionReturnValue = dispatcher.Execute(controllerContext.Controller, completionParametersArray);
                return actionReturnValue;
            };

            return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _executeTag, asyncManager.Timeout);
        }

        private string CreateUniqueId()
        {
            return base.UniqueId + DescriptorUtil.CreateUniqueId(AsyncMethodInfo, CompletedMethodInfo);
        }

        public override object EndExecute(IAsyncResult asyncResult)
        {
            return AsyncResultWrapper.End<object>(asyncResult, _executeTag);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(AsyncMethodInfo, inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(AsyncMethodInfo, attributeType, inherit);
        }

        public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            if (useCache && GetType() == typeof(ReflectedAsyncActionDescriptor))
            {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetMethodFilterAttributes(AsyncMethodInfo);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override ParameterDescriptor[] GetParameters()
        {
            return ActionDescriptorHelper.GetParameters(this, AsyncMethodInfo, ref _parametersCache);
        }

        public override ICollection<ActionSelector> GetSelectors()
        {
            return ActionDescriptorHelper.GetSelectors(AsyncMethodInfo);
        }

        internal override ICollection<ActionNameSelector> GetNameSelectors()
        {
            return ActionDescriptorHelper.GetNameSelectors(AsyncMethodInfo);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.IsDefined(AsyncMethodInfo, attributeType, inherit);
        }
    }
}
