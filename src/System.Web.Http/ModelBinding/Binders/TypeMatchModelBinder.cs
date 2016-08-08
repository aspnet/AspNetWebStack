// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class TypeMatchModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueProviderResult = GetCompatibleValueProviderResult(bindingContext);
            if (valueProviderResult == null)
            {
                return false; // conversion would have failed
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            object model = valueProviderResult.RawValue;
            ModelBindingHelper.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref model);
            bindingContext.Model = model;
            if (bindingContext.ModelMetadata.IsComplexType)
            {
                HttpControllerContext controllerContext = actionContext.ControllerContext;
                if (controllerContext == null)
                {
                    throw Error.Argument("actionContext", SRResources.TypePropertyMustNotBeNull,
                        typeof(HttpActionContext).Name, "ControllerContext");
                }

                HttpConfiguration configuration = controllerContext.Configuration;
                if (configuration == null)
                {
                    throw Error.Argument("actionContext", SRResources.TypePropertyMustNotBeNull,
                        typeof(HttpControllerContext).Name, "Configuration");
                }

                ServicesContainer services = configuration.Services;
                Contract.Assert(services != null);

                IBodyModelValidator validator = services.GetBodyModelValidator();
                ModelMetadataProvider metadataProvider = services.GetModelMetadataProvider();
                if (validator != null && metadataProvider != null)
                {
                    validator.Validate(model, bindingContext.ModelType, metadataProvider, actionContext, bindingContext.ModelName);
                }
            }

            return true;
        }

        internal static ValueProviderResult GetCompatibleValueProviderResult(ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return null; // the value doesn't exist
            }

            if (!TypeHelper.IsCompatibleObject(bindingContext.ModelType, valueProviderResult.RawValue))
            {
                return null; // value is of incompatible type
            }

            return valueProviderResult;
        }
    }
}
