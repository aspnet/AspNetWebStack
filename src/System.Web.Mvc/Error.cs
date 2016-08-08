// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    internal static class Error
    {
        public static InvalidOperationException AsyncActionMethodSelector_CouldNotFindMethod(string methodName, Type controllerType)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionMethodSelector_CouldNotFindMethod,
                                           methodName, controllerType);
            return new InvalidOperationException(message);
        }

        public static InvalidOperationException AsyncCommon_AsyncResultAlreadyConsumed()
        {
            return new InvalidOperationException(MvcResources.AsyncCommon_AsyncResultAlreadyConsumed);
        }

        public static InvalidOperationException AsyncCommon_ControllerMustImplementIAsyncManagerContainer(Type actualControllerType)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncCommon_ControllerMustImplementIAsyncManagerContainer,
                                           actualControllerType);
            return new InvalidOperationException(message);
        }

        public static ArgumentException AsyncCommon_InvalidAsyncResult(string parameterName)
        {
            return new ArgumentException(MvcResources.AsyncCommon_InvalidAsyncResult, parameterName);
        }

        public static ArgumentOutOfRangeException AsyncCommon_InvalidTimeout(string parameterName)
        {
            return new ArgumentOutOfRangeException(parameterName, MvcResources.AsyncCommon_InvalidTimeout);
        }

        public static InvalidOperationException ChildActionOnlyAttribute_MustBeInChildRequest(ActionDescriptor actionDescriptor)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ChildActionOnlyAttribute_MustBeInChildRequest,
                                           actionDescriptor.ActionName);
            return new InvalidOperationException(message);
        }

        public static ArgumentException ParameterCannotBeNullOrEmpty(string parameterName)
        {
            return new ArgumentException(MvcResources.Common_NullOrEmpty, parameterName);
        }

        public static InvalidOperationException PropertyCannotBeNullOrEmpty(string propertyName)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.Common_PropertyCannotBeNullOrEmpty,
                                           propertyName);
            return new InvalidOperationException(message);
        }

        public static SynchronousOperationException SynchronizationContextUtil_ExceptionThrown(Exception innerException)
        {
            return new SynchronousOperationException(MvcResources.SynchronizationContextUtil_ExceptionThrown, innerException);
        }

        public static InvalidOperationException ViewDataDictionary_WrongTModelType(Type valueType, Type modelType)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ViewDataDictionary_WrongTModelType,
                                           valueType, modelType);
            return new InvalidOperationException(message);
        }

        public static InvalidOperationException ViewDataDictionary_ModelCannotBeNull(Type modelType)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ViewDataDictionary_ModelCannotBeNull,
                                           modelType);
            return new InvalidOperationException(message);
        }

        public static ArgumentOutOfRangeException ArgumentMustBeGreaterThanOrEqualTo(string parameterName, int actualValue, int minValue)
        {
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ArgumentMustBeGreaterThanOrEqualTo,
                                           minValue);
            return new ArgumentOutOfRangeException(parameterName, actualValue, message);
        }

        public static Exception ArgumentNull(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        public static InvalidOperationException InvalidOperation(string messageFormat, params object[] args)
        {
            string message = String.Format(CultureInfo.CurrentCulture, messageFormat, args);
            return new InvalidOperationException(message);
        }

        internal static string Format(string format, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, format, args);
        }

        internal static ArgumentException Argument(string parameterName, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentException(Error.Format(messageFormat, messageArgs), parameterName);
        }
    }
}
