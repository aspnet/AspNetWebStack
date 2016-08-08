// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeMatchModelBinderTest
    {
        [Fact]
        public void BindModel_InvalidValueProviderResult_ReturnsFalse()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
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
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinder binder = new TypeMatchModelBinder();
            HttpActionContext actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Configuration = new HttpConfiguration() }
            };

            // Act
            bool retVal = binder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("theModelName"));
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderResultRawValueIncorrect_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
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
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
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
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider();

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.Null(vpResult); // No key matched
        }

        [Fact]
        public void BindModel_ThrowsArgument_IfControllerContextIsNullOnActionContext()
        {
            // Arrange
            TypeMatchModelBinder binder = new TypeMatchModelBinder();
            HttpActionContext actionContext = new HttpActionContext();
            Customer model = new Customer { Age = 99999 };
            ModelBindingContext bindingContext = GetBindingContext(typeof(Customer));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", model }
            };

            // Act
            Assert.ThrowsArgument(() => binder.BindModel(actionContext, bindingContext),
                "actionContext", "HttpActionContext.ControllerContext must not be null.");
        }

        [Fact]
        public void BindModel_ThrowsArgument_IfConfigurationIsNullOnControllerContext()
        {
            // Arrange
            TypeMatchModelBinder binder = new TypeMatchModelBinder();
            HttpActionContext actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext() };
            Customer model = new Customer { Age = 99999 };
            ModelBindingContext bindingContext = GetBindingContext(typeof(Customer));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", model }
            };

            // Act
            Assert.ThrowsArgument(() => binder.BindModel(actionContext, bindingContext),
                "actionContext", "HttpControllerContext.Configuration must not be null.");
        }

        [Fact]
        public void BindModel_Performs_Validation()
        {
            // Arrange
            TypeMatchModelBinder binder = new TypeMatchModelBinder();
            HttpActionContext actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Configuration = new HttpConfiguration() }
            };

            Customer model = new Customer { Age = 99999 };
            ModelBindingContext bindingContext = GetBindingContext(typeof(Customer));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", model }
            };
            bindingContext.ModelState = actionContext.ModelState;

            // Act
            bool retVal = binder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Same(model, bindingContext.Model);
            Assert.True(actionContext.ModelState.ContainsKey("theModelName"));
            Assert.False(actionContext.ModelState.IsValid);
            Assert.Equal("The field Age must be between 0 and 100.", actionContext.ModelState["theModelName.Age"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void BindModel_Performs_ValidationOnArrays()
        {
            // Arrange
            TypeMatchModelBinder binder = new TypeMatchModelBinder();
            HttpActionContext actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Configuration = new HttpConfiguration() }
            };

            Customer[] model = new[] { new Customer { Age = 99999 } };
            ModelBindingContext bindingContext = GetBindingContext(typeof(Customer[]));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", model }
            };
            bindingContext.ModelState = actionContext.ModelState;

            // Act
            bool retVal = binder.BindModel(actionContext, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Same(model, bindingContext.Model);
            Assert.True(actionContext.ModelState.ContainsKey("theModelName"));
            Assert.False(actionContext.ModelState.IsValid);
            Assert.Equal("The field Age must be between 0 and 100.", actionContext.ModelState["theModelName[0].Age"].Errors[0].ErrorMessage);
        }

        private static ModelBindingContext GetBindingContext()
        {
            return GetBindingContext(typeof(int));
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName",
            };
        }

        private class Customer
        {
            [Range(0, 100)]
            public int Age { get; set; }
        }
    }
}
