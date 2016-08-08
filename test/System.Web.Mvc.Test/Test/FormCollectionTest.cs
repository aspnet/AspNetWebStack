// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FormCollectionTest
    {
        [Fact]
        public void ConstructorCopiesProvidedCollection()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection()
            {
                { "foo", "fooValue" },
                { "bar", "barValue" }
            };

            // Act
            FormCollection formCollection = new FormCollection(nvc);

            // Assert
            Assert.Equal(2, formCollection.Count);
            Assert.Equal("fooValue", formCollection["foo"]);
            Assert.Equal("barValue", formCollection["bar"]);
        }

        [Fact]
        public void ConstructorThrowsIfCollectionIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new FormCollection(null); }, "collection");
        }

        [Fact]
        public void ConstructorUsesValidatedValuesWhenControllerIsNull()
        {
            // Arrange
            var values = new NameValueCollection()
            {
                { "foo", "fooValue" },
                { "bar", "barValue" }
            };

            // Act
            var result = new FormCollection(controller: null,
                                            validatedValuesThunk: () => values,
                                            unvalidatedValuesThunk: () => { throw new NotImplementedException(); });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("fooValue", result["foo"]);
            Assert.Equal("barValue", result["bar"]);
        }

        [Fact]
        public void ConstructorUsesValidatedValuesWhenControllerValidateRequestIsTrue()
        {
            // Arrange
            var values = new NameValueCollection()
            {
                { "foo", "fooValue" },
                { "bar", "barValue" }
            };
            var controller = new Mock<ControllerBase>().Object;
            controller.ValidateRequest = true;

            // Act
            var result = new FormCollection(controller,
                                            validatedValuesThunk: () => values,
                                            unvalidatedValuesThunk: () => { throw new NotImplementedException(); });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("fooValue", result["foo"]);
            Assert.Equal("barValue", result["bar"]);
        }

        [Fact]
        public void ConstructorUsesUnvalidatedValuesWhenControllerValidateRequestIsFalse()
        {
            // Arrange
            var values = new NameValueCollection()
            {
                { "foo", "fooValue" },
                { "bar", "barValue" }
            };
            var controller = new Mock<ControllerBase>().Object;
            controller.ValidateRequest = false;

            // Act
            var result = new FormCollection(controller,
                                            validatedValuesThunk: () => { throw new NotImplementedException(); },
                                            unvalidatedValuesThunk: () => values);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("fooValue", result["foo"]);
            Assert.Equal("barValue", result["bar"]);
        }

        [Fact]
        public void CustomBinderBindModelReturnsFormCollection()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection() { { "foo", "fooValue" }, { "bar", "barValue" } };
            IModelBinder binder = ModelBinders.Binders.GetBinder(typeof(FormCollection));

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Form).Returns(nvc);

            // Act
            FormCollection formCollection = (FormCollection)binder.BindModel(mockControllerContext.Object, null);

            // Assert
            Assert.NotNull(formCollection);
            Assert.Equal(2, formCollection.Count);
            Assert.Equal("fooValue", nvc["foo"]);
            Assert.Equal("barValue", nvc["bar"]);
        }

        [Fact]
        public void CustomBinderBindModelThrowsIfControllerContextIsNull()
        {
            // Arrange
            IModelBinder binder = ModelBinders.Binders.GetBinder(typeof(FormCollection));

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(null, null); }, "controllerContext");
        }

        [Fact]
        public void GetValue_ThrowsIfNameIsNull()
        {
            // Arrange
            FormCollection formCollection = new FormCollection();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { formCollection.GetValue(null); }, "name");
        }

        [Fact]
        public void GetValue_KeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            FormCollection formCollection = new FormCollection();

            // Act
            ValueProviderResult vpResult = formCollection.GetValue("");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_KeyExists_ReturnsResult()
        {
            // Arrange
            FormCollection formCollection = new FormCollection()
            {
                { "foo", "1" },
                { "foo", "2" }
            };

            // Act
            ValueProviderResult vpResult = formCollection.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new[] { "1", "2" }, (string[])vpResult.RawValue);
            Assert.Equal("1,2", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult.Culture);
        }
    }
}
