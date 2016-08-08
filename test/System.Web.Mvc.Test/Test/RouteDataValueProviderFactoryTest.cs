// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class RouteDataValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            RouteDataValueProviderFactory factory = new RouteDataValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();
            controllerContext.RouteData.Values["forty-two"] = 42;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.IsType<RouteDataValueProvider>(valueProvider);
            ValueProviderResult vpResult = valueProvider.GetValue("forty-two");

            Assert.NotNull(vpResult);
            Assert.Equal(42, vpResult.RawValue);
            Assert.Equal("42", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProvider_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            RouteDataValueProviderFactory factory = new RouteDataValueProviderFactory();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { factory.GetValueProvider(null); }, "controllerContext");
        }
    }
}
