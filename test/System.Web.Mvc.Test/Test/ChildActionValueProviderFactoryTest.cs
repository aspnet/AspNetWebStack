// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ExpiicitRouteDataValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProviderReturnsChildActionValue()
        {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();

            string conflictingKey = "conflictingKey";

            controllerContext.RouteData.Values["conflictingKey"] = 43;

            DictionaryValueProvider<object> explictValueDictionary = new DictionaryValueProvider<object>(new RouteValueDictionary { { conflictingKey, 42 } }, CultureInfo.InvariantCulture);
            controllerContext.RouteData.Values[ChildActionValueProvider.ChildActionValuesKey] = explictValueDictionary;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue(conflictingKey);

            Assert.NotNull(vpResult);
            Assert.Equal(42, vpResult.RawValue);
            Assert.Equal("42", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValueProviderReturnsNullIfNoChildActionDictionary()
        {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();
            controllerContext.RouteData.Values["forty-two"] = 42;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("forty-two");

            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValueProviderReturnsNullIfKeyIsNotInChildActionDictionary()
        {
            // Arrange
            ChildActionValueProviderFactory factory = new ChildActionValueProviderFactory();

            ControllerContext controllerContext = new ControllerContext();
            controllerContext.RouteData = new RouteData();
            controllerContext.RouteData.Values["forty-two"] = 42;

            DictionaryValueProvider<object> explictValueDictionary = new DictionaryValueProvider<object>(new RouteValueDictionary { { "forty-three", 42 } }, CultureInfo.CurrentUICulture);
            controllerContext.RouteData.Values[ChildActionValueProvider.ChildActionValuesKey] = explictValueDictionary;

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(typeof(ChildActionValueProvider), valueProvider.GetType());
            ValueProviderResult vpResult = valueProvider.GetValue("forty-two");

            Assert.Null(vpResult);
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
