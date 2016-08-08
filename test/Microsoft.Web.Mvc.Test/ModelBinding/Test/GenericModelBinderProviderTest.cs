// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class GenericModelBinderProviderTest
    {
        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelBinderFactoryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(typeof(List<>), (Func<Type[], IExtensibleModelBinder>)null); }, "modelBinderFactory");
        }

        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new GenericModelBinderProvider(typeof(List<int>), _ => null); },
                "The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type." + Environment.NewLine
              + "Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(null, _ => null); }, "modelType");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelBinderIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(typeof(List<>), (IExtensibleModelBinder)null); }, "modelBinder");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new GenericModelBinderProvider(typeof(List<int>), new MutableObjectModelBinder()); },
                "The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type." + Environment.NewLine
              + "Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(null, new MutableObjectModelBinder()); }, "modelType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeIsNotModelBinder()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new GenericModelBinderProvider(typeof(List<>), typeof(string)); },
                "The type 'System.String' does not implement the interface 'Microsoft.Web.Mvc.ModelBinding.IExtensibleModelBinder'." + Environment.NewLine
              + "Parameter name: modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(typeof(List<>), (Type)null); }, "modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeTypeArgumentMismatch()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new GenericModelBinderProvider(typeof(List<>), typeof(DictionaryModelBinder<,>)); },
                "The open model type 'System.Collections.Generic.List`1[T]' has 1 generic type argument(s), but the open binder type 'Microsoft.Web.Mvc.ModelBinding.DictionaryModelBinder`2[TKey,TValue]' has 2 generic type argument(s). The binder type must not be an open generic type or must have the same number of generic arguments as the open model type." + Environment.NewLine
              + "Parameter name: modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new GenericModelBinderProvider(typeof(List<int>), typeof(MutableObjectModelBinder)); },
                "The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type." + Environment.NewLine
              + "Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new GenericModelBinderProvider(null, typeof(MutableObjectModelBinder)); }, "modelType");
        }

        [Fact]
        public void GetBinder_TypeDoesNotMatch_ModelTypeIsInterface_ReturnsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IEnumerable<>), typeof(CollectionModelBinder<>))
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
        public void GetBinder_TypeDoesNotMatch_ModelTypeIsNotInterface_ReturnsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), typeof(CollectionModelBinder<>))
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
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), binderInstance);

            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));
            bindingContext.ValueProvider = new SimpleValueProvider();

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_Factory_ReturnsBinder()
        {
            // Arrange
            IExtensibleModelBinder binderInstance = new Mock<IExtensibleModelBinder>().Object;

            Func<Type[], IExtensibleModelBinder> binderFactory = typeArguments =>
            {
                Assert.Equal(new[] { typeof(int) }, typeArguments);
                return binderInstance;
            };

            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IList<>), binderFactory)
            {
                SuppressPrefixCheck = true
            };

            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Same(binderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_Instance_ReturnsBinder()
        {
            // Arrange
            IExtensibleModelBinder binderInstance = new Mock<IExtensibleModelBinder>().Object;

            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), binderInstance)
            {
                SuppressPrefixCheck = true
            };

            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Same(binderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_TypeActivation_ReturnsBinder()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), typeof(CollectionModelBinder<>))
            {
                SuppressPrefixCheck = true
            };

            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IExtensibleModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<CollectionModelBinder<int>>(returnedBinder);
        }

        [Fact]
        public void GetBinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IEnumerable<>), typeof(CollectionModelBinder<>));

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { provider.GetBinder(null, null); }, "bindingContext");
        }

        [Fact]
        public void GetBinderThrowsIfModelBinderHasNoParameterlessConstructor()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));
            GenericModelBinderProvider provider =
                new GenericModelBinderProvider(typeof(List<>), typeof(NoParameterlessCtorBinder<>))
                {
                    SuppressPrefixCheck = true,
                };

            // Act & Assert, confirming type name and full stack are available in Exception
            MissingMethodException exception = Assert.Throws<MissingMethodException>(
                () => provider.GetBinder(null, bindingContext),
                "No parameterless constructor defined for this object. Object type 'Microsoft.Web.Mvc.ModelBinding.Test.GenericModelBinderProviderTest+NoParameterlessCtorBinder`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]'.");
            Assert.Contains("System.Activator.CreateInstance(", exception.ToString());
        }

        private static ExtensibleModelBindingContext GetBindingContext(Type modelType)
        {
            return new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => null, modelType)
            };
        }

        private class NoParameterlessCtorBinder<T> : IExtensibleModelBinder
        {
            public NoParameterlessCtorBinder(int parameter)
            {
            }

            public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
