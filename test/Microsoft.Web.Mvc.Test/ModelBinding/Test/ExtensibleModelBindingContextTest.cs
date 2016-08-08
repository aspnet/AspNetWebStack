// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ExtensibleModelBindingContextTest
    {
        [Fact]
        public void CopyConstructor()
        {
            // Arrange
            ExtensibleModelBindingContext originalBindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object)),
                ModelName = "theName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleValueProvider()
            };

            // Act
            ExtensibleModelBindingContext newBindingContext = new ExtensibleModelBindingContext(originalBindingContext);

            // Assert
            Assert.Null(newBindingContext.ModelMetadata);
            Assert.Equal("", newBindingContext.ModelName);
            Assert.Equal(originalBindingContext.ModelState, newBindingContext.ModelState);
            Assert.Equal(originalBindingContext.ValueProvider, newBindingContext.ValueProvider);
        }

        [Fact]
        public void ModelBinderProvidersProperty()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext();

            // Act & assert            
            MemberHelper.TestPropertyWithDefaultInstance(bindingContext, "ModelBinderProviders", new ModelBinderProviderCollection(), ModelBinderProviders.Providers);
        }

        [Fact]
        public void ModelProperty()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int))
            };

            // Act & assert            
            MemberHelper.TestPropertyValue(bindingContext, "Model", 42);
        }

        [Fact]
        public void ModelProperty_ThrowsIfModelMetadataDoesNotExist()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { bindingContext.Model = null; },
                "The ModelMetadata property must be set before accessing this property.");
        }

        [Fact]
        public void ModelNameProperty()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext();

            // Act & assert
            Assert.Reflection.StringProperty(bindingContext, (context) => context.ModelName, String.Empty);
        }

        [Fact]
        public void ModelStateProperty()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext();
            ModelStateDictionary modelState = new ModelStateDictionary();

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(bindingContext, "ModelState", modelState);
        }

        [Fact]
        public void ModelAndModelTypeAreFedFromModelMetadata()
        {
            // Act
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => 42, typeof(int))
            };

            // Assert
            Assert.Equal(42, bindingContext.Model);
            Assert.Equal(typeof(int), bindingContext.ModelType);
        }

        [Fact]
        public void ValidationNodeProperty()
        {
            // Act
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => 42, typeof(int))
            };

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(bindingContext, "ValidationNode", new ModelValidationNode(bindingContext.ModelMetadata, "someName"));
        }

        [Fact]
        public void ValidationNodeProperty_DefaultValues()
        {
            // Act
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => 42, typeof(int)),
                ModelName = "theInt"
            };

            // Act
            ModelValidationNode validationNode = bindingContext.ValidationNode;

            // Assert
            Assert.NotNull(validationNode);
            Assert.Equal(bindingContext.ModelMetadata, validationNode.ModelMetadata);
            Assert.Equal(bindingContext.ModelName, validationNode.ModelStateKey);
        }
    }
}
