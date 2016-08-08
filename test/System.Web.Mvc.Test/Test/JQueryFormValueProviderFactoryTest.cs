// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class JQueryFormValueProviderFactoryTest
    {
        private static readonly NameValueCollection _backingStore = new NameValueCollection()
        {
            { "foo", "fooValue" },
            { "fooArray[0][bar1]", "fooArrayValue"}
        };

        private static readonly NameValueCollection _unvalidatedBackingStore = new NameValueCollection()
        {
            { "foo", "fooUnvalidated" },
            { "fooArray[0][bar1][0][nested]", "fooNestedUnvalidatedValue" }
        };

        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            Mock<MockableUnvalidatedRequestValues> mockUnvalidatedValues = new Mock<MockableUnvalidatedRequestValues>();
            JQueryFormValueProviderFactory factory = 
                        new JQueryFormValueProviderFactory(_ => mockUnvalidatedValues.Object);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Form).Returns(_backingStore);

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Equal(typeof(JQueryFormValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("fooArray[0].bar1");

            Assert.NotNull(vpResult);
            Assert.Equal("fooArrayValue", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProvider_GetValue_SkipValidation()
        {
            // Arrange
            Mock<MockableUnvalidatedRequestValues> mockUnvalidatedValues = new Mock<MockableUnvalidatedRequestValues>();
            mockUnvalidatedValues.Setup(o => o.Form).Returns(_unvalidatedBackingStore);
            JQueryFormValueProviderFactory factory = 
                        new JQueryFormValueProviderFactory(_ => mockUnvalidatedValues.Object);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Form).Returns(_backingStore);

            // Act
            IUnvalidatedValueProvider valueProvider = 
                        (IUnvalidatedValueProvider)factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Equal(typeof(JQueryFormValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("fooArray[0].bar1[0].nested", skipValidation: true);

            Assert.NotNull(vpResult);
            Assert.Equal("fooNestedUnvalidatedValue", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProvider_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            JQueryFormValueProviderFactory factory = new JQueryFormValueProviderFactory();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { factory.GetValueProvider(null); }, "controllerContext");
        }
    }
}
