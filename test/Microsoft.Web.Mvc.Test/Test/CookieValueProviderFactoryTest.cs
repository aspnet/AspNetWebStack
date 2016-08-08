// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class CookieValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            HttpCookieCollection cookies = new HttpCookieCollection
            {
                new HttpCookie("foo", "fooValue"),
                new HttpCookie("bar.baz", "barBazValue"),
                new HttpCookie("", "emptyValue"),
                new HttpCookie(null, "nullValue")
            };

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Cookies).Returns(cookies);

            CookieValueProviderFactory factory = new CookieValueProviderFactory();

            // Act
            IValueProvider provider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Null(provider.GetValue(""));
            Assert.True(provider.ContainsPrefix("bar"));
            Assert.Equal("fooValue", provider.GetValue("foo").AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, provider.GetValue("foo").Culture);
        }
    }
}
