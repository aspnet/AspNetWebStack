// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class TempDataValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider_CorrectlyRetainsOrRemovesKeys()
        {
            // Arrange
            string[] expectedRetainedKeys = new[]
            {
                "retainMe"
            };

            TempDataDictionary tempData = new TempDataDictionary
            {
                { "retainMe", "retainMeValue" },
                { "removeMe", "removeMeValue" },
                { "previouslyRemoved", "previouslyRemovedValue" }
            };
            object dummy = tempData["previouslyRemoved"]; // mark value for removal

            ControllerContext controllerContext = GetControllerContext(tempData);

            TempDataValueProviderFactory factory = new TempDataValueProviderFactory();

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(controllerContext);
            ValueProviderResult nonExistentResult = valueProvider.GetValue("nonExistent");
            ValueProviderResult removeMeResult = valueProvider.GetValue("removeme");

            // Assert
            Assert.Null(nonExistentResult);
            Assert.Equal("removeMeValue", removeMeResult.AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, removeMeResult.Culture);

            // Verify that keys have been removed or retained correctly by the provider
            Mock<ITempDataProvider> mockTempDataProvider = new Mock<ITempDataProvider>();
            string[] retainedKeys = null;
            mockTempDataProvider
                .Setup(o => o.SaveTempData(controllerContext, It.IsAny<IDictionary<string, object>>()))
                .Callback(
                    delegate(ControllerContext cc, IDictionary<string, object> d) { retainedKeys = d.Keys.ToArray(); });

            tempData.Save(controllerContext, mockTempDataProvider.Object);
            Assert.Equal(expectedRetainedKeys, retainedKeys);
        }

        [Fact]
        public void GetValueProvider_EmptyTempData_ReturnsNull()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();
            ControllerContext controllerContext = GetControllerContext(tempData);

            TempDataValueProviderFactory factory = new TempDataValueProviderFactory();

            // Act
            IValueProvider provider = factory.GetValueProvider(controllerContext);

            // Assert
            Assert.Null(provider);
        }

        private static ControllerContext GetControllerContext(TempDataDictionary tempData)
        {
            return new ControllerContext
            {
                Controller = new EmptyController
                {
                    TempData = tempData
                }
            };
        }

        private sealed class EmptyController : Controller
        {
        }
    }
}
