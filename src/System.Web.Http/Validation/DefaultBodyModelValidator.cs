// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Recursively validate an object.
    /// </summary>
    public class DefaultBodyModelValidator : IBodyModelValidator
    {
        /// <summary>
        /// Determines whether the <paramref name="model"/> is valid and adds any validation errors to the <paramref name="actionContext"/>'s <see cref="ModelStateDictionary"/>
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <param name="type">The <see cref="Type"/> to use for validation.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="actionContext">The <see cref="HttpActionContext"/> within which the model is being validated.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns><c>true</c>if <paramref name="model"/> is valid, <c>false</c> otherwise.</returns>
        public bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (metadataProvider == null)
            {
                throw Error.ArgumentNull("metadataProvider");
            }

            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (model != null && !ShouldValidateType(model.GetType()))
            {
                return true;
            }

            ModelValidatorProvider[] validatorProviders = actionContext.GetValidatorProviders().ToArray();
            // Optimization : avoid validating the object graph if there are no validator providers
            if (validatorProviders == null || validatorProviders.Length == 0)
            {
                return true;
            }

            ModelMetadata metadata = metadataProvider.GetMetadataForType(() => model, type);
            BodyModelValidatorContext validationContext = new BodyModelValidatorContext(actionContext.ModelState)
            {
                MetadataProvider = metadataProvider,
                ActionContext = actionContext,
                ValidatorCache = actionContext.GetValidatorCache(),
                RootPrefix = keyPrefix
            };
            return ValidateNodeAndChildren(metadata, validationContext, container: null, validators: null);
        }

        /// <summary>
        /// Determines whether instances of a particular type should be validated
        /// </summary>
        /// <param name="type">The type to validate.</param>
        /// <returns><c>true</c> if the type should be validated; <c>false</c> otherwise</returns>
        public virtual bool ShouldValidateType(Type type)
        {
            return !MediaTypeFormatterCollection.IsTypeExcludedFromValidation(type);
        }

        /// <summary>
        /// Recursively validate the given <paramref name="metadata"/> and <paramref name="container"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> for the object to validate.</param>
        /// <param name="validationContext">The <see cref="BodyModelValidatorContext"/>.</param>
        /// <param name="container">The object containing the object to validate.</param>
        /// <param name="validators">The collection of <see cref="ModelValidator"/>s.</param>
        /// <returns>
        /// <see langword="true"/> if validation succeeds for the given <paramref name="metadata"/>,
        /// <paramref name="container"/>, and child nodes; <see langword="false"/> otherwise.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "See comment below")]
        protected virtual bool ValidateNodeAndChildren(
            ModelMetadata metadata,
            BodyModelValidatorContext validationContext,
            object container,
            IEnumerable<ModelValidator> validators)
        {
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validationContext == null)
            {
                throw Error.ArgumentNull("validationContext");
            }

            object model = null;
            try
            {
                model = metadata.Model;
            }
            catch
            {
                // Retrieving the model failed - typically caused by a property getter throwing
                // Being unable to retrieve a property is not a validation error - many properties can only be retrieved if certain conditions are met
                // For example, Uri.AbsoluteUri throws for relative URIs but it shouldn't be considered a validation error
                return true;
            }

            bool isValid = true;

            if (validators == null)
            {
                validators = validationContext.ActionContext.GetValidators(metadata, validationContext.ValidatorCache);
            }

            // We don't need to recursively traverse the graph for null values
            if (model == null)
            {
                return ShallowValidate(metadata, validationContext, container, validators);
            }

            // We don't need to recursively traverse the graph for types that shouldn't be validated
            Type modelType = model.GetType();
            if (TypeHelper.IsSimpleType(modelType) || !ShouldValidateType(modelType))
            {
                return ShallowValidate(metadata, validationContext, container, validators);
            }

            // Check to avoid infinite recursion. This can happen with cycles in an object graph.
            if (validationContext.Visited.Contains(model))
            {
                return true;
            }
            validationContext.Visited.Add(model);

            // Validate the children first - depth-first traversal
            IEnumerable enumerableModel = model as IEnumerable;
            if (enumerableModel == null)
            {
                isValid = ValidateProperties(metadata, validationContext);
            }
            else
            {
                isValid = ValidateElements(enumerableModel, validationContext);
            }
            if (isValid)
            {
                // Don't bother to validate this node if children failed.
                isValid = ShallowValidate(metadata, validationContext, container, validators);
            }

            // Pop the object so that it can be validated again in a different path
            validationContext.Visited.Remove(model);

            return isValid;
        }

        /// <summary>
        /// Recursively validate the properties of the given <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> for the object to validate.</param>
        /// <param name="validationContext">The <see cref="BodyModelValidatorContext"/>.</param>
        /// <returns>
        /// <see langword="true"/> if validation succeeds for all properties in <paramref name="metadata"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool ValidateProperties(ModelMetadata metadata, BodyModelValidatorContext validationContext)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validationContext == null)
            {
                throw Error.ArgumentNull("validationContext");
            }

            bool isValid = true;
            PropertyScope propertyScope = new PropertyScope();
            validationContext.KeyBuilders.Push(propertyScope);
            foreach (ModelMetadata childMetadata in validationContext.MetadataProvider.GetMetadataForProperties(metadata.Model, metadata.RealModelType))
            {
                propertyScope.PropertyName = childMetadata.PropertyName;
                if (!ValidateNodeAndChildren(childMetadata, validationContext, metadata.Model, validators: null))
                {
                    isValid = false;
                }
            }
            validationContext.KeyBuilders.Pop();
            return isValid;
        }

        /// <summary>
        /// Recursively validate the elements of the <paramref name="model"/> collection.
        /// </summary>
        /// <param name="model">The <see cref="IEnumerable"/> instance containing the elements to validate.</param>
        /// <param name="validationContext">The <see cref="BodyModelValidatorContext"/>.</param>
        /// <returns>
        /// <see langword="true"/> if validation succeeds for all elements of <paramref name="model"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool ValidateElements(IEnumerable model, BodyModelValidatorContext validationContext)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (validationContext == null)
            {
                throw Error.ArgumentNull("validationContext");
            }

            bool isValid = true;
            Type elementType = GetElementType(model.GetType());
            ModelMetadata elementMetadata = validationContext.MetadataProvider.GetMetadataForType(null, elementType);

            ElementScope elementScope = new ElementScope() { Index = 0 };
            validationContext.KeyBuilders.Push(elementScope);
            IEnumerable<ModelValidator> validators = validationContext.ActionContext.GetValidators(elementMetadata, validationContext.ValidatorCache);

            // if there are no validators or the object is null we bail out quickly
            // when there are large arrays of null, this will save a significant amount of processing
            // with minimal impact to other scenarios.
            bool anyValidatorsDefined = validators.Any();

            foreach (object element in model)
            {
                // If the element is non null, the recursive calls might find more validators.
                // If it's null, then a shallow validation will be performed.
                if (element != null || anyValidatorsDefined)
                {
                    elementMetadata.Model = element;

                    if (!ValidateNodeAndChildren(elementMetadata, validationContext, model, validators))
                    {
                        isValid = false;
                    }
                }

                elementScope.Index++;
            }
            validationContext.KeyBuilders.Pop();
            return isValid;
        }

        /// <summary>
        /// Validate a single node, not including its children.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="validationContext">The <see cref="BodyModelValidatorContext"/>.</param>
        /// <param name="container">The object to validate.</param>
        /// <param name="validators">The collection of <see cref="ModelValidator"/>s.</param>
        /// <returns>
        /// <see langword="true"/> if validation succeeds for the given <paramref name="metadata"/> and
        /// <paramref name="container"/>; <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool ShallowValidate(
            ModelMetadata metadata,
            BodyModelValidatorContext validationContext,
            object container,
            IEnumerable<ModelValidator> validators)
        {
            if (metadata == null)
            {
                throw Error.ArgumentNull("metadata");
            }
            if (validationContext == null)
            {
                throw Error.ArgumentNull("validationContext");
            }
            if (validators == null)
            {
                throw Error.ArgumentNull("validators");
            }

            bool isValid = true;
            string modelKey = null;

            // When the are no validators we bail quickly. This saves a GetEnumerator allocation.
            // In a large array (tens of thousands or more) scenario it's very significant.
            ICollection validatorsAsCollection = validators as ICollection;
            if (validatorsAsCollection != null && validatorsAsCollection.Count == 0)
            {
                return isValid;
            }

            foreach (ModelValidator validator in validators)
            {
                foreach (ModelValidationResult error in validator.Validate(metadata, container))
                {
                    if (modelKey == null)
                    {
                        modelKey = validationContext.RootPrefix;
                        foreach (IBodyModelValidatorKeyBuilder keyBuilder in validationContext.KeyBuilders.Reverse())
                        {
                            modelKey = keyBuilder.AppendTo(modelKey);
                        }
                    }
                    string errorKey = ModelBindingHelper.CreatePropertyModelName(modelKey, error.MemberName);
                    validationContext.ModelState.AddModelError(errorKey, error.Message);
                    isValid = false;
                }
            }
            return isValid;
        }

        private static Type GetElementType(Type type)
        {
            Contract.Assert(typeof(IEnumerable).IsAssignableFrom(type));
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            foreach (Type implementedInterface in type.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return implementedInterface.GetGenericArguments()[0];
                }
            }

            return typeof(object);
        }

        private class PropertyScope : IBodyModelValidatorKeyBuilder
        {
            public string PropertyName { get; set; }

            public string AppendTo(string prefix)
            {
                return ModelBindingHelper.CreatePropertyModelName(prefix, PropertyName);
            }
        }

        private class ElementScope : IBodyModelValidatorKeyBuilder
        {
            public int Index { get; set; }

            public string AppendTo(string prefix)
            {
                return ModelBindingHelper.CreateIndexModelName(prefix, Index);
            }
        }
    }
}
