// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;

namespace System.Web.Http.ModelBinding
{
    // REVIEW: Need a way to get the user's resource string choice
    // Provides configuration settings common to the new model binding system.
    public static class ModelBinderConfig
    {
        private static string _resourceClassKey;
        private static ModelBinderErrorMessageProvider _typeConversionErrorMessageProvider;
        private static ModelBinderErrorMessageProvider _valueRequiredErrorMessageProvider;

        public static string ResourceClassKey
        {
            get { return _resourceClassKey ?? String.Empty; }
            set { _resourceClassKey = value; }
        }

        public static ModelBinderErrorMessageProvider TypeConversionErrorMessageProvider
        {
            get
            {
                if (_typeConversionErrorMessageProvider == null)
                {
                    _typeConversionErrorMessageProvider = DefaultTypeConversionErrorMessageProvider;
                }
                return _typeConversionErrorMessageProvider;
            }
            set { _typeConversionErrorMessageProvider = value; }
        }

        public static ModelBinderErrorMessageProvider ValueRequiredErrorMessageProvider
        {
            get
            {
                if (_valueRequiredErrorMessageProvider == null)
                {
                    _valueRequiredErrorMessageProvider = DefaultValueRequiredErrorMessageProvider;
                }
                return _valueRequiredErrorMessageProvider;
            }
            set { _valueRequiredErrorMessageProvider = value; }
        }

        private static string DefaultTypeConversionErrorMessageProvider(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(actionContext, modelMetadata, incomingValue, GetValueInvalidResource);
        }

        private static string DefaultValueRequiredErrorMessageProvider(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(actionContext, modelMetadata, incomingValue, GetValueRequiredResource);
        }

        private static string GetResourceCommon(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue, Func<HttpActionContext, string> resourceAccessor)
        {
            string displayName = modelMetadata.GetDisplayName();
            string errorMessageTemplate = resourceAccessor(actionContext);
            return Error.Format(errorMessageTemplate, incomingValue, displayName);
        }

        private static string GetUserResourceString(HttpActionContext actionContext, string resourceName)
        {
            return GetUserResourceString(actionContext, resourceName, ResourceClassKey);
        }

        // If the user specified a ResourceClassKey try to load the resource they specified.
        // If the class key is invalid, an exception will be thrown.
        // If the class key is valid but the resource is not found, it returns null, in which
        // case it will fall back to the MVC default error message.
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "resourceName", Justification = "Temporary")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "resourceClassKey", Justification = "Temporary")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "actionContext", Justification = "Temporary")]
        internal static string GetUserResourceString(HttpActionContext actionContext, string resourceName, string resourceClassKey)
        {
#if false
            return (!String.IsNullOrEmpty(resourceClassKey) && (actionContext != null) && (actionContext.HttpContext != null))
                       ? actionContext.HttpContext.GetGlobalResourceObject(resourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string
                       : null;
#else
            return null;
#endif
        }

        private static string GetValueInvalidResource(HttpActionContext actionContext)
        {
            return GetUserResourceString(actionContext, "PropertyValueInvalid") ?? SRResources.ModelBinderConfig_ValueInvalid;
        }

        private static string GetValueRequiredResource(HttpActionContext actionContext)
        {
            return GetUserResourceString(actionContext, "PropertyValueRequired") ?? SRResources.ModelBinderConfig_ValueRequired;
        }
    }
}
