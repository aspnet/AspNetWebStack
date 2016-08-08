// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class ComplexModelDtoModelBinder : IExtensibleModelBinder
    {
        public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(ComplexModelDto), false /* allowNullModel */);

            ComplexModelDto dto = (ComplexModelDto)bindingContext.Model;
            foreach (ModelMetadata propertyMetadata in dto.PropertyMetadata)
            {
                ExtensibleModelBindingContext propertyBindingContext = new ExtensibleModelBindingContext(bindingContext)
                {
                    ModelMetadata = propertyMetadata,
                    ModelName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, propertyMetadata.PropertyName)
                };

                // bind and propagate the values
                IExtensibleModelBinder propertyBinder = bindingContext.ModelBinderProviders.GetBinder(controllerContext, propertyBindingContext);
                if (propertyBinder != null)
                {
                    if (propertyBinder.BindModel(controllerContext, propertyBindingContext))
                    {
                        dto.Results[propertyMetadata] = new ComplexModelDtoResult(propertyBindingContext.Model, propertyBindingContext.ValidationNode);
                    }
                    else
                    {
                        dto.Results[propertyMetadata] = null;
                    }
                }
            }

            return true;
        }
    }
}
