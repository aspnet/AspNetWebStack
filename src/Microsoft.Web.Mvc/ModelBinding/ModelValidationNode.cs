// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class ModelValidationNode
    {
        public ModelValidationNode(ModelMetadata modelMetadata, string modelStateKey)
            : this(modelMetadata, modelStateKey, null)
        {
        }

        public ModelValidationNode(ModelMetadata modelMetadata, string modelStateKey, IEnumerable<ModelValidationNode> childNodes)
        {
            if (modelMetadata == null)
            {
                throw new ArgumentNullException("modelMetadata");
            }
            if (modelStateKey == null)
            {
                throw new ArgumentNullException("modelStateKey");
            }

            ModelMetadata = modelMetadata;
            ModelStateKey = modelStateKey;
            ChildNodes = (childNodes != null) ? childNodes.ToList() : new List<ModelValidationNode>();
        }

        public event EventHandler<ModelValidatedEventArgs> Validated;

        public event EventHandler<ModelValidatingEventArgs> Validating;

        public ICollection<ModelValidationNode> ChildNodes { get; private set; }

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
                foreach (ModelValidationNode childNode in otherNode.ChildNodes)
                {
                    ChildNodes.Add(childNode);
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

        public void Validate(ControllerContext controllerContext)
        {
            Validate(controllerContext, null /* parentNode */);
        }

        public void Validate(ControllerContext controllerContext, ModelValidationNode parentNode)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (SuppressValidation)
            {
                // no-op
                return;
            }

            // pre-validation steps
            ModelValidatingEventArgs validatingEventArgs = new ModelValidatingEventArgs(controllerContext, parentNode);
            OnValidating(validatingEventArgs);
            if (validatingEventArgs.Cancel)
            {
                return;
            }

            ValidateChildren(controllerContext);
            ValidateThis(controllerContext, parentNode);

            // post-validation steps
            ModelValidatedEventArgs validatedEventArgs = new ModelValidatedEventArgs(controllerContext, parentNode);
            OnValidated(validatedEventArgs);
        }

        private void ValidateChildren(ControllerContext controllerContext)
        {
            foreach (ModelValidationNode child in ChildNodes)
            {
                child.Validate(controllerContext, this);
            }

            if (ValidateAllProperties)
            {
                ValidateProperties(controllerContext);
            }
        }

        private void ValidateProperties(ControllerContext controllerContext)
        {
            // Based off CompositeModelValidator.
            ModelStateDictionary modelState = controllerContext.Controller.ViewData.ModelState;

            // DevDiv Bugs #227802 - Caching problem in ModelMetadata requires us to manually regenerate
            // the ModelMetadata.
            object model = ModelMetadata.Model;
            ModelMetadata updatedMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, ModelMetadata.ModelType);

            foreach (ModelMetadata propertyMetadata in updatedMetadata.Properties)
            {
                // Only want to add errors to ModelState if something doesn't already exist for the property node,
                // else we could end up with duplicate or irrelevant error messages.
                string propertyKeyRoot = ModelBinderUtil.CreatePropertyModelName(ModelStateKey, propertyMetadata.PropertyName);

                if (modelState.IsValidField(propertyKeyRoot))
                {
                    foreach (ModelValidator propertyValidator in propertyMetadata.GetValidators(controllerContext))
                    {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(model))
                        {
                            string thisErrorKey = ModelBinderUtil.CreatePropertyModelName(propertyKeyRoot, propertyResult.MemberName);
                            modelState.AddModelError(thisErrorKey, propertyResult.Message);
                        }
                    }
                }
            }
        }

        private void ValidateThis(ControllerContext controllerContext, ModelValidationNode parentNode)
        {
            ModelStateDictionary modelState = controllerContext.Controller.ViewData.ModelState;
            if (!modelState.IsValidField(ModelStateKey))
            {
                return; // short-circuit
            }

            object container = TryConvertContainerToMetadataType(parentNode);
            foreach (ModelValidator validator in ModelMetadata.GetValidators(controllerContext))
            {
                foreach (ModelValidationResult validationResult in validator.Validate(container))
                {
                    string trueModelStateKey = ModelBinderUtil.CreatePropertyModelName(ModelStateKey, validationResult.MemberName);
                    modelState.AddModelError(trueModelStateKey, validationResult.Message);
                }
            }
        }
    }
}
