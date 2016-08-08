// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // A model binder that is used to interface between the old system and the new system.
    public sealed class ExtensibleModelBinderAdapter : IModelBinder
    {
        public ExtensibleModelBinderAdapter(ModelBinderProviderCollection providers)
        {
            Providers = providers ?? ModelBinderProviders.Providers;
        }

        public ModelBinderProviderCollection Providers { get; private set; }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            CheckPropertyFilter(bindingContext);
            ExtensibleModelBindingContext newBindingContext = CreateNewBindingContext(bindingContext, bindingContext.ModelName);

            IExtensibleModelBinder binder = Providers.GetBinder(controllerContext, newBindingContext);
            if (binder == null && !String.IsNullOrEmpty(bindingContext.ModelName)
                && bindingContext.FallbackToEmptyPrefix && bindingContext.ModelMetadata.IsComplexType)
            {
                // fallback to empty prefix?
                newBindingContext = CreateNewBindingContext(bindingContext, String.Empty /* modelName */);
                binder = Providers.GetBinder(controllerContext, newBindingContext);
            }

            if (binder != null)
            {
                bool boundSuccessfully = binder.BindModel(controllerContext, newBindingContext);
                if (boundSuccessfully)
                {
                    // run validation and return the model
                    newBindingContext.ValidationNode.Validate(controllerContext, null /* parentNode */);
                    return newBindingContext.Model;
                }
            }

            return null; // something went wrong
        }

        private static void CheckPropertyFilter(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType.GetProperties().Select(p => p.Name).Any(name => !bindingContext.PropertyFilter(name)))
            {
                throw new InvalidOperationException(MvcResources.ExtensibleModelBinderAdapter_PropertyFilterMustNotBeSet);
            }
        }

        private ExtensibleModelBindingContext CreateNewBindingContext(ModelBindingContext oldBindingContext, string modelName)
        {
            ExtensibleModelBindingContext newBindingContext = new ExtensibleModelBindingContext
            {
                ModelBinderProviders = Providers,
                ModelMetadata = oldBindingContext.ModelMetadata,
                ModelName = modelName,
                ModelState = oldBindingContext.ModelState,
                ValueProvider = oldBindingContext.ValueProvider
            };

            return newBindingContext;
        }
    }
}
