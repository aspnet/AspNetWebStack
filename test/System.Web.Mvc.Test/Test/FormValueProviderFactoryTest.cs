// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class FormValueProviderFactoryTest
    {
        private static readonly NameValueCollection _backingStore = new NameValueCollection()
        {
            { "foo", "fooValue" }
        };

        private static readonly NameValueCollection _unvalidatedBackingStore = new NameValueCollection()
        {
            { "foo", "fooUnvalidated" }
        };

        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            Mock<MockableUnvalidatedRequestValues> mockUnvalidatedValues = new Mock<MockableUnvalidatedRequestValues>();
            FormValueProviderFactory factory = new FormValueProviderFactory(_ => mockUnvalidatedValues.Object);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Form).Returns(_backingStore);

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Equal(typeof(FormValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            Assert.NotNull(vpResult);
            Assert.Equal("fooValue", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProvider_GetValue_SkipValidation()
        {
            // Arrange
            Mock<MockableUnvalidatedRequestValues> mockUnvalidatedValues = new Mock<MockableUnvalidatedRequestValues>();
            mockUnvalidatedValues.Setup(o => o.Form).Returns(_unvalidatedBackingStore);
            FormValueProviderFactory factory = new FormValueProviderFactory(_ => mockUnvalidatedValues.Object);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Form).Returns(_backingStore);

            // Act
            IUnvalidatedValueProvider valueProvider = (IUnvalidatedValueProvider)factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Equal(typeof(FormValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("foo", skipValidation: true);

            Assert.NotNull(vpResult);
            Assert.Equal("fooUnvalidated", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProvider_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            FormValueProviderFactory factory = new FormValueProviderFactory();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { factory.GetValueProvider(null); }, "controllerContext");
        }
    }
}
