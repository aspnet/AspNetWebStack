// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class ModelBinderAttributeTest
    {
        [Fact]
        public void Empty_BinderType()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ModelBinderProvider), new CustomModelBinderProvider());

            ModelBinderAttribute attr = new ModelBinderAttribute();

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<CustomModelBinderProvider>(provider);
        }

        [Fact]
        public void Illegal_BinderType()
        {
            // Given an illegal type.
            // Constructor shouldn't throw. But trying to instantiate the model binder provider will throw.
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(object));

            Assert.Equal(typeof(object), attr.BinderType); // can still lookup illegal type
            Assert.Throws<InvalidOperationException>(
                () => attr.GetModelBinderProvider(config)
            );
        }

        [Fact]
        public void BinderType_Provided()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<CustomModelBinderProvider>(provider);
        }

        [Fact]
        public void BinderType_From_DependencyResolver()
        {
            // To test dependency resolver, the registered type and actual type should be different.
            HttpConfiguration config = new HttpConfiguration();
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider)))
                               .Returns(new SecondCustomModelBinderProvider());
            config.DependencyResolver = mockDependencyResolver.Object;

            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

            ModelBinderProvider provider = attr.GetModelBinderProvider(config);
            Assert.IsType<SecondCustomModelBinderProvider>(provider);
        }

        [Fact]
        public void BinderType_From_DependencyResolver_ReleasedWhenConfigIsDisposed()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            SecondCustomModelBinderProvider provider = new SecondCustomModelBinderProvider();
            mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider))).Returns(provider);
            config.DependencyResolver = mockDependencyResolver.Object;

            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));
            attr.GetModelBinderProvider(config);

            // Act
            config.Dispose();

            // Assert
            mockDependencyResolver.Verify(dr => dr.Dispose(), Times.Once());
        }

        [Fact]
        public void Set_ModelBinder_And_ValueProviders()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ValueProviderAttribute(typeof(CustomValueProviderFactory)) { BinderType = typeof(CustomModelBinderProvider) };
            IEnumerable<ValueProviderFactory> vpfs = attr.GetValueProviderFactories(config);

            Assert.IsType<CustomModelBinderProvider>(attr.GetModelBinderProvider(config));
            object valueProviderFactory = Assert.Single(vpfs);
            Assert.IsType<CustomValueProviderFactory>(valueProviderFactory);
        }

        [Fact]
        public void Get_ModelBinder_From_Empty_Attribute()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ModelBinderProvider), new CustomModelBinderProvider());

            // binder = null, so pulls default from config. But attribute still has value by specifying the value providers.
            ModelBinderAttribute attr = new ValueProviderAttribute(typeof(CustomValueProviderFactory));

            // Act
            IModelBinder binder = attr.GetModelBinder(config, null);

            // Assert
            Assert.Null(attr.BinderType); // using the default
            Assert.NotNull(binder);
            Assert.IsType<CustomModelBinder>(binder);
        }

        [Fact]
        public void Get_ModelBinder_From_Binder()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ModelBinderAttribute { BinderType = typeof(CustomModelBinder) };

            // Act
            IModelBinder binder = attr.GetModelBinder(config, null);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<CustomModelBinder>(binder);
        }

        [Fact]
        public void Get_ModelBinder_From_BinderProvider()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelBinderAttribute attr = new ModelBinderAttribute { BinderType = typeof(CustomModelBinderProvider) };

            // Act
            IModelBinder binder = attr.GetModelBinder(config, null);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<CustomModelBinder>(binder);
        }

        private class CustomModelBinderProvider : ModelBinderProvider
        {
            public override IModelBinder GetBinder(HttpConfiguration config, Type modelType)
            {
                return new CustomModelBinder();
            }
        }

        private class CustomModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                return true;
            }
        }

        private class SecondCustomModelBinderProvider : ModelBinderProvider
        {
            public override IModelBinder GetBinder(HttpConfiguration config, Type modelType)
            {
                throw new NotImplementedException();
            }
        }

        private class CustomValueProviderFactory : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
