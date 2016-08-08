// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class ServerVariablesValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            NameValueCollection serverVars = new NameValueCollection
            {
                { "foo", "fooValue" },
                { "bar.baz", "barBazValue" }
            };

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ServerVariables).Returns(serverVars);

            ServerVariablesValueProviderFactory factory = new ServerVariablesValueProviderFactory();

            // Act
            IValueProvider provider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.True(provider.ContainsPrefix("bar"));
            Assert.Equal("fooValue", provider.GetValue("foo").AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, provider.GetValue("foo").Culture);
        }
    }
}
