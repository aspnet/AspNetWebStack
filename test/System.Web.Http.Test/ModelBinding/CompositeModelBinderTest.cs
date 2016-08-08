// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class CompositeModelBinderTest
    {
        //// REVIEW: remove or activate when PropertyFilter is activated
        ////[Fact]
        ////public void BindModel_PropertyFilterIsSet_Throws()
        ////{
        ////    // Arrange
        ////    HttpExecutionContext executionContext = GetHttpExecutionContext();

        ////    ModelBindingContext bindingContext = new ModelBindingContext
        ////    {
        ////        FallbackToEmptyPrefix = true,
        ////        ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(SimpleModel)),
        ////        //PropertyFilter = (new BindAttribute { Include = "FirstName " }).IsPropertyAllowed
        ////    };

        ////    List<ModelBinderProvider> binderProviders = new List<ModelBinderProvider>();
        ////    CompositeModelBinder shimBinder = new CompositeModelBinder(binderProviders);

        ////    // Act & assert
        ////    Assert.Throws<InvalidOperationException>(
        ////        delegate { shimBinder.BindModel(executionContext, bindingContext); },
        ////        @"The new model binding system cannot be used when a property allow list or disallow list has been specified in [Bind] or via the call to UpdateModel() / TryUpdateModel(). Use the [BindRequired] and [BindNever] attributes on the model type or its properties instead.");
        ////}

        [Fact]
        public void BindModel_SuccessfulBind_RunsValidationAndReturnsModel()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext(GetHttpControllerContext());
            bool validationCalled = false;

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                //ModelState = executionContext.Controller.ViewData.ModelState,
                //PropertyFilter = _ => true,
                ValueProvider = new SimpleValueProvider
                {
                    { "someName", "dummyValue" }
                }
            };

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(actionContext, It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(HttpActionContext cc, ModelBindingContext mbc)
                    {
                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("someName", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = 42;
                        mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            //binderProviders.RegisterBinderForType(typeof(int), mockIntBinder.Object, false /* suppressPrefixCheck */);
            IModelBinder shimBinder = new CompositeModelBinder(mockIntBinder.Object);

            // Act
            bool isBound = shimBinder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(validationCalled);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_SuccessfulBind_ComplexTypeFallback_RunsValidationAndReturnsModel()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext(GetHttpControllerContext());

            bool validationCalled = false;
            List<int> expectedModel = new List<int> { 1, 2, 3, 4, 5 };

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                //ModelState = executionContext.Controller.ViewData.ModelState,
                //PropertyFilter = _ => true,
                ValueProvider = new SimpleValueProvider
                {
                    { "someOtherName", "dummyValue" }
                }
            };

            Mock<IModelBinder> mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModel(actionContext, It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate(HttpActionContext cc, ModelBindingContext mbc)
                    {
                        if (!String.IsNullOrEmpty(mbc.ModelName))
                        {
                            return false;
                        }

                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = expectedModel;
                        mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return true;
                    });

            //binderProviders.RegisterBinderForType(typeof(List<int>), mockIntBinder.Object, false /* suppressPrefixCheck */);
            IModelBinder shimBinder = new CompositeModelBinder(mockIntBinder.Object);

            // Act
            bool isBound = shimBinder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(expectedModel, bindingContext.Model);
            Assert.True(validationCalled);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_BinderFails_ReturnsNull()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext(GetHttpControllerContext());
            Mock<IModelBinder> mockListBinder = new Mock<IModelBinder>();
            mockListBinder.Setup(o => o.BindModel(actionContext, It.IsAny<ModelBindingContext>())).Returns(false).Verifiable();

            IModelBinder shimBinder = (IModelBinder)mockListBinder.Object;

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = false,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                //ModelState = executionContext.Controller.ViewData.ModelState
            };

            // Act
            bool isBound = shimBinder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.ModelState.IsValid);
            mockListBinder.Verify();
        }

        [Fact]
        public void BindModel_UnsuccessfulBind_SimpleTypeNoFallback_ReturnsNull()
        {
            // Arrange
            HttpActionContext actionContext = ContextUtil.CreateActionContext(GetHttpControllerContext());
            Mock<ModelBinderProvider> mockBinderProvider = new Mock<ModelBinderProvider>();
            mockBinderProvider.Setup(o => o.GetBinder(It.IsAny<HttpConfiguration>(), It.IsAny<Type>())).Returns((IModelBinder)null).Verifiable();
            List<ModelBinderProvider> binderProviders = new List<ModelBinderProvider>()
            {
                mockBinderProvider.Object
            };
            CompositeModelBinderProvider shimBinderProvider = new CompositeModelBinderProvider(binderProviders);
            CompositeModelBinder shimBinder = new CompositeModelBinder(shimBinderProvider.GetBinder(null, null));

            ModelBindingContext bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                //ModelState = executionContext.Controller.ViewData.ModelState
            };

            // Act
            bool isBound = shimBinder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.ModelState.IsValid);
            mockBinderProvider.Verify();
            mockBinderProvider.Verify(o => o.GetBinder(It.IsAny<HttpConfiguration>(), It.IsAny<Type>()), Times.AtMostOnce());
        }

        private static HttpControllerContext GetHttpControllerContext()
        {
            return ContextUtil.CreateControllerContext();
        }

        private class SimpleController : ApiController
        {
        }

        private class SimpleModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class SimpleValueProvider : Dictionary<string, object>, IValueProvider
        {
            private readonly CultureInfo _culture;

            public SimpleValueProvider()
                : this(null)
            {
            }

            public SimpleValueProvider(CultureInfo culture)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                _culture = culture ?? CultureInfo.InvariantCulture;
            }

            // copied from ValueProviderUtil
            public bool ContainsPrefix(string prefix)
            {
                foreach (string key in Keys)
                {
                    if (key != null)
                    {
                        if (prefix.Length == 0)
                        {
                            return true; // shortcut - non-null key matches empty prefix
                        }

                        if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (key.Length == prefix.Length)
                            {
                                return true; // exact match
                            }
                            else
                            {
                                switch (key[prefix.Length])
                                {
                                    case '.': // known separator characters
                                    case '[':
                                        return true;
                                }
                            }
                        }
                    }
                }

                return false; // nothing found
            }

            public ValueProviderResult GetValue(string key)
            {
                object rawValue;
                if (TryGetValue(key, out rawValue))
                {
                    return new ValueProviderResult(rawValue, Convert.ToString(rawValue, _culture), _culture);
                }
                else
                {
                    // value not found
                    return null;
                }
            }
        }
    }
}
