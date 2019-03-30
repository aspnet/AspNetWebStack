// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// An action descriptor representing a reflected synchronous or asynchronous action method.
    /// </summary>
    public class ReflectedHttpActionDescriptor : HttpActionDescriptor
    {
        private static readonly object[] _empty = new object[0];

        private readonly Lazy<Collection<HttpParameterDescriptor>> _parameters;
        private ParameterInfo[] _parameterInfos;

        private Lazy<ActionExecutor> _actionExecutor;
        private MethodInfo _methodInfo;
        private Type _returnType;
        private string _actionName;
        private Collection<HttpMethod> _supportedHttpMethods;

        // Getting custom attributes via reflection is slow. 
        // But iterating over a object[] to pick out specific types is fast. 
        // Furthermore, many different services may call to ask for different attributes, so we have multiple callers. 
        // That means there's not a single cache for the callers, which means there's some value caching here.
        // This cache can be a 2x speedup in some benchmarks.
        private object[] _attributeCache;
        private object[] _declaredOnlyAttributeCache;

        private static readonly HttpMethod[] _supportedHttpMethodsByConvention = 
        { 
            HttpMethod.Get, 
            HttpMethod.Post, 
            HttpMethod.Put, 
            HttpMethod.Delete, 
            HttpMethod.Head, 
            HttpMethod.Options, 
            new HttpMethod("PATCH") 
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedHttpActionDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public ReflectedHttpActionDescriptor()
        {
            _parameters = new Lazy<Collection<HttpParameterDescriptor>>(() => InitializeParameterDescriptors());
            _supportedHttpMethods = new Collection<HttpMethod>();
        }

        public ReflectedHttpActionDescriptor(HttpControllerDescriptor controllerDescriptor, MethodInfo methodInfo)
            : base(controllerDescriptor)
        {
            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }

            InitializeProperties(methodInfo);
            _parameters = new Lazy<Collection<HttpParameterDescriptor>>(() => InitializeParameterDescriptors());
        }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public override Collection<HttpMethod> SupportedHttpMethods
        {
            get { return _supportedHttpMethods; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                InitializeProperties(value);
            }
        }

        private ParameterInfo[] ParameterInfos
        {
            get
            {
                if (_parameterInfos == null)
                {
                    _parameterInfos = _methodInfo.GetParameters();
                }
                return _parameterInfos;
            }
        }

        /// <inheritdoc/>
        public override Type ReturnType
        {
            get { return _returnType; }
        }

        /// <inheritdoc/>
        public override Collection<T> GetCustomAttributes<T>(bool inherit)
        {
            object[] attributes = inherit ? _attributeCache : _declaredOnlyAttributeCache;
            return new Collection<T>(TypeHelper.OfType<T>(attributes));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (arguments == null)
            {
                throw Error.ArgumentNull("arguments");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<object>();
            }

            try
            {
                object[] argumentValues = PrepareParameters(arguments, controllerContext);
                return _actionExecutor.Value.Execute(controllerContext.Controller, argumentValues);
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        public override Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>(GetCustomAttributes<IFilter>().Concat(base.GetFilters()).ToList());
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _parameters.Value;
        }

        private void InitializeProperties(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            _parameterInfos = null;
            _returnType = GetReturnType(methodInfo);
            _actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(_methodInfo));
            _declaredOnlyAttributeCache = _methodInfo.GetCustomAttributes(inherit: false);
            _attributeCache = _methodInfo.GetCustomAttributes(inherit: true);
            _actionName = GetActionName(_methodInfo, _attributeCache);
            _supportedHttpMethods = GetSupportedHttpMethods(_methodInfo, _attributeCache);
        }

        internal static Type GetReturnType(MethodInfo methodInfo)
        {
            Type result = methodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(result))
            {
                result = TypeHelper.GetTaskInnerTypeOrNull(methodInfo.ReturnType);
            }
            if (result == typeof(void))
            {
                result = null;
            }
            return result;
        }

        private Collection<HttpParameterDescriptor> InitializeParameterDescriptors()
        {
            Contract.Assert(_methodInfo != null);

            List<HttpParameterDescriptor> parameterInfos = ParameterInfos.Select(
                (item) => new ReflectedHttpParameterDescriptor(this, item)).ToList<HttpParameterDescriptor>();
            return new Collection<HttpParameterDescriptor>(parameterInfos);
        }

        private object[] PrepareParameters(IDictionary<string, object> parameters, HttpControllerContext controllerContext)
        {
            // This is on a hotpath, so a quick check to avoid the allocation if we have no parameters. 
            if (_parameters.Value.Count == 0)
            {
                return _empty;
            }

            ParameterInfo[] parameterInfos = ParameterInfos;
            int parameterCount = parameterInfos.Length;
            object[] parameterValues = new object[parameterCount];
            for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
            {
                parameterValues[parameterIndex] = ExtractParameterFromDictionary(parameterInfos[parameterIndex], parameters, controllerContext);
            }
            return parameterValues;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        private object ExtractParameterFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters, HttpControllerContext controllerContext)
        {
            object value;

            if (!parameters.TryGetValue(parameterInfo.Name, out value))
            {
                // the key should always be present, even if the parameter value is null
                throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterNotInDictionary,
                                 parameterInfo.Name, parameterInfo.ParameterType, MethodInfo, MethodInfo.DeclaringType)));
            }

            if (value == null && !TypeHelper.TypeAllowsNullValue(parameterInfo.ParameterType))
            {
                // tried to pass a null value for a non-nullable parameter type
                throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterCannotBeNull,
                                    parameterInfo.Name, parameterInfo.ParameterType, MethodInfo, MethodInfo.DeclaringType)));
            }

            if (value != null && !parameterInfo.ParameterType.IsInstanceOfType(value))
            {
                // value was supplied but is not of the proper type
                throw new HttpResponseException(controllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterValueHasWrongType,
                                    parameterInfo.Name, MethodInfo, MethodInfo.DeclaringType, value.GetType(), parameterInfo.ParameterType)));
            }

            return value;
        }

        private static string GetActionName(MethodInfo methodInfo, object[] actionAttributes)
        {
            ActionNameAttribute nameAttribute = TypeHelper.OfType<ActionNameAttribute>(actionAttributes).FirstOrDefault();
            return nameAttribute != null
                       ? nameAttribute.Name
                       : methodInfo.Name;
        }

        private static Collection<HttpMethod> GetSupportedHttpMethods(MethodInfo methodInfo, object[] actionAttributes)
        {
            Collection<HttpMethod> supportedHttpMethods = new Collection<HttpMethod>();
            ICollection<IActionHttpMethodProvider> httpMethodProviders = TypeHelper.OfType<IActionHttpMethodProvider>(actionAttributes);
            if (httpMethodProviders.Count > 0)
            {
                // Get HttpMethod from attributes
                foreach (IActionHttpMethodProvider httpMethodSelector in httpMethodProviders)
                {
                    foreach (HttpMethod httpMethod in httpMethodSelector.HttpMethods)
                    {
                        supportedHttpMethods.Add(httpMethod);
                    }
                }
            }
            else
            {
                // Get HttpMethod from method name convention 
                for (int i = 0; i < _supportedHttpMethodsByConvention.Length; i++)
                {
                    if (methodInfo.Name.StartsWith(_supportedHttpMethodsByConvention[i].Method, StringComparison.OrdinalIgnoreCase))
                    {
                        supportedHttpMethods.Add(_supportedHttpMethodsByConvention[i]);
                        break;
                    }
                }
            }

            if (supportedHttpMethods.Count == 0)
            {
                // Use POST as the default HttpMethod
                supportedHttpMethods.Add(HttpMethod.Post);
            }

            return supportedHttpMethods;
        }

        // Implementing Equals and GetHashCode is needed here because when tracing is enabled, a different set of action descriptors
        // are available at configuration time for attribute routing and at runtime. This is because the default action selector
        // clears its action descriptor cache when the controller descriptor is different. And since tracing wraps the controller
        // descriptor for tracing, the cache gets cleared and new action descriptors get created for tracing. We need to compare
        // the action descriptors by method info to be able to correlate attribute routing actions to the tracing action descriptors.

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (_methodInfo != null)
            {
                return _methodInfo.GetHashCode();
            }

            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (_methodInfo != null)
            {
                ReflectedHttpActionDescriptor otherDescriptor = obj as ReflectedHttpActionDescriptor;
                if (otherDescriptor == null)
                {
                    return false;
                }

                return _methodInfo.Equals(otherDescriptor._methodInfo);
            }

            return base.Equals(obj);
        }

        private static ActionExecutor InitializeActionExecutor(MethodInfo methodInfo)
        {
            if (methodInfo.ContainsGenericParameters)
            {
                throw Error.InvalidOperation(SRResources.ReflectedHttpActionDescriptor_CannotCallOpenGenericMethods,
                                     methodInfo, methodInfo.ReflectedType.FullName);
            }

            return new ActionExecutor(methodInfo);
        }

        private sealed class ActionExecutor
        {
            private readonly Func<object, object[], Task<object>> _executor;
            private static MethodInfo _convertOfTMethod = typeof(ActionExecutor).GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic);

            public ActionExecutor(MethodInfo methodInfo)
            {
                Contract.Assert(methodInfo != null);
                _executor = GetExecutor(methodInfo);
            }

            public Task<object> Execute(object instance, object[] arguments)
            {
                return _executor(instance, arguments);
            }

            // Method called via reflection.
            private static Task<object> Convert<T>(object taskAsObject)
            {
                Task<T> task = (Task<T>)taskAsObject;
                return task.CastToObject<T>();
            }

            // Do not inline or optimize this method to avoid stack-related reflection demand issues when
            // running from the GAC in medium trust
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static Func<object, Task<object>> CompileGenericTaskConversionDelegate(Type taskValueType)
            {
                Contract.Assert(taskValueType != null);

                return (Func<object, Task<object>>)Delegate.CreateDelegate(typeof(Func<object, Task<object>>), _convertOfTMethod.MakeGenericMethod(taskValueType));
            }

            private static Func<object, object[], Task<object>> GetExecutor(MethodInfo methodInfo)
            {
                // Parameters to executor
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

                // Build parameter list
                List<Expression> parameters = new List<Expression>();
                ParameterInfo[] paramInfos = methodInfo.GetParameters();
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    ParameterInfo paramInfo = paramInfos[i];
                    BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                    UnaryExpression valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                    // valueCast is "(Ti) parameters[i]"
                    parameters.Add(valueCast);
                }

                // Call method
                UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(instanceParameter, methodInfo.ReflectedType) : null;
                MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, parameters);

                // methodCall is "((MethodInstanceType) instance).method((T0) parameters[0], (T1) parameters[1], ...)"
                // Create function
                if (methodCall.Type == typeof(void))
                {
                    // for: public void Action()
                    Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);
                    Action<object, object[]> voidExecutor = lambda.Compile();
                    return (instance, methodParameters) =>
                    {
                        voidExecutor(instance, methodParameters);
                        return TaskHelpers.NullResult();
                    };
                }
                else
                {
                    // must coerce methodCall to match Func<object, object[], object> signature
                    UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                    Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);
                    Func<object, object[], object> compiled = lambda.Compile();
                    if (methodCall.Type == typeof(Task))
                    {
                        // for: public Task Action()
                        return (instance, methodParameters) =>
                        {
                            Task r = (Task)compiled(instance, methodParameters);
                            ThrowIfWrappedTaskInstance(methodInfo, r.GetType());
                            return r.CastToObject();
                        };
                    }
                    else if (typeof(Task).IsAssignableFrom(methodCall.Type))
                    {
                        // for: public Task<T> Action()
                        // constructs: return (Task<object>)Convert<T>(((Task<T>)instance).method((T0) param[0], ...))
                        Type taskValueType = TypeHelper.GetTaskInnerTypeOrNull(methodCall.Type);
                        var compiledConversion = CompileGenericTaskConversionDelegate(taskValueType);

                        return (instance, methodParameters) =>
                        {
                            object callResult = compiled(instance, methodParameters);
                            Task<object> convertedResult = compiledConversion(callResult);
                            return convertedResult;
                        };
                    }
                    else
                    {
                        // for: public T Action()
                        return (instance, methodParameters) =>
                        {
                            var result = compiled(instance, methodParameters);
                            // Throw when the result of a method is Task. Asynchronous methods need to declare that they
                            // return a Task.
                            Task resultAsTask = result as Task;
                            if (resultAsTask != null)
                            {
                                throw Error.InvalidOperation(SRResources.ActionExecutor_UnexpectedTaskInstance,
                                    methodInfo.Name, methodInfo.DeclaringType.Name);
                            }
                            return Task.FromResult(result);
                        };
                    }
                }
            }

            private static void ThrowIfWrappedTaskInstance(MethodInfo method, Type type)
            {
                // Throw if a method declares a return type of Task and returns an instance of Task<Task> or Task<Task<T>>
                // This most likely indicates that the developer forgot to call Unwrap() somewhere.
                Contract.Assert(method.ReturnType == typeof(Task));
                // Fast path: check if type is exactly Task first.
                if (type != typeof(Task))
                {
                    Type innerTaskType = TypeHelper.GetTaskInnerTypeOrNull(type);
                    if (innerTaskType != null && typeof(Task).IsAssignableFrom(innerTaskType))
                    {
                        throw Error.InvalidOperation(SRResources.ActionExecutor_WrappedTaskInstance,
                            method.Name, method.DeclaringType.Name, type.FullName);
                    }
                }
            }
        }
    }
}
