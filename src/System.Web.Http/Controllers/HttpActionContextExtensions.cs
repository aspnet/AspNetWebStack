// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Extension methods for <see cref="HttpActionContext"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpActionContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="ModelMetadataProvider"/> instance for a given <see cref="HttpActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns>An <see cref="ModelMetadataProvider"/> instance.</returns>
        public static ModelMetadataProvider GetMetadataProvider(this HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidatorProvider"/> instances.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns>A collection of <see cref="ModelValidatorProvider"/> instances.</returns>
        public static IEnumerable<ModelValidatorProvider> GetValidatorProviders(this HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return actionContext.ControllerContext.Configuration.Services.GetModelValidatorProviders();
        }

        /// <summary>
        /// Gets the collection of registered <see cref="ModelValidator"/> instances.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>A collection of registered <see cref="ModelValidator"/> instances.</returns>
        public static IEnumerable<ModelValidator> GetValidators(this HttpActionContext actionContext, ModelMetadata metadata)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            IModelValidatorCache validatorCache = actionContext.GetValidatorCache();
            return actionContext.GetValidators(metadata, validatorCache);
        }

        internal static IEnumerable<ModelValidator> GetValidators(this HttpActionContext actionContext, ModelMetadata metadata, IModelValidatorCache validatorCache)
        {
            if (validatorCache == null)
            {
                // slow path: there is no validator cache on the configuration
                return metadata.GetValidators(actionContext.GetValidatorProviders());
            }
            else
            {
                return validatorCache.GetValidators(metadata);
            }
        }

        internal static IModelValidatorCache GetValidatorCache(this HttpActionContext actionContext)
        {
            Contract.Assert(actionContext != null);

            HttpConfiguration configuration = actionContext.ControllerContext.Configuration;
            return configuration.Services.GetModelValidatorCache();
        }

        public static bool TryBindStrongModel<TModel>(this HttpActionContext actionContext, ModelBindingContext parentBindingContext, string propertyName, ModelMetadataProvider metadataProvider, out TModel model)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ModelBindingContext propertyBindingContext = new ModelBindingContext(parentBindingContext)
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, typeof(TModel)),
                ModelName = ModelBindingHelper.CreatePropertyModelName(parentBindingContext.ModelName, propertyName)
            };

            if (actionContext.Bind(propertyBindingContext))
            {
                object untypedModel = propertyBindingContext.Model;
                model = ModelBindingHelper.CastOrDefault<TModel>(untypedModel);
                parentBindingContext.ValidationNode.ChildNodes.Add(propertyBindingContext.ValidationNode);
                return true;
            }

            model = default(TModel);
            return false;
        }

        // Pulls binders from the config
        public static bool Bind(this HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            Type modelType = bindingContext.ModelType;
            HttpConfiguration config = actionContext.ControllerContext.Configuration;

            IEnumerable<IModelBinder> binders = from provider in config.Services.GetModelBinderProviders()
                                                select provider.GetBinder(config, modelType);

            return Bind(actionContext, bindingContext, binders);
        }

        /// <summary>
        /// Attempt to bind against the given ActionContext.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <param name="binders">set of binders to use for binding</param>
        /// <returns>True if the bind was successful, else false.</returns>
        public static bool Bind(this HttpActionContext actionContext, ModelBindingContext bindingContext, IEnumerable<IModelBinder> binders)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (bindingContext == null)
            {
                throw Error.ArgumentNull("bindingContext");
            }

            // Protects against stack overflow for deeply nested model binding
            RuntimeHelpers.EnsureSufficientExecutionStack();

            Type modelType = bindingContext.ModelType;
            HttpConfiguration config = actionContext.ControllerContext.Configuration;

            ModelBinderProvider providerFromAttr;
            if (ModelBindingHelper.TryGetProviderFromAttributes(modelType, out providerFromAttr))
            {
                IModelBinder binder = providerFromAttr.GetBinder(config, modelType);
                if (binder != null)
                {
                    return binder.BindModel(actionContext, bindingContext);
                }
            }

            foreach (IModelBinder binder in binders)
            {
                if (binder != null)
                {
                    if (binder.BindModel(actionContext, bindingContext))
                    {
                        return true;
                    }
                }
            }

            // Either we couldn't find a binder, or the binder couldn't bind. Distinction is not important.
            return false;
        }
    }
}
