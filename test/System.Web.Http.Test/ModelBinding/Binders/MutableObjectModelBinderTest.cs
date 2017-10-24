// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding.Binders
{
    public class MutableObjectModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            Mock<IValueProvider> mockValueProvider = new Mock<IValueProvider>();
            mockValueProvider.Setup(o => o.ContainsPrefix(It.IsAny<string>())).Returns(true);

            Mock<IModelBinder> mockDtoBinder = new Mock<IModelBinder>();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person()),
                ModelName = "someName",
                ValueProvider = mockValueProvider.Object
            };
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ControllerContext.Configuration.Services.Replace(typeof(ModelBinderProvider), new SimpleModelBinderProvider(typeof(ComplexModelDto), mockDtoBinder.Object) { SuppressPrefixCheck = true });

            mockDtoBinder
                .Setup(o => o.BindModel(context, It.IsAny<ModelBindingContext>()))
                .Returns((HttpActionContext cc, ModelBindingContext mbc2) =>
                {
                    return true; // just return the DTO unchanged
                });

            Mock<TestableMutableObjectModelBinder> mockTestableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            mockTestableBinder.Setup(o => o.EnsureModelPublic(context, bindingContext)).Verifiable();
            mockTestableBinder.Setup(o => o.GetMetadataForPropertiesPublic(context, bindingContext)).Returns(new ModelMetadata[0]).Verifiable();
            TestableMutableObjectModelBinder testableBinder = mockTestableBinder.Object;
            testableBinder.MetadataProvider = new DataAnnotationsModelMetadataProvider();

            // Act
            bool retValue = testableBinder.BindModel(context, bindingContext);

            // Assert
            Assert.True(retValue);
            Assert.IsType<Person>(bindingContext.Model);
            Assert.True(bindingContext.ValidationNode.ValidateAllProperties);
            mockTestableBinder.Verify();
        }

        [Fact]
        public void CanUpdateProperty_HasPublicSetter_ReturnsTrue()
        {
            // Arrange
            ModelMetadata propertyMetadata = GetMetadataForCanUpdateProperty("ReadWriteString");

            // Act
            bool canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.True(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyArray_ReturnsFalse()
        {
            // Arrange
            ModelMetadata propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyArray");

            // Act
            bool canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyReferenceTypeNotBlacklisted_ReturnsTrue()
        {
            // Arrange
            ModelMetadata propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyObject");

            // Act
            bool canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.True(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyString_ReturnsFalse()
        {
            // Arrange
            ModelMetadata propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyString");

            // Act
            bool canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CanUpdateProperty_ReadOnlyValueType_ReturnsFalse()
        {
            // Arrange
            ModelMetadata propertyMetadata = GetMetadataForCanUpdateProperty("ReadOnlyInt");

            // Act
            bool canUpdate = MutableObjectModelBinder.CanUpdatePropertyInternal(propertyMetadata);

            // Assert
            Assert.False(canUpdate);
        }

        [Fact]
        public void CreateModel()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            object retModel = testableBinder.CreateModelPublic(null, bindingContext);

            // Assert
            Assert.IsType<Person>(retModel);
        }

        [Fact]
        public void EnsureModel_ModelIsNotNull_DoesNothing()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person())
            };

            Mock<TestableMutableObjectModelBinder> mockTestableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            TestableMutableObjectModelBinder testableBinder = mockTestableBinder.Object;

            // Act
            object originalModel = bindingContext.Model;
            testableBinder.EnsureModelPublic(null, bindingContext);
            object newModel = bindingContext.Model;

            // Assert
            Assert.Same(originalModel, newModel);
            mockTestableBinder.Verify(o => o.CreateModelPublic(actionContext, bindingContext), Times.Never());
        }

        [Fact]
        public void EnsureModel_ModelIsNull_CallsCreateModel()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            Mock<TestableMutableObjectModelBinder> mockTestableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            mockTestableBinder.Setup(o => o.CreateModelPublic(null, bindingContext)).Returns(new Person()).Verifiable();
            TestableMutableObjectModelBinder testableBinder = mockTestableBinder.Object;

            // Act
            object originalModel = bindingContext.Model;
            testableBinder.EnsureModelPublic(null, bindingContext);
            object newModel = bindingContext.Model;

            // Assert
            Assert.Null(originalModel);
            Assert.IsType<Person>(newModel);
            mockTestableBinder.Verify();
        }

        [Fact]
        public void GetMetadataForProperties_WithBindAttribute()
        {
            // Arrange
            string[] expectedPropertyNames = new[] { "FirstName", "LastName" };

            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion))
            };

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            IEnumerable<ModelMetadata> propertyMetadatas = testableBinder.GetMetadataForPropertiesPublic(actionContext, bindingContext);
            string[] returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetMetadataForProperties_WithoutBindAttribute()
        {
            // Arrange
            string[] expectedPropertyNames = new[] { "DateOfBirth", "DateOfDeath", "ValueTypeRequired", "FirstName", "LastName", "PropertyWithDefaultValue" };

            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            IEnumerable<ModelMetadata> propertyMetadatas = testableBinder.GetMetadataForPropertiesPublic(actionContext, bindingContext);
            string[] returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetRequiredPropertiesCollection_MixedAttributes()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new ModelWithMixedBindingBehaviors())
            };

            // Act
            HashSet<string> requiredProperties;
            Dictionary<string, ModelValidator> requiredValidators;
            HashSet<string> skipProperties;
            MutableObjectModelBinder.GetRequiredPropertiesCollection(actionContext, bindingContext, out requiredProperties, out requiredValidators, out skipProperties);

            // Assert
            Assert.Equal(new[] { "Required" }, requiredProperties.ToArray());
            Assert.Equal(new[] { "Never" }, skipProperties.ToArray());
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateAlreadyInvalid_DoesNothing()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            context.ModelState.AddModelError("foo.bar", "Some existing error.");

            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(context, null /* parentNode */);

            // Act
            EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(modelMetadata, null /* incomingValue */);
            handler(validationNode, e);

            // Assert
            Assert.False(context.ModelState.ContainsKey("foo"));
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateValid_AddsErrorString()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(context, null /* parentNode */);

            // Act
            EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(modelMetadata, null /* incomingValue */);
            handler(validationNode, e);

            // Assert
            Assert.True(context.ModelState.ContainsKey("foo"));
            Assert.Equal("A value is required.", context.ModelState["foo"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateValid_CallbackReturnsNull_DoesNothing()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(context, null /* parentNode */);

            // Act
            ModelBinderErrorMessageProvider originalProvider = ModelBinderConfig.ValueRequiredErrorMessageProvider;
            try
            {
                ModelBinderConfig.ValueRequiredErrorMessageProvider = (ec, mm, value) => null;
                EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(modelMetadata, null /* incomingValue */);
                handler(validationNode, e);
            }
            finally
            {
                ModelBinderConfig.ValueRequiredErrorMessageProvider = originalProvider;
            }

            // Assert
            Assert.True(context.ModelState.IsValid);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_BindRequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            ModelWithBindRequired model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            ModelMetadata containerMetadata = GetMetadataForObject(model);

            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };
            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);

            ModelMetadata nameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "Name");
            dto.Results[nameProperty] = new ComplexModelDtoResult("John Doe", new ModelValidationNode(nameProperty, ""));

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("The Age property is required.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_BindRequiredFieldNull_RaisesModelError()
        {
            // Arrange
            ModelWithBindRequired model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            ModelMetadata containerMetadata = GetMetadataForObject(model);
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel",
                ModelState = context.ModelState,
            };

            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            ModelMetadata propertyMetadata = dto.PropertyMetadata.Single(o => o.PropertyName == "Name");
            dto.Results[propertyMetadata] =
                new ComplexModelDtoResult("John Doe", new ModelValidationNode(propertyMetadata, "theModel.Name"));

            // Attempt to set non-Nullable property to null.  HttpBindRequiredAttribute should not be relevant in this
            // case because the binding exists.
            propertyMetadata = dto.PropertyMetadata.Single(o => o.PropertyName == "Age");
            dto.Results[propertyMetadata] =
                new ComplexModelDtoResult(null, new ModelValidationNode(propertyMetadata, "theModel.Age"));

            // Act; must also Validate because null-check error handler is late-bound
            testableBinder.ProcessDto(context, bindingContext, dto);
            bindingContext.ValidationNode.Validate(context);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("A value is required.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_RequiredFieldMissing_RaisesModelError()
        {
            // Arrange
            ModelWithRequired model = new ModelWithRequired();
            ModelMetadata containerMetadata = GetMetadataForObject(model);
            HttpActionContext context = ContextUtil.CreateActionContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };

            // Set no properties though Age (a non-Nullable struct) and City (a class) properties are required.
            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(2, modelStateDictionary.Count);

            // Check Age error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.Age", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("The Age field is required.", modelError.ErrorMessage);

            // Check City error.
            Assert.True(modelStateDictionary.TryGetValue("theModel.City", out modelState));

            modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("The City field is required.", modelError.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ProcessDto_RequiredFieldNull_RaisesModelError()
        {
            // Arrange
            ModelWithRequired model = new ModelWithRequired();
            ModelMetadata containerMetadata = GetMetadataForObject(model);
            HttpActionContext context = ContextUtil.CreateActionContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };

            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Make Age valid and City invalid.
            ModelMetadata propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "Age");
            dto.Results[propertyMetadata] =
                new ComplexModelDtoResult(23, new ModelValidationNode(propertyMetadata, "theModel.Age"));
            propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "City");
            dto.Results[propertyMetadata] =
                new ComplexModelDtoResult(null, new ModelValidationNode(propertyMetadata, "theModel.City"));

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check City error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.City", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("The City field is required.", modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_RequiredFieldMissing_RaisesModelErrorWithMessage()
        {
            // Arrange
            Person model = new Person();
            ModelMetadata containerMetadata = GetMetadataForObject(model);
            HttpActionContext context = ContextUtil.CreateActionContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };

            // Set no properties though ValueTypeRequired (a non-Nullable struct) property is required.
            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check ValueTypeRequired error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.ValueTypeRequired", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("Sample message", modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_RequiredFieldNull_RaisesModelErrorWithMessage()
        {
            // Arrange
            Person model = new Person();
            ModelMetadata containerMetadata = GetMetadataForObject(model);
            HttpActionContext context = ContextUtil.CreateActionContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };

            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Make ValueTypeRequired invalid.
            ModelMetadata propertyMetadata = dto.PropertyMetadata.Single(p => p.PropertyName == "ValueTypeRequired");
            dto.Results[propertyMetadata] =
                new ComplexModelDtoResult(null, new ModelValidationNode(propertyMetadata, "theModel.ValueTypeRequired"));

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            ModelStateDictionary modelStateDictionary = bindingContext.ModelState;
            Assert.False(modelStateDictionary.IsValid);
            Assert.Single(modelStateDictionary);

            // Check ValueTypeRequired error.
            ModelState modelState;
            Assert.True(modelStateDictionary.TryGetValue("theModel.ValueTypeRequired", out modelState));

            ModelError modelError = Assert.Single(modelState.Errors);
            Assert.Null(modelError.Exception);
            Assert.NotNull(modelError.ErrorMessage);
            Assert.Equal("Sample message", modelError.ErrorMessage);
        }

        [Fact]
        public void ProcessDto_Success()
        {
            // Arrange
            DateTime dob = new DateTime(2001, 1, 1);
            PersonWithBindExclusion model = new PersonWithBindExclusion
            {
                DateOfBirth = dob
            };
            ModelMetadata containerMetadata = GetMetadataForObject(model);

            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = containerMetadata
            };
            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);

            ModelMetadata firstNameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "FirstName");
            dto.Results[firstNameProperty] = new ComplexModelDtoResult("John", new ModelValidationNode(firstNameProperty, ""));
            ModelMetadata lastNameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "LastName");
            dto.Results[lastNameProperty] = new ComplexModelDtoResult("Doe", new ModelValidationNode(lastNameProperty, ""));
            ModelMetadata dobProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "DateOfBirth");
            dto.Results[dobProperty] = null;

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.ProcessDto(context, bindingContext, dto);

            // Assert
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal(dob, model.DateOfBirth);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyHasDefaultValue_SetsDefaultValue()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person())
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "PropertyWithDefaultValue");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal(123.456m, person.PropertyWithDefaultValue);
            Assert.True(context.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsReadOnly_DoesNothing()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "NonUpdateableProperty");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(null, bindingContext, propertyMetadata, dtoResult, requiredValidator: null);

            // Assert
            // If didn't throw, success!
        }

        [Fact]
        public void SetProperty_PropertyIsSettable_CallsSetter()
        {
            // Arrange
            Person model = new Person();
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(model)
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfBirth");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(new DateTime(2001, 1, 1), validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            validationNode.Validate(context);
            Assert.True(context.ModelState.IsValid);
            Assert.Equal(new DateTime(2001, 1, 1), model.DateOfBirth);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_PropertyIsSettable_SetterThrows_RecordsError()
        {
            // Arrange
            Person model = new Person
            {
                DateOfBirth = new DateTime(1900, 1, 1)
            };
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(model)
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfDeath");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(new DateTime(1800, 1, 1), validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(null, bindingContext, propertyMetadata, dtoResult, requiredValidator: null);

            // Assert
            Assert.Equal("Date of death can't be before date of birth." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo"].Errors[0].Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorNotPresent_RegistersValidationCallback()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person()),
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfBirth");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            Assert.True(context.ModelState.IsValid);
            validationNode.Validate(context, bindingContext.ValidationNode);
            Assert.False(context.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorPresent_AddsModelError()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "ValueTypeRequired");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.ValueTypeRequired");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("Sample message", bindingContext.ModelState["foo.ValueTypeRequired"].Errors[0].ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorNotPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new ModelWhosePropertySetterThrows()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "NameNoAttribute");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.NameNoAttribute");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            ModelError modelError = Assert.Single(bindingContext.ModelState["foo.NameNoAttribute"].Errors);
            Assert.Equal(
                "This is a different exception." + Environment.NewLine + "Parameter name: value",
                modelError.Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new ModelWhosePropertySetterThrows()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "Name");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.Name");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);
            ModelValidator requiredValidator = context.GetValidators(propertyMetadata).Where(v => v.IsRequired).FirstOrDefault();

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            ModelError modelError = Assert.Single(bindingContext.ModelState["foo.Name"].Errors);
            Assert.Equal("This message comes from the [Required] attribute.", modelError.ErrorMessage);
        }

        private static ModelMetadata GetMetadataForCanUpdateProperty(string propertyName)
        {
            DataAnnotationsModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            return metadataProvider.GetMetadataForProperty(null, typeof(MyModelTestingCanUpdateProperty), propertyName);
        }

        private static ModelMetadata GetMetadataForObject(object o)
        {
            DataAnnotationsModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            return metadataProvider.GetMetadataForType(() => o, o.GetType());
        }

        private static ModelMetadata GetMetadataForType(Type t)
        {
            DataAnnotationsModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            return metadataProvider.GetMetadataForType(null, t);
        }

        private class Person
        {
            private DateTime? _dateOfDeath;

            public DateTime DateOfBirth { get; set; }

            public DateTime? DateOfDeath
            {
                get { return _dateOfDeath; }
                set
                {
                    if (value < DateOfBirth)
                    {
                        throw new ArgumentOutOfRangeException("value", "Date of death can't be before date of birth.");
                    }
                    _dateOfDeath = value;
                }
            }

            [Required(ErrorMessage = "Sample message")]
            public int ValueTypeRequired { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }

            [DefaultValue(typeof(decimal), "123.456")]
            public decimal PropertyWithDefaultValue { get; set; }
        }

        private class PersonWithBindExclusion
        {
            [HttpBindNever]
            public DateTime DateOfBirth { get; set; }

            [HttpBindNever]
            public DateTime? DateOfDeath { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }
        }

        private class ModelWithRequired
        {
            public string Name { get; set; }

            [Required]
            public int Age { get; set; }

            [Required]
            public string City { get; set; }
        }

        private class ModelWithBindRequired
        {
            public string Name { get; set; }

            [HttpBindRequired]
            public int Age { get; set; }
        }

        [HttpBindRequired]
        private class ModelWithMixedBindingBehaviors
        {
            public string Required { get; set; }

            [HttpBindNever]
            public string Never { get; set; }

            [HttpBindingBehavior(HttpBindingBehavior.Optional)]
            public string Optional { get; set; }
        }

        private sealed class MyModelTestingCanUpdateProperty
        {
            public int ReadOnlyInt { get; private set; }
            public string ReadOnlyString { get; private set; }
            public string[] ReadOnlyArray { get; private set; }
            public object ReadOnlyObject { get; private set; }
            public string ReadWriteString { get; set; }
        }

        private sealed class ModelWhosePropertySetterThrows
        {
            [Required(ErrorMessage = "This message comes from the [Required] attribute.")]
            public string Name
            {
                get { return null; }
                set { throw new ArgumentException("This is an exception.", "value"); }
            }

            public string NameNoAttribute
            {
                get { return null; }
                set { throw new ArgumentException("This is a different exception.", "value"); }
            }
        }

        public class TestableMutableObjectModelBinder : MutableObjectModelBinder
        {
            public virtual bool CanUpdatePropertyPublic(ModelMetadata propertyMetadata)
            {
                return base.CanUpdateProperty(propertyMetadata);
            }

            protected override bool CanUpdateProperty(ModelMetadata propertyMetadata)
            {
                return CanUpdatePropertyPublic(propertyMetadata);
            }

            public virtual object CreateModelPublic(HttpActionContext context, ModelBindingContext bindingContext)
            {
                return base.CreateModel(context, bindingContext);
            }

            protected override object CreateModel(HttpActionContext context, ModelBindingContext bindingContext)
            {
                return CreateModelPublic(context, bindingContext);
            }

            public virtual void EnsureModelPublic(HttpActionContext context, ModelBindingContext bindingContext)
            {
                base.EnsureModel(context, bindingContext);
            }

            protected override void EnsureModel(HttpActionContext context, ModelBindingContext bindingContext)
            {
                EnsureModelPublic(context, bindingContext);
            }

            public virtual IEnumerable<ModelMetadata> GetMetadataForPropertiesPublic(HttpActionContext context, ModelBindingContext bindingContext)
            {
                return base.GetMetadataForProperties(context, bindingContext);
            }

            protected override IEnumerable<ModelMetadata> GetMetadataForProperties(HttpActionContext context, ModelBindingContext bindingContext)
            {
                return GetMetadataForPropertiesPublic(context, bindingContext);
            }

            public virtual void SetPropertyPublic(HttpActionContext context, ModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelDtoResult dtoResult, ModelValidator requiredValidator)
            {
                base.SetProperty(context, bindingContext, propertyMetadata, dtoResult, requiredValidator);
            }

            protected override void SetProperty(HttpActionContext actionContext, ModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelDtoResult dtoResult, ModelValidator requiredValidator)
            {
                SetPropertyPublic(actionContext, bindingContext, propertyMetadata, dtoResult, requiredValidator);
            }
        }
    }
}
