// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public abstract class ActionDescriptor : ICustomAttributeProvider, IUniquelyIdentifiable
    {
        private static readonly ActionMethodDispatcherCache _staticDispatcherCache = new ActionMethodDispatcherCache();

        private static readonly ActionSelector[] _emptySelectors = new ActionSelector[0];
        private static readonly ActionNameSelector[] _emptyNameSelectors = new ActionNameSelector[0];
        private string _uniqueId;
        private ActionMethodDispatcherCache _instanceDispatcherCache;

        protected ActionDescriptor()
        {
        }

        public abstract string ActionName { get; }

        public abstract ControllerDescriptor ControllerDescriptor { get; }

        internal ActionMethodDispatcherCache DispatcherCache
        {
            get
            {
                if (_instanceDispatcherCache == null)
                {
                    _instanceDispatcherCache = _staticDispatcherCache;
                }
                return _instanceDispatcherCache;
            }
            set { _instanceDispatcherCache = value; }
        }

        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "This is overridden elsewhere in System.Web.Mvc")]
        public virtual string UniqueId
        {
            get 
            {
                if (_uniqueId == null)
                {
                    _uniqueId = CreateUniqueId();
                }
                return _uniqueId; 
            }
        }

        private string CreateUniqueId()
        {
            return DescriptorUtil.CreateUniqueId(GetType(), ControllerDescriptor, ActionName);
        }

        public abstract object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters);

        internal static object ExtractParameterFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters, MethodInfo methodInfo)
        {
            object value;

            if (!parameters.TryGetValue(parameterInfo.Name, out value))
            {
                // the key should always be present, even if the parameter value is null
                string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_ParameterNotInDictionary,
                                               parameterInfo.Name, parameterInfo.ParameterType, methodInfo, methodInfo.DeclaringType);
                throw new ArgumentException(message, "parameters");
            }

            if (value == null && !TypeHelpers.TypeAllowsNullValue(parameterInfo.ParameterType))
            {
                // tried to pass a null value for a non-nullable parameter type
                string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_ParameterCannotBeNull,
                                               parameterInfo.Name, parameterInfo.ParameterType, methodInfo, methodInfo.DeclaringType);
                throw new ArgumentException(message, "parameters");
            }

            if (value != null && !parameterInfo.ParameterType.IsInstanceOfType(value))
            {
                // value was supplied but is not of the proper type
                string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_ParameterValueHasWrongType,
                                               parameterInfo.Name, methodInfo, methodInfo.DeclaringType, value.GetType(), parameterInfo.ParameterType);
                throw new ArgumentException(message, "parameters");
            }

            return value;
        }

        internal static object ExtractParameterOrDefaultFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters)
        {
            Type parameterType = parameterInfo.ParameterType;

            object value;
            parameters.TryGetValue(parameterInfo.Name, out value);

            // if wrong type, replace with default instance
            if (parameterType.IsInstanceOfType(value))
            {
                return value;
            }
            else
            {
                object defaultValue;
                if (ParameterInfoUtil.TryGetDefaultValue(parameterInfo, out defaultValue))
                {
                    return defaultValue;
                }
                else
                {
                    return TypeHelpers.GetDefaultValue(parameterType);
                }
            }
        }

        public virtual object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            return (object[])Array.CreateInstance(attributeType, 0);
        }

        public virtual IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            return GetCustomAttributes(typeof(FilterAttribute), inherit: true).Cast<FilterAttribute>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Please call System.Web.Mvc.FilterProviders.Providers.GetFilters() now.", true)]
        public virtual FilterInfo GetFilters()
        {
            return new FilterInfo();
        }

        public abstract ParameterDescriptor[] GetParameters();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual ICollection<ActionSelector> GetSelectors()
        {
            return _emptySelectors;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        internal virtual ICollection<ActionNameSelector> GetNameSelectors()
        {
            return _emptyNameSelectors;
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            return false;
        }

        internal static string VerifyActionMethodIsCallable(MethodInfo methodInfo)
        {
            // we can't call static methods
            if (methodInfo.IsStatic)
            {
                return String.Format(CultureInfo.CurrentCulture,
                                     MvcResources.ReflectedActionDescriptor_CannotCallStaticMethod,
                                     methodInfo,
                                     methodInfo.ReflectedType.FullName);
            }

            // we can't call instance methods where the 'this' parameter is a type other than ControllerBase
            if (!typeof(ControllerBase).IsAssignableFrom(methodInfo.ReflectedType))
            {
                return String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_CannotCallInstanceMethodOnNonControllerType,
                                     methodInfo, methodInfo.ReflectedType.FullName);
            }

            // we can't call methods with open generic type parameters
            if (methodInfo.ContainsGenericParameters)
            {
                return String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_CannotCallOpenGenericMethods,
                                     methodInfo, methodInfo.ReflectedType.FullName);
            }

            // we can't call methods with ref/out parameters
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef)
                {
                    return String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedActionDescriptor_CannotCallMethodsWithOutOrRefParameters,
                                         methodInfo, methodInfo.ReflectedType.FullName, parameterInfo);
                }
            }

            // we can call this method
            return null;
        }
    }
}
