// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Validation
{
    public sealed class ModelValidationNode
    {
        private IEnumerable<ModelValidator> _validators;
        private readonly List<ModelValidationNode> _childNodes;

        public ModelValidationNode(ModelMetadata modelMetadata, string modelStateKey)
            : this(modelMetadata, modelStateKey, null)
        {
        }

        public ModelValidationNode(ModelMetadata modelMetadata, string modelStateKey, IEnumerable<ModelValidationNode> childNodes)
        {
            if (modelMetadata == null)
            {
                throw Error.ArgumentNull("modelMetadata");
            }
            if (modelStateKey == null)
            {
                throw Error.ArgumentNull("modelStateKey");
            }

            ModelMetadata = modelMetadata;
            ModelStateKey = modelStateKey;
            _childNodes = (childNodes != null) ? childNodes.ToList() : new List<ModelValidationNode>();
        }

        public event EventHandler<ModelValidatedEventArgs> Validated;

        public event EventHandler<ModelValidatingEventArgs> Validating;

        public ICollection<ModelValidationNode> ChildNodes 
        {
            get { return _childNodes; }
        }

        public ModelMetadata ModelMetadata { get; private set; }

        public string ModelStateKey { get; private set; }

        public bool ValidateAllProperties { get; set; }

        public bool SuppressValidation { get; set; }

        public void CombineWith(ModelValidationNode otherNode)
        {
            if (otherNode != null && !otherNode.SuppressValidation)
            {
                Validated += otherNode.Validated;
                Validating += otherNode.Validating;
                List<ModelValidationNode> otherChildNodes = otherNode._childNodes;
                for (int i = 0; i < otherChildNodes.Count; i++)
                {
                    ModelValidationNode childNode = otherChildNodes[i];
                    _childNodes.Add(childNode);
                }
            }
        }

        private void OnValidated(ModelValidatedEventArgs e)
        {
            EventHandler<ModelValidatedEventArgs> handler = Validated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnValidating(ModelValidatingEventArgs e)
        {
            EventHandler<ModelValidatingEventArgs> handler = Validating;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private object TryConvertContainerToMetadataType(ModelValidationNode parentNode)
        {
            if (parentNode != null)
            {
                object containerInstance = parentNode.ModelMetadata.Model;
                if (containerInstance != null)
                {
                    Type expectedContainerType = ModelMetadata.ContainerType;
                    if (expectedContainerType != null)
                    {
                        if (expectedContainerType.IsInstanceOfType(containerInstance))
                        {
                            return containerInstance;
                        }
                    }
                }
            }

            return null;
        }

        public void Validate(HttpActionContext actionContext)
        {
            Validate(actionContext, null /* parentNode */);
        }

        public void Validate(HttpActionContext actionContext, ModelValidationNode parentNode)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (SuppressValidation)
            {
                // no-op
                return;
            }

            // pre-validation steps
            ModelValidatingEventArgs validatingEventArgs = new ModelValidatingEventArgs(actionContext, parentNode);
            OnValidating(validatingEventArgs);
            if (validatingEventArgs.Cancel)
            {
                return;
            }

            ValidateChildren(actionContext);
            ValidateThis(actionContext, parentNode);

            // post-validation steps
            ModelValidatedEventArgs validatedEventArgs = new ModelValidatedEventArgs(actionContext, parentNode);
            OnValidated(validatedEventArgs);
        }

        private void ValidateChildren(HttpActionContext actionContext)
        {
            for (int i = 0; i < _childNodes.Count; i++)
            {
                ModelValidationNode child = _childNodes[i];
                child.Validate(actionContext, this);
            }

            if (ValidateAllProperties)
            {
                ValidateProperties(actionContext);
            }
        }

        private void ValidateProperties(HttpActionContext actionContext)
        {
            // Based off CompositeModelValidator.
            ModelStateDictionary modelState = actionContext.ModelState;

            // DevDiv Bugs #227802 - Caching problem in ModelMetadata requires us to manually regenerate
            // the ModelMetadata.
            object model = ModelMetadata.Model;
            ModelMetadata updatedMetadata = actionContext.GetMetadataProvider().GetMetadataForType(() => model, ModelMetadata.ModelType);

            foreach (ModelMetadata propertyMetadata in updatedMetadata.Properties)
            {
                // Only want to add errors to ModelState if something doesn't already exist for the property node,
                // else we could end up with duplicate or irrelevant error messages.
                string propertyKeyRoot = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, propertyMetadata.PropertyName);

                if (modelState.IsValidField(propertyKeyRoot))
                {
                    foreach (ModelValidator propertyValidator in actionContext.GetValidators(propertyMetadata))
                    {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(propertyMetadata, model))
                        {
                            string thisErrorKey = ModelBindingHelper.CreatePropertyModelName(propertyKeyRoot, propertyResult.MemberName);
                            modelState.AddModelError(thisErrorKey, propertyResult.Message);
                        }
                    }
                }
            }
        }

        private void ValidateThis(HttpActionContext actionContext, ModelValidationNode parentNode)
        {
            ModelStateDictionary modelState = actionContext.ModelState;
            if (!modelState.IsValidField(ModelStateKey))
            {
                return; // short-circuit
            }

            // If 'this' is null and there is no parent, we cannot validate, and
            // the DataAnnotationsModelValidator will throw.   So we intercept here
            // to provide a catch-all value-required validation error
            if (parentNode == null && ModelMetadata.Model == null)
            {
                string trueModelStateKey = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, ModelMetadata.GetDisplayName());
                modelState.AddModelError(trueModelStateKey, SRResources.Validation_ValueNotFound);
                return;
            }

            _validators = actionContext.GetValidators(ModelMetadata);

            object container = TryConvertContainerToMetadataType(parentNode);
            // Optimize for the common case where the validators are in an array
            ModelValidator[] validators = _validators.AsArray();
            for (int i = 0; i < validators.Length; i++)
            {
                ModelValidator validator = validators[i];
                foreach (ModelValidationResult validationResult in validator.Validate(ModelMetadata, container))
                {
                    string trueModelStateKey = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, validationResult.MemberName);
                    modelState.AddModelError(trueModelStateKey, validationResult.Message);
                }
            }
        }
    }
}
