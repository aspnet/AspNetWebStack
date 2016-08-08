// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class ComplexModelDtoModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext, typeof(ComplexModelDto), false /* allowNullModel */);

            ComplexModelDto dto = (ComplexModelDto)bindingContext.Model;
            foreach (ModelMetadata propertyMetadata in dto.PropertyMetadata)
            {
                ModelBindingContext propertyBindingContext = new ModelBindingContext(bindingContext)
                {
                    ModelMetadata = propertyMetadata,
                    ModelName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName, propertyMetadata.PropertyName)
                };

                // bind and propagate the values
                // If we can't bind, then leave the result missing (don't add a null).
                if (actionContext.Bind(propertyBindingContext))
                {
                    dto.Results[propertyMetadata] = new ComplexModelDtoResult(propertyBindingContext.Model, propertyBindingContext.ValidationNode);
                }
            }

            return true;
        }
    }
}
