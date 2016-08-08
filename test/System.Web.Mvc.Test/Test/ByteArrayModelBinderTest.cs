// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Test
{
    public class ByteArrayModelBinderTest
    {
        internal const string Base64TestString = "Fys1";
        internal static readonly byte[] Base64TestBytes = new byte[] { 23, 43, 53 };

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

            ByteArrayModelBinder binder = new ByteArrayModelBinder();

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

            ByteArrayModelBinder binder = new ByteArrayModelBinder();

            // Act
            object binderResult = binder.BindModel(null, bindingContext);

            // Assert
            Assert.Null(binderResult);
        }

        [Fact]
        public void BindModelThrowsIfBindingContextIsNull()
        {
            // Arrange
            ByteArrayModelBinder binder = new ByteArrayModelBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(null, null); }, "bindingContext");
        }

        [Fact]
        public void BindModelWithBase64QuotedValueReturnsByteArray()
        {
            // Arrange
            string base64Value = Base64TestString;
            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", "\"" + base64Value + "\"" }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            ByteArrayModelBinder binder = new ByteArrayModelBinder();

            // Act
            byte[] boundValue = binder.BindModel(null, bindingContext) as byte[];

            // Assert
            Assert.Equal(Base64TestBytes, boundValue);
        }

        [Fact]
        public void BindModelWithBase64UnquotedValueReturnsByteArray()
        {
            // Arrange
            string base64Value = Base64TestString;
            SimpleValueProvider valueProvider = new SimpleValueProvider()
            {
                { "foo", base64Value }
            };

            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                ModelName = "foo",
                ValueProvider = valueProvider
            };

            ByteArrayModelBinder binder = new ByteArrayModelBinder();

            // Act
            byte[] boundValue = binder.BindModel(null, bindingContext) as byte[];

            // Assert
            Assert.Equal(Base64TestBytes, boundValue);
        }
    }
}
