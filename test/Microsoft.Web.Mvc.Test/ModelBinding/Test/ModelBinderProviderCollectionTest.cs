// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ModelBinderProviderCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            ModelBinderProvider[] providers = new[]
            {
                new Mock<ModelBinderProvider>().Object,
                new Mock<ModelBinderProvider>().Object
            };

            // Act
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection(providers);

            // Assert
            Assert.Equal(providers, collection.ToArray());
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void AddNullProviderThrows()
        {
            // Arrange
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void RegisterBinderForGenericType_Factory()
        {
            // Arrange
            ModelBinderProvider mockProvider = new Mock<ModelBinderProvider>().Object;
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                mockProvider
            };

            // Act
            collection.RegisterBinderForGenericType(typeof(List<>), _ => mockBinder);

            // Assert
            var genericProvider = Assert.IsType<GenericModelBinderProvider>(collection[0]);
            Assert.Equal(typeof(List<>), genericProvider.ModelType);
            Assert.Equal(mockProvider, collection[1]);
        }

        [Fact]
        public void RegisterBinderForGenericType_Instance()
        {
            // Arrange
            ModelBinderProvider mockProvider = new Mock<ModelBinderProvider>().Object;
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                mockProvider
            };

            // Act
            collection.RegisterBinderForGenericType(typeof(List<>), mockBinder);

            // Assert
            var genericProvider = Assert.IsType<GenericModelBinderProvider>(collection[0]);
            Assert.Equal(typeof(List<>), genericProvider.ModelType);
            Assert.Equal(mockProvider, collection[1]);
        }

        [Fact]
        public void RegisterBinderForGenericType_Type()
        {
            // Arrange
            ModelBinderProvider mockProvider = new Mock<ModelBinderProvider>().Object;
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                mockProvider
            };

            // Act
            collection.RegisterBinderForGenericType(typeof(List<>), typeof(CollectionModelBinder<>));

            // Assert
            var genericProvider = Assert.IsType<GenericModelBinderProvider>(collection[0]);
            Assert.Equal(typeof(List<>), genericProvider.ModelType);
            Assert.Equal(mockProvider, collection[1]);
        }

        [Fact]
        public void RegisterBinderForType_Factory()
        {
            // Arrange
            ModelBinderProvider mockProvider = new Mock<ModelBinderProvider>().Object;
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                mockProvider
            };

            // Act
            collection.RegisterBinderForType(typeof(int), () => mockBinder);

            // Assert
            var simpleProvider = Assert.IsType<SimpleModelBinderProvider>(collection[0]);
            Assert.Equal(typeof(int), simpleProvider.ModelType);
            Assert.Equal(mockProvider, collection[1]);
        }

        [Fact]
        public void RegisterBinderForType_Instance()
        {
            // Arrange
            ModelBinderProvider mockProvider = new Mock<ModelBinderProvider>().Object;
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                mockProvider
            };

            // Act
            collection.RegisterBinderForType(typeof(int), mockBinder);

            // Assert
            var simpleProvider = Assert.IsType<SimpleModelBinderProvider>(collection[0]);
            Assert.Equal(typeof(int), simpleProvider.ModelType);
            Assert.Equal(mockProvider, collection[1]);
        }

        [Fact]
        public void RegisterBinderForType_Instance_InsertsNewProviderBehindFrontOfListProviders()
        {
            // Arrange
            ModelBinderProvider frontOfListProvider = new ProviderAtFront();
            IExtensibleModelBinder mockBinder = new Mock<IExtensibleModelBinder>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection
            {
                frontOfListProvider
            };

            // Act
            collection.RegisterBinderForType(typeof(int), mockBinder);

            // Assert
            Assert.Equal(
                new[] { typeof(ProviderAtFront), typeof(SimpleModelBinderProvider) },
                collection.Select(o => o.GetType()).ToArray());
        }

        [Fact]
        public void SetItem()
        {
            // Arrange
            ModelBinderProvider provider0 = new Mock<ModelBinderProvider>().Object;
            ModelBinderProvider provider1 = new Mock<ModelBinderProvider>().Object;
            ModelBinderProvider provider2 = new Mock<ModelBinderProvider>().Object;

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();
            collection.Add(provider0);
            collection.Add(provider1);

            // Act
            collection[1] = provider2;

            // Assert
            Assert.Equal(new[] { provider0, provider2 }, collection.ToArray());
        }

        [Fact]
        public void SetNullProviderThrows()
        {
            // Arrange
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();
            collection.Add(new Mock<ModelBinderProvider>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }

        [Fact]
        public void GetBinder_FromAttribute_BadAttribute_Throws()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_BadAttribute))
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { providers.GetBinder(controllerContext, bindingContext); },
                @"The type 'System.Object' does not subclass Microsoft.Web.Mvc.ModelBinding.ModelBinderProvider or implement the interface Microsoft.Web.Mvc.ModelBinding.IExtensibleModelBinder.");
        }

        [Fact]
        public void GetBinder_FromAttribute_Binder_Generic_ReturnsBinder()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_Binder_Generic<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", "fooValue" }
                }
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();
            providers.RegisterBinderForType(typeof(ModelWithProviderAttribute_Binder_Generic<int>), new Mock<IExtensibleModelBinder>().Object, true /* suppressPrefix */);

            // Act
            IExtensibleModelBinder binder = providers.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.IsType<CustomGenericBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_FromAttribute_Binder_SuppressPrefixCheck_ReturnsBinder()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_Binder_SuppressPrefix)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "bar", "barValue" }
                }
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();
            providers.RegisterBinderForType(typeof(ModelWithProviderAttribute_Binder_SuppressPrefix), new Mock<IExtensibleModelBinder>().Object, true /* suppressPrefix */);

            // Act
            IExtensibleModelBinder binder = providers.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.IsType<CustomBinder>(binder);
        }

        [Fact]
        public void GetBinder_FromAttribute_Binder_ValueNotPresent_ReturnsNull()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_Binder)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "bar", "barValue" }
                }
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();
            providers.RegisterBinderForType(typeof(ModelWithProviderAttribute_Binder), new Mock<IExtensibleModelBinder>().Object, true /* suppressPrefix */);

            // Act
            IExtensibleModelBinder binder = providers.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_FromAttribute_Binder_ValuePresent_ReturnsBinder()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_Binder)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", "fooValue" }
                }
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();
            providers.RegisterBinderForType(typeof(ModelWithProviderAttribute_Binder), new Mock<IExtensibleModelBinder>().Object, true /* suppressPrefix */);

            // Act
            IExtensibleModelBinder binder = providers.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.IsType<CustomBinder>(binder);
        }

        [Fact]
        public void GetBinder_FromAttribute_Provider_ReturnsBinder()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithProviderAttribute_Provider))
            };

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection();
            providers.RegisterBinderForType(typeof(ModelWithProviderAttribute_Provider), new Mock<IExtensibleModelBinder>().Object, true /* suppressPrefix */);

            // Act
            IExtensibleModelBinder binder = providers.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.IsType<CustomBinder>(binder);
        }

        [Fact]
        public void GetBinderReturnsFirstBinderFromProviders()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object))
            };
            IExtensibleModelBinder expectedBinder = new Mock<IExtensibleModelBinder>().Object;

            Mock<ModelBinderProvider> mockProvider = new Mock<ModelBinderProvider>();
            mockProvider.Setup(p => p.GetBinder(controllerContext, bindingContext)).Returns(expectedBinder);

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection(new[]
            {
                new Mock<ModelBinderProvider>().Object,
                mockProvider.Object,
                new Mock<ModelBinderProvider>().Object
            });

            // Act
            IExtensibleModelBinder returned = collection.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.Equal(expectedBinder, returned);
        }

        [Fact]
        public void GetBinderReturnsNullIfNoProviderMatches()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object))
            };

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection(new[]
            {
                new Mock<ModelBinderProvider>().Object,
            });

            // Act
            IExtensibleModelBinder returned = collection.GetBinder(controllerContext, bindingContext);

            // Assert
            Assert.Null(returned);
        }

        [Fact]
        public void GetBinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { collection.GetBinder(new ControllerContext(), null); }, "bindingContext");
        }

        [Fact]
        public void GetBinderThrowsIfControllerContextIsNull()
        {
            // Arrange
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { collection.GetBinder(null, new ExtensibleModelBindingContext()); }, "controllerContext");
        }

        [Fact]
        public void GetBinderThrowsIfModelTypeHasBindAttribute()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ModelWithBindAttribute))
            };
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { collection.GetBinder(controllerContext, bindingContext); },
                @"The model of type 'Microsoft.Web.Mvc.ModelBinding.Test.ModelBinderProviderCollectionTest+ModelWithBindAttribute' has a [Bind] attribute. The new model binding system cannot be used with models that have type-level [Bind] attributes. Use the [BindRequired] and [BindNever] attributes on the model type or its properties instead.");
        }

        [Fact]
        public void GetBinderThrowsIfBinderHasNoParameterlessConstructor()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(
                    null,
                    typeof(ModelWithProviderAttribute_ProviderHasNoParameterlessConstructor)),
            };

            // Act & Assert, confirming type name and full stack are available in Exception
            MissingMethodException exception = Assert.Throws<MissingMethodException>(
                () => collection.GetBinder(controllerContext, bindingContext),
                "No parameterless constructor defined for this object. Object type 'Microsoft.Web.Mvc.ModelBinding.Test.ModelBinderProviderCollectionTest+NoParameterlessCtorProvider'.");
            Assert.Contains("System.Activator.CreateInstance(", exception.ToString());
        }

        [Fact]
        public void GetBinderThrowsIfGenericProviderHasNoParameterlessConstructor()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ModelBinderProviderCollection collection = new ModelBinderProviderCollection();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(
                    null,
                    typeof(ModelWithProviderAttribute_ProviderHasNoParameterlessConstructor<int>)),
            };

            // Act & Assert, confirming type name and full stack are available in Exception
            MissingMethodException exception = Assert.Throws<MissingMethodException>(
                () => collection.GetBinder(controllerContext, bindingContext),
                "No parameterless constructor defined for this object. Object type 'Microsoft.Web.Mvc.ModelBinding.Test.ModelBinderProviderCollectionTest+NoParameterlessCtorBinder`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]'.");
            Assert.Contains("System.Activator.CreateInstance(", exception.ToString());
        }

        [Fact]
        public void GetRequiredBinderThrowsIfNoProviderMatches()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int))
            };

            ModelBinderProviderCollection collection = new ModelBinderProviderCollection(new[]
            {
                new Mock<ModelBinderProvider>().Object,
            });

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { collection.GetRequiredBinder(controllerContext, bindingContext); },
                @"A binder for type System.Int32 could not be located.");
        }

        [MetadataType(typeof(ModelWithBindAttribute_Buddy))]
        private class ModelWithBindAttribute
        {
            [Bind]
            private class ModelWithBindAttribute_Buddy
            {
            }
        }

        [ModelBinderProviderOptions(FrontOfList = true)]
        private class ProviderAtFront : ModelBinderProvider
        {
            public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        [ExtensibleModelBinder(typeof(object))]
        private class ModelWithProviderAttribute_BadAttribute
        {
        }

        [ExtensibleModelBinder(typeof(CustomBinder))]
        private class ModelWithProviderAttribute_Binder
        {
        }

        [ExtensibleModelBinder(typeof(CustomGenericBinder<>))]
        private class ModelWithProviderAttribute_Binder_Generic<T>
        {
        }

        [ExtensibleModelBinder(typeof(CustomBinder), SuppressPrefixCheck = true)]
        private class ModelWithProviderAttribute_Binder_SuppressPrefix
        {
        }

        [ExtensibleModelBinder(typeof(CustomProvider))]
        private class ModelWithProviderAttribute_Provider
        {
        }

        [ExtensibleModelBinder(typeof(NoParameterlessCtorProvider))]
        private class ModelWithProviderAttribute_ProviderHasNoParameterlessConstructor
        {
        }

        [ExtensibleModelBinder(typeof(NoParameterlessCtorBinder<>))]
        private class ModelWithProviderAttribute_ProviderHasNoParameterlessConstructor<T>
        {
        }

        private class CustomProvider : ModelBinderProvider
        {
            public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                return new CustomBinder();
            }
        }

        private class CustomBinder : IExtensibleModelBinder
        {
            public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class CustomGenericBinder<T> : IExtensibleModelBinder
        {
            public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class NoParameterlessCtorProvider : ModelBinderProvider
        {
            public NoParameterlessCtorProvider(int parameter)
            {
            }

            public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
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
