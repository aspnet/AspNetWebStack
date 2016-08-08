// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public class MutableObjectModelBinder : IExtensibleModelBinder
    {
        private ModelMetadataProvider _metadataProvider;

        internal ModelMetadataProvider MetadataProvider
        {
            get
            {
                if (_metadataProvider == null)
                {
                    _metadataProvider = ModelMetadataProviders.Current;
                }
                return _metadataProvider;
            }
            set { _metadataProvider = value; }
        }

        public virtual bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            EnsureModel(controllerContext, bindingContext);
            IEnumerable<ModelMetadata> propertyMetadatas = GetMetadataForProperties(controllerContext, bindingContext);
            ComplexModelDto dto = CreateAndPopulateDto(controllerContext, bindingContext, propertyMetadatas);

            // post-processing, e.g. property setters and hooking up validation
            ProcessDto(controllerContext, bindingContext, dto);
            bindingContext.ValidationNode.ValidateAllProperties = true; // complex models require full validation
            return true;
        }

        protected virtual bool CanUpdateProperty(ModelMetadata propertyMetadata)
        {
            return CanUpdatePropertyInternal(propertyMetadata);
        }

        internal static bool CanUpdatePropertyInternal(ModelMetadata propertyMetadata)
        {
            return (!propertyMetadata.IsReadOnly || CanUpdateReadOnlyProperty(propertyMetadata.ModelType));
        }

        private static bool CanUpdateReadOnlyProperty(Type propertyType)
        {
            // Value types have copy-by-value semantics, which prevents us from updating
            // properties that are marked readonly.
            if (propertyType.IsValueType)
            {
                return false;
            }

            // Arrays are strange beasts since their contents are mutable but their sizes aren't.
            // Therefore we shouldn't even try to update these. Further reading:
            // http://blogs.msdn.com/ericlippert/archive/2008/09/22/arrays-considered-somewhat-harmful.aspx
            if (propertyType.IsArray)
            {
                return false;
            }

            // Special-case known immutable reference types
            if (propertyType == typeof(string))
            {
                return false;
            }

            return true;
        }

        private ComplexModelDto CreateAndPopulateDto(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, IEnumerable<ModelMetadata> propertyMetadatas)
        {
            // create a DTO and call into the DTO binder
            ComplexModelDto originalDto = new ComplexModelDto(bindingContext.ModelMetadata, propertyMetadatas);
            ExtensibleModelBindingContext dtoBindingContext = new ExtensibleModelBindingContext(bindingContext)
            {
                ModelMetadata = MetadataProvider.GetMetadataForType(() => originalDto, typeof(ComplexModelDto)),
                ModelName = bindingContext.ModelName
            };

            IExtensibleModelBinder dtoBinder = bindingContext.ModelBinderProviders.GetRequiredBinder(controllerContext, dtoBindingContext);
            dtoBinder.BindModel(controllerContext, dtoBindingContext);
            return (ComplexModelDto)dtoBindingContext.Model;
        }

        protected virtual object CreateModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            // If the Activator throws an exception, we want to propagate it back up the call stack, since the application
            // developer should know that this was an invalid type to try to bind to.
            try
            {
                return Activator.CreateInstance(bindingContext.ModelType);
            }
            catch (MissingMethodException exception)
            {
                // Ensure thrown exception contains the type name.  Might be down a few levels.
                MissingMethodException replacementException =
                    ModelBinderUtil.EnsureDebuggableException(exception, bindingContext.ModelType.FullName);
                if (replacementException != null)
                {
                    throw replacementException;
                }

                throw;
            }
        }

        // Called when the property setter null check failed, allows us to add our own error message to ModelState.
        internal static EventHandler<ModelValidatedEventArgs> CreateNullCheckFailedHandler(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue)
        {
            return (sender, e) =>
            {
                ModelValidationNode validationNode = (ModelValidationNode)sender;
                ModelStateDictionary modelState = e.ControllerContext.Controller.ViewData.ModelState;

                if (modelState.IsValidField(validationNode.ModelStateKey))
                {
                    string errorMessage = ModelBinderConfig.ValueRequiredErrorMessageProvider(controllerContext, modelMetadata, incomingValue);
                    if (errorMessage != null)
                    {
                        modelState.AddModelError(validationNode.ModelStateKey, errorMessage);
                    }
                }
            };
        }

        protected virtual void EnsureModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            if (bindingContext.Model == null)
            {
                bindingContext.ModelMetadata.Model = CreateModel(controllerContext, bindingContext);
            }
        }

        protected virtual IEnumerable<ModelMetadata> GetMetadataForProperties(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            // keep a set of the required properties so that we can cross-reference bound properties later
            HashSet<string> requiredProperties;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext.ModelType, out requiredProperties, out skipProperties);

            return from propertyMetadata in bindingContext.ModelMetadata.Properties
                   let propertyName = propertyMetadata.PropertyName
                   let shouldUpdateProperty = requiredProperties.Contains(propertyName) || !skipProperties.Contains(propertyName)
                   where shouldUpdateProperty && CanUpdateProperty(propertyMetadata)
                   select propertyMetadata;
        }

        private static object GetPropertyDefaultValue(PropertyDescriptor propertyDescriptor)
        {
            DefaultValueAttribute attr = propertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            return (attr != null) ? attr.Value : null;
        }

        internal static void GetRequiredPropertiesCollection(Type modelType, out HashSet<string> requiredProperties, out HashSet<string> skipProperties)
        {
            requiredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            skipProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Use attributes on the property before attributes on the type.
            ICustomTypeDescriptor modelDescriptor = TypeDescriptorHelper.Get(modelType);
            PropertyDescriptorCollection propertyDescriptors = modelDescriptor.GetProperties();
            BindingBehaviorAttribute typeAttr = modelDescriptor.GetAttributes().OfType<BindingBehaviorAttribute>().SingleOrDefault();

            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
            {
                BindingBehaviorAttribute propAttr = propertyDescriptor.Attributes.OfType<BindingBehaviorAttribute>().SingleOrDefault();
                BindingBehaviorAttribute workingAttr = propAttr ?? typeAttr;
                if (workingAttr != null)
                {
                    switch (workingAttr.Behavior)
                    {
                        case BindingBehavior.Required:
                            requiredProperties.Add(propertyDescriptor.Name);
                            break;

                        case BindingBehavior.Never:
                            skipProperties.Add(propertyDescriptor.Name);
                            break;
                    }
                }
            }
        }

        internal void ProcessDto(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, ComplexModelDto dto)
        {
            HashSet<string> requiredProperties;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext.ModelType, out requiredProperties, out skipProperties);

            // Are all of the required fields accounted for?
            HashSet<string> missingRequiredProperties = new HashSet<string>(requiredProperties);
            missingRequiredProperties.ExceptWith(dto.Results.Select(r => r.Key.PropertyName));
            string missingPropertyName = missingRequiredProperties.FirstOrDefault();
            if (missingPropertyName != null)
            {
                string fullPropertyKey = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, missingPropertyName);
                throw Error.BindingBehavior_ValueNotFound(fullPropertyKey);
            }

            // for each property that was bound, call the setter, recording exceptions as necessary
            foreach (var entry in dto.Results)
            {
                ModelMetadata propertyMetadata = entry.Key;

                ComplexModelDtoResult dtoResult = entry.Value;
                if (dtoResult != null)
                {
                    SetProperty(controllerContext, bindingContext, propertyMetadata, dtoResult);
                    bindingContext.ValidationNode.ChildNodes.Add(dtoResult.ValidationNode);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We're recording this exception so that we can act on it later.")]
        protected virtual void SetProperty(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelDtoResult dtoResult)
        {
            PropertyDescriptor propertyDescriptor = TypeDescriptorHelper.Get(bindingContext.ModelType).GetProperties().Find(propertyMetadata.PropertyName, true /* ignoreCase */);
            if (propertyDescriptor == null || propertyDescriptor.IsReadOnly)
            {
                return; // nothing to do
            }

            object value = dtoResult.Model ?? GetPropertyDefaultValue(propertyDescriptor);
            propertyMetadata.Model = value;

            // 'Required' validators need to run first so that we can provide useful error messages if
            // the property setters throw, e.g. if we're setting entity keys to null. See comments in
            // DefaultModelBinder.SetProperty() for more information.
            if (value == null)
            {
                string modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                if (bindingContext.ModelState.IsValidField(modelStateKey))
                {
                    ModelValidator requiredValidator = ModelValidatorProviders.Providers.GetValidators(propertyMetadata, controllerContext).Where(v => v.IsRequired).FirstOrDefault();
                    if (requiredValidator != null)
                    {
                        foreach (ModelValidationResult validationResult in requiredValidator.Validate(bindingContext.Model))
                        {
                            bindingContext.ModelState.AddModelError(modelStateKey, validationResult.Message);
                        }
                    }
                }
            }

            if (value != null || TypeHelpers.TypeAllowsNullValue(propertyDescriptor.PropertyType))
            {
                try
                {
                    propertyDescriptor.SetValue(bindingContext.Model, value);
                }
                catch (Exception ex)
                {
                    // don't display a duplicate error message if a binding error has already occurred for this field
                    string modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                    if (bindingContext.ModelState.IsValidField(modelStateKey))
                    {
                        bindingContext.ModelState.AddModelError(modelStateKey, ex);
                    }
                }
            }
            else
            {
                // trying to set a non-nullable value type to null, need to make sure there's a message
                string modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                if (bindingContext.ModelState.IsValidField(modelStateKey))
                {
                    dtoResult.ValidationNode.Validated += CreateNullCheckFailedHandler(controllerContext, propertyMetadata, value);
                }
            }
        }
    }
}
