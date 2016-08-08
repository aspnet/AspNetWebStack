// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    /// <summary>
    /// This class is an <see cref="IModelBinder"/> that delegates to one of a collection of
    /// <see cref="ModelBinderProvider"/> instances.
    /// </summary>
    /// <remarks>
    /// If no binder is available and the <see cref="ModelBindingContext"/> allows it,
    /// this class tries to find a binder using an empty prefix.
    /// </remarks>
    public class CompositeModelBinder : IModelBinder
    {
        public CompositeModelBinder(IEnumerable<IModelBinder> binders)
            : this(binders.ToArray())
        {
        }

        public CompositeModelBinder(params IModelBinder[] binders)
        {
            Binders = binders;
        }

        private IModelBinder[] Binders { get; set; }

        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingContext newBindingContext = CreateNewBindingContext(bindingContext, bindingContext.ModelName);

            bool boundSuccessfully = TryBind(actionContext, newBindingContext);
            if (!boundSuccessfully && !String.IsNullOrEmpty(bindingContext.ModelName)
                && bindingContext.FallbackToEmptyPrefix)
            {
                // fallback to empty prefix?
                newBindingContext = CreateNewBindingContext(bindingContext, modelName: String.Empty);
                boundSuccessfully = TryBind(actionContext, newBindingContext);
            }

            if (!boundSuccessfully)
            {
                return false; // something went wrong
            }

            // run validation and return the model
            // If we fell back to an empty prefix above and are dealing with simple types,
            // propagate the non-blank model name through for user clarity in validation errors.
            // Complex types will reveal their individual properties as model names and do not require this.
            if (!newBindingContext.ModelMetadata.IsComplexType && String.IsNullOrEmpty(newBindingContext.ModelName))
            {
                newBindingContext.ValidationNode = new Validation.ModelValidationNode(newBindingContext.ModelMetadata, bindingContext.ModelName);
            }

            newBindingContext.ValidationNode.Validate(actionContext, null /* parentNode */);
            bindingContext.Model = newBindingContext.Model;
            return true;
        }

        private bool TryBind(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            return actionContext.Bind(bindingContext, Binders);
        }

        private static ModelBindingContext CreateNewBindingContext(ModelBindingContext oldBindingContext, string modelName)
        {
            ModelBindingContext newBindingContext = new ModelBindingContext
            {
                ModelMetadata = oldBindingContext.ModelMetadata,
                ModelName = modelName,
                ModelState = oldBindingContext.ModelState,
                ValueProvider = oldBindingContext.ValueProvider
            };

            // validation is expensive to create, so copy it over if we can
            if (Object.ReferenceEquals(modelName, oldBindingContext.ModelName))
            {
                newBindingContext.ValidationNode = oldBindingContext.ValidationNode;
            }

            return newBindingContext;
        }
    }
}
