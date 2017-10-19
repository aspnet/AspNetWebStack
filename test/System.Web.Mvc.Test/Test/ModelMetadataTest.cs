// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class ModelMetadataTest
    {
        // Guard clauses

        [Fact]
        public void NullProviderThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(null /* provider */, null /* containerType */, null /* model */, typeof(object), null /* propertyName */),
                "provider");
        }

        [Fact]
        public void NullTypeThrows()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(provider.Object, null /* containerType */, null /* model */, null /* modelType */, null /* propertyName */),
                "modelType");
        }

        // Constructor

        [Fact]
        public void DefaultValues()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(Exception), () => "model", typeof(string), "propertyName");

            // Assert
            Assert.Equal(typeof(Exception), metadata.ContainerType);
            Assert.True(metadata.ConvertEmptyStringToNull);
            Assert.Null(metadata.DataTypeName);
            Assert.Null(metadata.Description);
            Assert.Null(metadata.DisplayFormatString);
            Assert.Null(metadata.DisplayName);
            Assert.Null(metadata.EditFormatString);
            Assert.False(metadata.HasNonDefaultEditFormat);
            Assert.False(metadata.HideSurroundingHtml);
            Assert.True(metadata.HtmlEncode);
            Assert.Equal("model", metadata.Model);
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Null(metadata.NullDisplayText);
            Assert.Equal(10000, metadata.Order);
            Assert.Equal("propertyName", metadata.PropertyName);
            Assert.False(metadata.IsReadOnly);
            Assert.True(metadata.RequestValidationEnabled);
            Assert.Null(metadata.ShortDisplayName);
            Assert.True(metadata.ShowForDisplay);
            Assert.True(metadata.ShowForEdit);
            Assert.Null(metadata.TemplateHint);
            Assert.Null(metadata.Watermark);
        }

        // IsComplexType

        struct IsComplexTypeModel
        {
        }

        [Fact]
        public void IsComplexTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Object), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IsComplexTypeModel), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<IsComplexTypeModel>), null).IsComplexType);
        }

        // IsNullableValueType

        [Fact]
        public void IsNullableValueTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsNullableValueType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsNullableValueType);
        }

        // IsRequired

        [Fact]
        public void IsRequiredTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsRequired); // Reference type not required
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsRequired); // Interface not required
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsRequired); // Nullable value type not required
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsRequired); // Value type required
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(DayOfWeek), null).IsRequired); // Enum (implicit value type) is required
        }

        // Properties

        [Fact]
        public void PropertiesCallsProvider()
        {
            // Arrange
            Type modelType = typeof(string);
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.Equal(propertyMetadata, result.ToList());
            provider.Verify();
        }

        [Fact]
        public void PropertiesUsesRealModelTypeRatherThanPassedModelType()
        {
            // Arrange
            string model = "String Value";
            Expression<Func<object, object>> accessor = _ => model;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(accessor, new ViewDataDictionary<object>());

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.Equal("Length", result.Single().PropertyName);
        }

        [Fact]
        public void PropertiesAreSortedByOrder()
        {
            // Arrange
            Type modelType = typeof(string);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>
            {
                new ModelMetadata(provider.Object, null, () => 1, typeof(int), null) { Order = 20 },
                new ModelMetadata(provider.Object, null, () => 2, typeof(int), null) { Order = 30 },
                new ModelMetadata(provider.Object, null, () => 3, typeof(int), null) { Order = 10 },
            };
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            List<ModelMetadata> result = metadata.Properties.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0].Model);
            Assert.Equal(1, result[1].Model);
            Assert.Equal(2, result[2].Model);
        }

        [Fact]
        public void PropertiesListGetsResetWhenModelGetsReset()
        { // Dev10 Bug #923263
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();
            var metadata = new ModelMetadata(provider, null, () => new Class1(), typeof(Class1), null);

            // Act
            ModelMetadata[] originalProps = metadata.Properties.ToArray();
            metadata.Model = new Class2();
            ModelMetadata[] newProps = metadata.Properties.ToArray();

            // Assert
            ModelMetadata originalProp = Assert.Single(originalProps);
            Assert.Equal(typeof(string), originalProp.ModelType);
            Assert.Equal("Prop1", originalProp.PropertyName);
            ModelMetadata newProp = Assert.Single(newProps);
            Assert.Equal(typeof(int), newProp.ModelType);
            Assert.Equal("Prop2", newProp.PropertyName);
        }

        class Class1
        {
            public string Prop1 { get; set; }
        }

        class Class2
        {
            public int Prop2 { get; set; }
        }

        // SimpleDisplayText

        [Fact]
        public void SimpleDisplayTextReturnsNullDisplayTextForNullModel()
        {
            // Arrange
            string nullText = "(null)";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null) { NullDisplayText = nullText };

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(nullText, result);
        }

        private class SimpleDisplayTextModelWithToString
        {
            public override string ToString()
            {
                return "Custom ToString Value";
            }
        }

        [Fact]
        public void SimpleDisplayTextReturnsToStringValueWhenOverridden()
        {
            // Arrange
            SimpleDisplayTextModelWithToString model = new SimpleDisplayTextModelWithToString();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(model.ToString(), result);
        }

        private class SimpleDisplayTextModelWithoutToString
        {
            public string FirstProperty { get; set; }

            public int SecondProperty { get; set; }
        }

        [Fact]
        public void SimpleDisplayTextReturnsFirstPropertyValueForNonNullModel()
        {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString
            {
                FirstProperty = "First Property Value"
            };
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(model.FirstProperty, result);
        }

        [Fact]
        public void SimpleDisplayTextReturnsFirstPropertyNullDisplayTextForNonNullModelWithNullDisplayColumnPropertyValue()
        {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString();
            EmptyModelMetadataProvider propertyProvider = new EmptyModelMetadataProvider();
            ModelMetadata propertyMetadata = propertyProvider.GetMetadataForProperty(() => model.FirstProperty, typeof(SimpleDisplayTextModelWithoutToString), "FirstProperty");
            propertyMetadata.NullDisplayText = "Null Display Text";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            provider.Setup(p => p.GetMetadataForProperties(model, typeof(SimpleDisplayTextModelWithoutToString)))
                .Returns(new[] { propertyMetadata });
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(propertyMetadata.NullDisplayText, result);
        }

        private class SimpleDisplayTextModelWithNoProperties
        {
        }

        [Fact]
        public void SimpleDisplayTextReturnsEmptyStringForNonNullModelWithNoVisibleProperties()
        {
            // Arrange
            SimpleDisplayTextModelWithNoProperties model = new SimpleDisplayTextModelWithNoProperties();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithNoProperties), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(String.Empty, result);
        }

        private class ObjectWithToStringOverride
        {
            private string _toStringValue;

            public ObjectWithToStringOverride(string toStringValue)
            {
                _toStringValue = toStringValue;
            }

            public override string ToString()
            {
                return _toStringValue;
            }
        }

        [Fact]
        public void SimpleDisplayTextReturnsToStringOfModelForNonNullModel()
        {
            // Arrange
            string toStringText = "text from ToString()";
            ObjectWithToStringOverride model = new ObjectWithToStringOverride(toStringText);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(ObjectWithToStringOverride), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(toStringText, result);
        }

        [Fact]
        public void SimpleDisplayTextReturnsEmptyStringForNonNullModelWithToStringNull()
        {
            // Arrange
            string toStringText = null;
            ObjectWithToStringOverride model = new ObjectWithToStringOverride(toStringText);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(ObjectWithToStringOverride), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(String.Empty, result);
        }

        // FromStringExpression()

        [Fact]
        public void FromStringExpressionGuardClauses()
        {
            // Null expression throws
            Assert.ThrowsArgumentNull(
                () => ModelMetadata.FromStringExpression(null, new ViewDataDictionary()),
                "expression");

            // Null view data dictionary throws
            Assert.ThrowsArgumentNull(
                () => ModelMetadata.FromStringExpression("expression", null),
                "viewData");
        }

        [Fact]
        public void FromStringExpressionEmptyExpressionReturnsExistingModelMetadata()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData.ModelMetadata = metadata;

            // Act
            ModelMetadata result = ModelMetadata.FromStringExpression(String.Empty, viewData, provider.Object);

            // Assert
            Assert.Same(metadata, result);
        }

        [Fact]
        public void FromStringExpressionItemNotFoundInViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ViewDataDictionary viewData = new ViewDataDictionary();
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Null(accessor);
                    Assert.Equal(typeof(string), type); // Don't know the type, must fall back on string
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromStringExpression("UnknownObject", viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromStringExpressionNullItemFoundAtRootOfViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["Object"] = null;
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Null(accessor());
                    Assert.Equal(typeof(string), type); // Don't know the type, must fall back on string
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromStringExpression("Object", viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromStringExpressionNonNullItemFoundAtRootOfViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            object model = new object();
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["Object"] = model;
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Same(model, accessor());
                    Assert.Equal(typeof(object), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromStringExpression("Object", viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromStringExpressionNullItemFoundOnPropertyOfItemInViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyModelContainer model = new DummyModelContainer();
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["Object"] = model;
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Null(accessor());
                    Assert.Equal(typeof(DummyModelContainer), type);
                    Assert.Equal("Model", propertyName);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromStringExpression("Object.Model", viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromStringExpressionNonNullItemFoundOnPropertyOfItemInViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyModelContainer model = new DummyModelContainer { Model = new DummyContactModel() };
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["Object"] = model;
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Same(model.Model, accessor());
                    Assert.Equal(typeof(DummyModelContainer), type);
                    Assert.Equal("Model", propertyName);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromStringExpression("Object.Model", viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromStringExpressionContainerTest()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyModelContainer model = new DummyModelContainer { Model = new DummyContactModel() };
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["Object"] = model;
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Returns<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    return new ModelMetadata(provider.Object, model.GetType(), accessor, type, propertyName);
                });

            // Act
            ModelMetadata metadata = ModelMetadata.FromStringExpression("Object.Model", viewData, provider.Object);

            // Assert
            Assert.Same(model, metadata.Container);
        }

        [Fact]
        public void FromStringExpressionWithNullModelButValidModelMetadataShouldReturnProperPropertyMetadata()
        {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(DummyContactModel));

            // Act
            ModelMetadata result = ModelMetadata.FromStringExpression("NullableIntValue", viewData);

            // Assert
            Assert.Null(result.Model);
            Assert.Equal(typeof(Nullable<int>), result.ModelType);
            Assert.Equal("NullableIntValue", result.PropertyName);
            Assert.Equal(typeof(DummyContactModel), result.ContainerType);
        }

        [Fact]
        public void FromStringExpressionValueInModelProperty()
        {
            // Arrange
            DummyContactModel model = new DummyContactModel { FirstName = "John" };
            ViewDataDictionary viewData = new ViewDataDictionary(model);

            // Act
            ModelMetadata metadata = ModelMetadata.FromStringExpression("FirstName", viewData);

            // Assert
            Assert.Equal("John", metadata.Model);
        }

        [Fact]
        public void FromStringExpressionValueInViewDataOverridesValueFromModelProperty()
        {
            // Arrange
            DummyContactModel model = new DummyContactModel { FirstName = "John" };
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            viewData["FirstName"] = "Jim";

            // Act
            ModelMetadata metadata = ModelMetadata.FromStringExpression("FirstName", viewData);

            // Assert
            Assert.Equal("Jim", metadata.Model);
            Assert.Null(metadata.Container);
        }

        // FromLambdaExpression()

        [Fact]
        public void FromLambdaExpressionGuardClauseTests()
        {
            // Null expression throws
            Assert.ThrowsArgumentNull(
                () => ModelMetadata.FromLambdaExpression<string, object>(null, new ViewDataDictionary<string>()),
                "expression");

            // Null view data throws
            Assert.ThrowsArgumentNull(
                () => ModelMetadata.FromLambdaExpression<string, object>(m => m, null),
                "viewData");

            // Unsupported expression type throws
            Assert.Throws<InvalidOperationException>(
                () => ModelMetadata.FromLambdaExpression<string, object>(m => new Object(), new ViewDataDictionary<string>()),
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [Fact]
        public void FromLambdaExpressionModelIdentityExpressionReturnsExistingModelMetadata()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);
            ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
            viewData.ModelMetadata = metadata;

            // Act
            ModelMetadata result = ModelMetadata.FromLambdaExpression<object, object>(m => m, viewData, provider.Object);

            // Assert
            Assert.Same(metadata, result);
        }

        [Fact]
        public void FromLambdaExpressionPropertyExpressionFromParameter()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel { FirstName = "Test" };
            ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Equal("Test", accessor());
                    Assert.Equal(typeof(DummyContactModel), type);
                    Assert.Equal("FirstName", propertyName);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m.FirstName, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionPropertyExpressionFromClosureValue()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel { FirstName = "Test" };
            ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Equal("Test", accessor());
                    Assert.Equal(typeof(DummyContactModel), type);
                    Assert.Equal("FirstName", propertyName);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<object, string>(m => model.FirstName, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionFieldExpressionFromParameter()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel { IntField = 42 };
            ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Equal(42, accessor());
                    Assert.Equal(typeof(int), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<DummyContactModel, int>(m => m.IntField, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionFieldExpressionFromFieldOfClosureValue()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel { IntField = 42 };
            ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Equal(42, accessor());
                    Assert.Equal(typeof(int), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<object, int>(m => model.IntField, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionFieldExpressionFromClosureValue()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel();
            ViewDataDictionary<object> viewData = new ViewDataDictionary<object>();
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Same(model, accessor());
                    Assert.Equal(typeof(DummyContactModel), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<object, DummyContactModel>(m => model, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionSingleParameterClassIndexer()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel();
            ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Equal("Indexed into 42", accessor());
                    Assert.Equal(typeof(string), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m[42], viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionSingleDimensionArrayIndex()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DummyContactModel model = new DummyContactModel { Array = new[] { 4, 8, 15, 16, 23, 42 } };
            ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>(model);
            provider.Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Callback<Func<object>, Type>((accessor, type) =>
                {
                    Assert.Equal(16, accessor());
                    Assert.Equal(typeof(int), type);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<DummyContactModel, int>(m => m.Array[3], viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionNullReferenceExceptionsInPropertyExpressionPreserveAllExpressionInformation()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ViewDataDictionary<DummyContactModel> viewData = new ViewDataDictionary<DummyContactModel>();
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Null(accessor());
                    Assert.Equal(typeof(DummyContactModel), type);
                    Assert.Equal("FirstName", propertyName);
                })
                .Returns(() => null)
                .Verifiable();

            // Act
            ModelMetadata.FromLambdaExpression<DummyContactModel, string>(m => m.FirstName, viewData, provider.Object);

            // Assert
            provider.Verify();
        }

        [Fact]
        public void FromLambdaExpressionSetsContainerTypeToDerivedMostType()
        { // Dev10 Bug #868619
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DerivedModel derivedModel = new DerivedModel();
            ViewDataDictionary<DerivedModel> viewData = new ViewDataDictionary<DerivedModel>(derivedModel);
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Null(accessor());
                    Assert.Equal(derivedModel.GetType(), type);
                    Assert.Equal("MyProperty", propertyName);
                })
                .Returns<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    return new ModelMetadata(provider.Object, derivedModel.GetType(), accessor, type, propertyName);
                })
                .Verifiable();

            // Act
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression<DerivedModel, string>(m => m.MyProperty, viewData, provider.Object);

            // Assert
            provider.Verify();
            Assert.Same(derivedModel, metadata.Container);
        }

        [Fact]
        public void FromLambdaExpressionSetsContainerWithoutObjectInViewData()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            DerivedModel derivedModel = new DerivedModel();
            ViewDataDictionary<DerivedModel> viewData = new ViewDataDictionary<DerivedModel>();
            provider.Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Callback<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    Assert.Null(accessor());
                    Assert.Equal(derivedModel.GetType(), type);
                    Assert.Equal("MyProperty", propertyName);
                })
                .Returns<Func<object>, Type, string>((accessor, type, propertyName) =>
                {
                    return new ModelMetadata(provider.Object, derivedModel.GetType(), accessor, type, propertyName);
                })
                .Verifiable();

            // Act
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression<DerivedModel, string>(m => m.MyProperty, viewData, provider.Object);

            // Assert
            provider.Verify();
            Assert.Null(metadata.Container);
        }

        private class BaseModel
        {
            public virtual string MyProperty { get; set; }
        }

        private class DerivedModel : BaseModel
        {
            [Required]
            public override string MyProperty
            {
                get { return base.MyProperty; }
                set { base.MyProperty = value; }
            }
        }

        // GetDisplayName()

        [Fact]
        public void ReturnsDisplayNameWhenSet()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName") { DisplayName = "Display Name" };

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Display Name", result);
        }

        [Fact]
        public void ReturnsPropertyNameWhenSetAndDisplayNameIsNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName");

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("PropertyName", result);
        }

        [Fact]
        public void ReturnsTypeNameWhenPropertyNameAndDisplayNameAreNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Object", result);
        }

        // Helpers

        private class DummyContactModel
        {
            public int IntField = 0;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> NullableIntValue { get; set; }
            public int[] Array { get; set; }

            public string this[int index]
            {
                get { return "Indexed into " + index; }
            }
        }

        private class DummyModelContainer
        {
            public DummyContactModel Model { get; set; }
        }
    }
}
