// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Provides configuration settings common to the new model binding system.
    public static class ModelBinderConfig
    {
        private static ModelBinderErrorMessageProvider _typeConversionErrorMessageProvider;
        private static ModelBinderErrorMessageProvider _valueRequiredErrorMessageProvider;

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

        private static string DefaultTypeConversionErrorMessageProvider(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(controllerContext, modelMetadata, incomingValue, GetValueInvalidResource);
        }

        private static string DefaultValueRequiredErrorMessageProvider(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return GetResourceCommon(controllerContext, modelMetadata, incomingValue, GetValueRequiredResource);
        }

        private static string GetResourceCommon(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue, Func<ControllerContext, string> resourceAccessor)
        {
            string displayName = modelMetadata.GetDisplayName();
            string errorMessageTemplate = resourceAccessor(controllerContext);
            string errorMessage = String.Format(CultureInfo.CurrentCulture, errorMessageTemplate, incomingValue, displayName);
            return errorMessage;
        }

        private static string GetUserResourceString(ControllerContext controllerContext, string resourceName)
        {
            return GetUserResourceString(controllerContext, resourceName, DefaultModelBinder.ResourceClassKey);
        }

        // If the user specified a ResourceClassKey try to load the resource they specified.
        // If the class key is invalid, an exception will be thrown.
        // If the class key is valid but the resource is not found, it returns null, in which
        // case it will fall back to the MVC default error message.
        internal static string GetUserResourceString(ControllerContext controllerContext, string resourceName, string resourceClassKey)
        {
            return (!String.IsNullOrEmpty(resourceClassKey) && (controllerContext != null) && (controllerContext.HttpContext != null))
                       ? controllerContext.HttpContext.GetGlobalResourceObject(resourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string
                       : null;
        }

        private static string GetValueInvalidResource(ControllerContext controllerContext)
        {
            return GetUserResourceString(controllerContext, "PropertyValueInvalid") ?? MvcResources.ModelBinderConfig_ValueInvalid;
        }

        private static string GetValueRequiredResource(ControllerContext controllerContext)
        {
            return GetUserResourceString(controllerContext, "PropertyValueRequired") ?? MvcResources.ModelBinderConfig_ValueRequired;
        }

        /*
         * Initialization routines which replace the default binder implementation with the new binder implementation.
         */

        public static void Initialize()
        {
            Initialize(ModelBinders.Binders, ModelBinderProviders.Providers);
        }

        internal static void Initialize(ModelBinderDictionary binders, ModelBinderProviderCollection providers)
        {
            binders.Clear();
            binders.DefaultBinder = new ExtensibleModelBinderAdapter(providers);
        }
    }
}
