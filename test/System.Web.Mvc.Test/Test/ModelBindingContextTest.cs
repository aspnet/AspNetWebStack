// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class ModelBindingContextTest
    {
        [Fact]
        public void CopyConstructor()
        {
            // Arrange
            ModelBindingContext originalBindingContext = new ModelBindingContext()
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object)),
                ModelName = "theName",
                ModelState = new ModelStateDictionary(),
                PropertyFilter = _ => false,
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            ModelBindingContext newBindingContext = new ModelBindingContext(originalBindingContext);

            // Assert
            Assert.False(newBindingContext.FallbackToEmptyPrefix);
            Assert.Null(newBindingContext.ModelMetadata);
            Assert.Equal("", newBindingContext.ModelName);
            Assert.Equal(originalBindingContext.ModelState, newBindingContext.ModelState);
            Assert.True(newBindingContext.PropertyFilter("foo"));
            Assert.Equal(originalBindingContext.ValueProvider, newBindingContext.ValueProvider);
        }

        [Fact]
        public void ModelNameProperty()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();

            // Act & assert
            MemberHelper.TestStringProperty(bindingContext, "ModelName", String.Empty);
        }

        [Fact]
        public void ModelStateProperty()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();
            ModelStateDictionary modelState = new ModelStateDictionary();

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(bindingContext, "ModelState", modelState);
        }

        [Fact]
        public void PropertyFilterPropertyDefaultInstanceReturnsTrueForAnyInput()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();

            // Act
            Predicate<string> propertyFilter = bindingContext.PropertyFilter;

            // Assert
            // We can't test all inputs, but at least this gives us high confidence that we ignore the parameter by default
            Assert.True(propertyFilter(null));
            Assert.True(propertyFilter(String.Empty));
            Assert.True(propertyFilter("Foo"));
        }

        [Fact]
        public void PropertyFilterPropertyReturnsDefaultInstance()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();
            Predicate<string> propertyFilter = _ => true;

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(bindingContext, "PropertyFilter", propertyFilter);
        }

        [Fact]
        public void ModelAndModelTypeAreFedFromModelMetadata()
        {
            // Act
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => 42, typeof(int))
            };

            // Assert
            Assert.Equal(42, bindingContext.Model);
            Assert.Equal(typeof(int), bindingContext.ModelType);
        }

        [Fact]
        public void ModelIsNotSettable()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => bindingContext.Model = "foo",
                "This property setter is obsolete, because its value is derived from ModelMetadata.Model now.");
        }

        [Fact]
        public void ModelTypeIsNotSettable()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => bindingContext.ModelType = typeof(string),
                "This property setter is obsolete, because its value is derived from ModelMetadata.Model now.");
        }
    }
}
