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
    public class ExtensibleModelBinderAdapterTest
    {
        [Fact]
        public void BindModel_PropertyFilterIsSet_Throws()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(SimpleModel)),
                PropertyFilter = (new BindAttribute { Include = "FirstName " }).IsPropertyAllowed
            };

            ModelBinderProviderCollection binderProviders = new ModelBinderProviderCollection();
            ExtensibleModelBinderAdapter shimBinder = new ExtensibleModelBinderAdapter(binderProviders);

            // Act & assert

            Assert.Throws<InvalidOperationException>(
                delegate { shimBinder.BindModel(controllerContext, bindingContext); },
                @"The new model binding system cannot be used when a property whitelist or blacklist has been specified in [Bind] or via the call to UpdateModel() / TryUpdateModel(). Use the [BindRequired] and [BindNever] attributes on the model type or its properties instead.");
        }

        [Fact]
        public void BindModel_SuccessfulBind_RunsValidationAndReturnsModel()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            bool validationCalled = false;

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = controllerContext.Controller.ViewData.ModelState,
                PropertyFilter = _ => true,
                ValueProvider = new SimpleValueProvider
                {
                    { "someName", "dummyValue" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("someName", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = 42;
                        mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            ModelBinderProviderCollection binderProviders = new ModelBinderProviderCollection();
            binderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, false /* suppressPrefixCheck */);
            ExtensibleModelBinderAdapter shimBinder = new ExtensibleModelBinderAdapter(binderProviders);

            // Act
            object retVal = shimBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal(42, retVal);
            Assert.True(validationCalled);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_SuccessfulBind_ComplexTypeFallback_RunsValidationAndReturnsModel()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();

            bool validationCalled = false;
            List<int> expectedModel = new List<int> { 1, 2, 3, 4, 5 };

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                ModelState = controllerContext.Controller.ViewData.ModelState,
                PropertyFilter = _ => true,
                ValueProvider = new SimpleValueProvider
                {
                    { "someOtherName", "dummyValue" }
                }
            };

            Mock<IExtensibleModelBinder> mockIntBinder = new Mock<IExtensibleModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>()))
                .Returns(
                    delegate(ControllerContext cc, ExtensibleModelBindingContext mbc)
                    {
                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = expectedModel;
                        mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            ModelBinderProviderCollection binderProviders = new ModelBinderProviderCollection();
            binderProviders.RegisterBinderForType(typeof(List<int>), mockIntBinder.Object, false /* suppressPrefixCheck */);
            ExtensibleModelBinderAdapter shimBinder = new ExtensibleModelBinderAdapter(binderProviders);

            // Act
            object retVal = shimBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Equal(expectedModel, retVal);
            Assert.True(validationCalled);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_BinderFails_ReturnsNull()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            Mock<IExtensibleModelBinder> mockListBinder = new Mock<IExtensibleModelBinder>();
            mockListBinder.Setup(o => o.BindModel(controllerContext, It.IsAny<ExtensibleModelBindingContext>())).Returns(false).Verifiable();

            ModelBinderProviderCollection binderProviders = new ModelBinderProviderCollection();
            binderProviders.RegisterBinderForType(typeof(List<int>), mockListBinder.Object, true /* suppressPrefixCheck */);
            ExtensibleModelBinderAdapter shimBinder = new ExtensibleModelBinderAdapter(binderProviders);

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = false,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelState = controllerContext.Controller.ViewData.ModelState
            };

            // Act
            object retVal = shimBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Null(retVal);
            Assert.True(bindingContext.ModelState.IsValid);
            mockListBinder.Verify();
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_SimpleTypeNoFallback_ReturnsNull()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            Mock<ModelBinderProvider> mockBinderProvider = new Mock<ModelBinderProvider>();
            mockBinderProvider.Setup(o => o.GetBinder(controllerContext, It.IsAny<ExtensibleModelBindingContext>())).Returns((IExtensibleModelBinder)null).Verifiable();
            ModelBinderProviderCollection binderProviders = new ModelBinderProviderCollection
            {
                mockBinderProvider.Object
            };
            ExtensibleModelBinderAdapter shimBinder = new ExtensibleModelBinderAdapter(binderProviders);

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new DataAnnotationsModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelState = controllerContext.Controller.ViewData.ModelState
            };

            // Act
            object retVal = shimBinder.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.Null(retVal);
            Assert.True(bindingContext.ModelState.IsValid);
            mockBinderProvider.Verify();
            mockBinderProvider.Verify(o => o.GetBinder(controllerContext, It.IsAny<ExtensibleModelBindingContext>()), Times.AtMostOnce());
        }

        private static ControllerContext GetControllerContext()
        {
            return new ControllerContext
            {
                Controller = new SimpleController()
            };
        }

        private class SimpleController : Controller
        {
        }

        private class SimpleModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
