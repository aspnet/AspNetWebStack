// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class TypeMatchModelBinderTest
    {
        [Fact]
        public void BindModel_InvalidValueProviderResult_ReturnsFalse()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeMatchModelBinder binder = new TypeMatchModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsTrue()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinder binder = new TypeMatchModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("theModelName"));
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderResultRawValueIncorrect_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", "not an integer" }
            };

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.Null(vpResult); // Raw value is the wrong type
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderResultValid_ReturnsValueProviderResult()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider
            {
                { "theModelName", 42 }
            };

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.NotNull(vpResult);
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleValueProvider();

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.Null(vpResult); // No key matched
        }

        private static ExtensibleModelBindingContext GetBindingContext()
        {
            return GetBindingContext(typeof(int));
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName"
            };
        }
    }
}
