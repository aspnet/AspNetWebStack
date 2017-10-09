// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class MutableObjectModelBinderTest
    {
        [Fact]
        public void BindModel()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelBinderProviders = new ModelBinderProviderCollection(),
                ModelMetadata = GetMetadataForObject(new Person()),
                ModelName = "someName"
            };

            Mock<IExtensibleModelBinder> mockDtoBinder = new Mock<IExtensibleModelBinder>();
            mockDtoBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc2)
                    {
                        return true; // just return the DTO unchanged
                    });
            bindingContext.ModelBinderProviders.RegisterBinderForType(typeof(ComplexModelDto), mockDtoBinder.Object, true /* suppressPrefixCheck */);

            Mock<TestableMutableObjectModelBinder> mockTestableBinder = new Mock<TestableMutableObjectModelBinder> { CallBase = true };
            mockTestableBinder.Setup(o => o.EnsureModelPublic(controllerContext, bindingContext)).Verifiable();
            mockTestableBinder.Setup(o => o.GetMetadataForPropertiesPublic(controllerContext, bindingContext)).Returns(new ModelMetadata[0]).Verifiable();
            TestableMutableObjectModelBinder testableBinder = mockTestableBinder.Object;
            testableBinder.MetadataProvider = new DataAnnotationsModelMetadataProvider();

            // Act
            bool retValue = testableBinder.BindModel(controllerContext, bindingContext);

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
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
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
        public void CreateModelThrowsIfModelTypeHasNoParameterlessConstructor()
        {
            // Arrange
            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(NoParameterlessCtor)),
            };

            // Act & Assert, confirming type name and full stack are available in Exception
            MissingMethodException exception = Assert.Throws<MissingMethodException>(
                () => testableBinder.CreateModelPublic(null, bindingContext),
                "No parameterless constructor defined for this object. Object type 'Microsoft.Web.Mvc.ModelBinding.Test.MutableObjectModelBinderTest+NoParameterlessCtor'.");
            Assert.Contains("System.Activator.CreateInstance(", exception.ToString());
        }

        [Fact]
        public void EnsureModel_ModelIsNotNull_DoesNothing()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
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
            mockTestableBinder.Verify(o => o.CreateModelPublic(null, bindingContext), Times.Never());
        }

        [Fact]
        public void EnsureModel_ModelIsNull_CallsCreateModel()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
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

            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(PersonWithBindExclusion))
            };

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            IEnumerable<ModelMetadata> propertyMetadatas = testableBinder.GetMetadataForPropertiesPublic(null, bindingContext);
            string[] returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetMetadataForProperties_WithoutBindAttribute()
        {
            // Arrange
            string[] expectedPropertyNames = new[] { "DateOfBirth", "DateOfDeath", "ValueTypeRequired", "FirstName", "LastName", "PropertyWithDefaultValue" };

            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            IEnumerable<ModelMetadata> propertyMetadatas = testableBinder.GetMetadataForPropertiesPublic(null, bindingContext);
            string[] returnedPropertyNames = propertyMetadatas.Select(o => o.PropertyName).ToArray();

            // Assert
            Assert.Equal(expectedPropertyNames, returnedPropertyNames);
        }

        [Fact]
        public void GetRequiredPropertiesCollection_MixedAttributes()
        {
            // Arrange
            Type modelType = typeof(ModelWithMixedBindingBehaviors);

            // Act
            HashSet<string> requiredProperties;
            HashSet<string> skipProperties;
            MutableObjectModelBinder.GetRequiredPropertiesCollection(modelType, out requiredProperties, out skipProperties);

            // Assert
            Assert.Equal(new[] { "Required" }, requiredProperties.ToArray());
            Assert.Equal(new[] { "Never" }, skipProperties.ToArray());
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateAlreadyInvalid_DoesNothing()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            controllerContext.Controller.ViewData.ModelState.AddModelError("foo.bar", "Some existing error.");

            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(controllerContext, null /* parentNode */);

            // Act
            EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(controllerContext, modelMetadata, null /* incomingValue */);
            handler(validationNode, e);

            // Assert
            Assert.False(controllerContext.Controller.ViewData.ModelState.ContainsKey("foo"));
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateValid_AddsErrorString()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };

            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(controllerContext, null /* parentNode */);

            // Act
            EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(controllerContext, modelMetadata, null /* incomingValue */);
            handler(validationNode, e);

            // Assert
            Assert.True(controllerContext.Controller.ViewData.ModelState.ContainsKey("foo"));
            Assert.Equal("A value is required.", controllerContext.Controller.ViewData.ModelState["foo"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void NullCheckFailedHandler_ModelStateValid_CallbackReturnsNull_DoesNothing()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };

            ModelMetadata modelMetadata = GetMetadataForType(typeof(Person));
            ModelValidationNode validationNode = new ModelValidationNode(modelMetadata, "foo");
            ModelValidatedEventArgs e = new ModelValidatedEventArgs(controllerContext, null /* parentNode */);

            // Act
            ModelBinderErrorMessageProvider originalProvider = ModelBinderConfig.ValueRequiredErrorMessageProvider;
            try
            {
                ModelBinderConfig.ValueRequiredErrorMessageProvider = delegate { return null; };
                EventHandler<ModelValidatedEventArgs> handler = MutableObjectModelBinder.CreateNullCheckFailedHandler(controllerContext, modelMetadata, null /* incomingValue */);
                handler(validationNode, e);
            }
            finally
            {
                ModelBinderConfig.ValueRequiredErrorMessageProvider = originalProvider;
            }

            // Assert
            Assert.True(controllerContext.Controller.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void ProcessDto_BindRequiredFieldMissing_Throws()
        {
            // Arrange
            ModelWithBindRequired model = new ModelWithBindRequired
            {
                Name = "original value",
                Age = -20
            };

            ModelMetadata containerMetadata = GetMetadataForObject(model);

            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = containerMetadata,
                ModelName = "theModel"
            };
            ComplexModelDto dto = new ComplexModelDto(containerMetadata, containerMetadata.Properties);

            ModelMetadata nameProperty = dto.PropertyMetadata.Single(o => o.PropertyName == "Name");
            dto.Results[nameProperty] = new ComplexModelDtoResult("John Doe", new ModelValidationNode(nameProperty, ""));

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { testableBinder.ProcessDto(controllerContext, bindingContext, dto); },
                @"A value for 'theModel.Age' is required but was not present in the request.");

            Assert.Equal("original value", model.Name);
            Assert.Equal(-20, model.Age);
        }

        [Fact]
        public void ProcessDto_Success()
        {
            // Arrange
            DateTime dob = new DateTime(2001, 1, 1);
            Person model = new Person
            {
                DateOfBirth = dob
            };
            ModelMetadata containerMetadata = GetMetadataForObject(model);

            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
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
            testableBinder.ProcessDto(controllerContext, bindingContext, dto);

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
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person())
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "PropertyWithDefaultValue");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            var person = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal(123.456m, person.PropertyWithDefaultValue);
            Assert.True(controllerContext.Controller.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_PropertyIsReadOnly_DoesNothing()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForType(typeof(Person))
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "NonUpdateableProperty");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(null, bindingContext, propertyMetadata, dtoResult);

            // Assert
            // If didn't throw, success!
        }

        [Fact]
        public void SetProperty_PropertyIsSettable_CallsSetter()
        {
            // Arrange
            Person model = new Person();
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(model)
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfBirth");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(new DateTime(2001, 1, 1), validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            validationNode.Validate(controllerContext);
            Assert.True(controllerContext.Controller.ViewData.ModelState.IsValid);
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
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(model)
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfDeath");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(new DateTime(1800, 1, 1), validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(null, bindingContext, propertyMetadata, dtoResult);

            // Assert
            Assert.Equal("Date of death can't be before date of birth." + Environment.NewLine
                       + "Parameter name: value",
                         bindingContext.ModelState["foo"].Errors[0].Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorNotPresent_RegistersValidationCallback()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person()),
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "DateOfBirth");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            Assert.True(controllerContext.Controller.ViewData.ModelState.IsValid);
            validationNode.Validate(controllerContext, bindingContext.ValidationNode);
            Assert.False(controllerContext.Controller.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void SetProperty_SettingNonNullableValueTypeToNull_RequiredValidatorPresent_AddsModelError()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new Person()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "ValueTypeRequired");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.ValueTypeRequired");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("Sample message", bindingContext.ModelState["foo.ValueTypeRequired"].Errors[0].ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorNotPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new ModelWhosePropertySetterThrows()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "NameNoAttribute");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.NameNoAttribute");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            ModelError error = Assert.Single(bindingContext.ModelState["foo.NameNoAttribute"].Errors);
            Assert.Equal("This is a different exception." + Environment.NewLine
                       + "Parameter name: value",
                         error.Exception.Message);
        }

        [Fact]
        public void SetProperty_SettingNullableTypeToNull_RequiredValidatorPresent_PropertySetterThrows_AddsRequiredMessageString()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadataForObject(new ModelWhosePropertySetterThrows()),
                ModelName = "foo"
            };

            ModelMetadata propertyMetadata = bindingContext.ModelMetadata.Properties.Single(o => o.PropertyName == "Name");
            ModelValidationNode validationNode = new ModelValidationNode(propertyMetadata, "foo.Name");
            ComplexModelDtoResult dtoResult = new ComplexModelDtoResult(null /* model */, validationNode);

            TestableMutableObjectModelBinder testableBinder = new TestableMutableObjectModelBinder();

            // Act
            testableBinder.SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);

            // Assert
            Assert.False(bindingContext.ModelState.IsValid);
            ModelError error = Assert.Single(bindingContext.ModelState["foo.Name"].Errors);
            Assert.Equal("This message comes from the [Required] attribute.", error.ErrorMessage);
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

        private class NoParameterlessCtor
        {
            public NoParameterlessCtor(int parameter)
            {
            }
        }

        private class PersonWithBindExclusion
        {
            [BindNever]
            public DateTime DateOfBirth { get; set; }

            [BindNever]
            public DateTime? DateOfDeath { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NonUpdateableProperty { get; private set; }
        }

        private class ModelWithBindRequired
        {
            public string Name { get; set; }

            [BindRequired]
            public int Age { get; set; }
        }

        [BindRequired]
        private class ModelWithMixedBindingBehaviors
        {
            public string Required { get; set; }

            [BindNever]
            public string Never { get; set; }

            [BindingBehavior(BindingBehavior.Optional)]
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

            public virtual object CreateModelPublic(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                return base.CreateModel(controllerContext, bindingContext);
            }

            protected override object CreateModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                return CreateModelPublic(controllerContext, bindingContext);
            }

            public virtual void EnsureModelPublic(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                base.EnsureModel(controllerContext, bindingContext);
            }

            protected override void EnsureModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                EnsureModelPublic(controllerContext, bindingContext);
            }

            public virtual IEnumerable<ModelMetadata> GetMetadataForPropertiesPublic(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                return base.GetMetadataForProperties(controllerContext, bindingContext);
            }

            protected override IEnumerable<ModelMetadata> GetMetadataForProperties(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                return GetMetadataForPropertiesPublic(controllerContext, bindingContext);
            }

            public virtual void SetPropertyPublic(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelDtoResult dtoResult)
            {
                base.SetProperty(controllerContext, bindingContext, propertyMetadata, dtoResult);
            }

            protected override void SetProperty(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelDtoResult dtoResult)
            {
                SetPropertyPublic(controllerContext, bindingContext, propertyMetadata, dtoResult);
            }
        }

        private class EmptyController : Controller
        {
        }
    }
}
