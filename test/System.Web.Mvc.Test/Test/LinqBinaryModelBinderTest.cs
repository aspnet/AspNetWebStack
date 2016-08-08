// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Linq;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Test
{
    public class LinqBinaryModelBinderTest
    {
        [Fact]
        public void BindModelWithNonExistentValueReturnsNull()
        {
            // Arrange
            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", null }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            LinqBinaryModelBinder binder = new LinqBinaryModelBinder();

            // Act
            object binderResult = binder.BindModel(null, bindingContext);

            // Assert
            Assert.Null(binderResult);
        }

        [Fact]
        public void BinderWithEmptyStringValueReturnsNull()
        {
            // Arrange
            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", "" }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            LinqBinaryModelBinder binder = new LinqBinaryModelBinder();

            // Act
            object binderResult = binder.BindModel(null, bindingContext);

            // Assert
            Assert.Null(binderResult);
        }

        [Fact]
        public void BindModelThrowsIfBindingContextIsNull()
        {
            // Arrange
            LinqBinaryModelBinder binder = new LinqBinaryModelBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(null, null); }, "bindingContext");
        }

        [Fact]
        public void BindModelWithBase64QuotedValueReturnsBinary()
        {
            // Arrange
            string base64Value = ByteArrayModelBinderTest.Base64TestString;

            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", "\"" + base64Value + "\"" }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            LinqBinaryModelBinder binder = new LinqBinaryModelBinder();

            // Act
            Binary boundValue = binder.BindModel(null, bindingContext) as Binary;

            // Assert
            Assert.Equal(ByteArrayModelBinderTest.Base64TestBytes, boundValue);
        }

        [Fact]
        public void BindModelWithBase64UnquotedValueReturnsBinary()
        {
            // Arrange
            string base64Value = ByteArrayModelBinderTest.Base64TestString;
            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", base64Value }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            LinqBinaryModelBinder binder = new LinqBinaryModelBinder();

            // Act
            Binary boundValue = binder.BindModel(null, bindingContext) as Binary;

            // Assert
            Assert.Equal(ByteArrayModelBinderTest.Base64TestBytes, boundValue);
        }
    }
}
