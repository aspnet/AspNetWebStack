// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class SimpleModelBinderProviderTest
    {
        [Fact]
        public void ConstructorWithFactoryThrowsIfModelBinderFactoryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new SimpleModelBinderProvider(typeof(object), (Func<IExtensibleModelBinder>)null); }, "modelBinderFactory");
        }

        [Fact]
        public void ConstructorWithFactoryThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new SimpleModelBinderProvider(null, () => null); }, "modelType");
        }

        [Fact]
        public void ConstructorWithInstanceThrowsIfModelBinderIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new SimpleModelBinderProvider(typeof(object), (IExtensibleModelBinder)null); }, "modelBinder");
        }

        [Fact]
        public void ConstructorWithInstanceThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new SimpleModelBinderProvider(null, new Mock<IExtensibleModelBinder>().Object); }, "modelType");
        }

        [Fact]
        public void GetBinder_TypeDoesNotMatch_ReturnsNull()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IExtensibleModelBinder>().Object)
            {
                SuppressPrefixCheck = true
            };
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IExtensibleModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixNotFound_ReturnsNull()
        {
            // Arrange
            IExtensibleModelBinder binderInstance = new Mock<IExtensibleModelBinder>().Object;
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), binderInstance);

            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(string));
            bindingContext.ValueProvider = new SimpleValueProvider();

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixSuppressed_ReturnsFactoryInstance()
        {
            // Arrange
            int numExecutions = 0;
            IExtensibleModelBinder theBinderInstance = new Mock<IExtensibleModelBinder>().Object;
            Func<IExtensibleModelBinder> factory = delegate
            {
                numExecutions++;
                return theBinderInstance;
            };

            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), factory)
            {
                SuppressPrefixCheck = true
            };
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(string));

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);
            returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Equal(2, numExecutions);
            Assert.Equal(theBinderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixSuppressed_ReturnsInstance()
        {
            // Arrange
            IExtensibleModelBinder theBinderInstance = new Mock<IExtensibleModelBinder>().Object;
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), theBinderInstance)
            {
                SuppressPrefixCheck = true
            };
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(string));

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Equal(theBinderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IExtensibleModelBinder>().Object);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { provider.GetBinder(null, null); }, "bindingContext");
        }

        [Fact]
        public void ModelTypeProperty()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IExtensibleModelBinder>().Object);

            // Act & assert
            Assert.Equal(typeof(string), provider.ModelType);
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => null, modelType)
            };
        }
    }
}
