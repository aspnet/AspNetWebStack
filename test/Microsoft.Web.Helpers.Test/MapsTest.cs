// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    public class MapsTest
    {
        [Fact]
        public void GetDirectionsQueryReturnsLocationIfNotEmpty()
        {
            // Arrange
            var location = "a%";
            var latitude = "12.34";
            var longitude = "-56.78";

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude);

            // Assert
            Assert.Equal("a%25", result);
        }

        [Fact]
        public void GetDirectionsQueryReturnsLatitudeLongitudeIfLocationIsEmpty()
        {
            // Arrange
            var location = "";
            var latitude = "12.34%";
            var longitude = "-&56.78";

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude);

            // Assert
            Assert.Equal("12.34%25%2c-%2656.78", result);
        }

        [Fact]
        public void GetDirectionsQueryUsesSpecifiedEncoder()
        {
            // Arrange
            var location = "24 gnidliuB tfosorciM";
            var latitude = "12.34%";
            var longitude = "-&56.78";
            Func<string, string> encoder = k => new String(k.Reverse().ToArray());

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude, encoder);

            // Assert
            Assert.Equal("Microsoft Building 42", result);
        }

        [Fact]
        public void GetProviderHtml_DoesNotContainBadRazorCompilation()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange
                var stubbedContext = new Mock<HttpContextBase>();
                var contextItems = new Hashtable();
                stubbedContext.SetupGet(x => x.Items).Returns(contextItems);
                Maps.GetCurrentHttpContext = () => stubbedContext.Object;

                // Act
                string bingResults = Maps.GetBingHtml("somekey", latitude: "100", longitude: "10").ToHtmlString();
                string googleResults = Maps.GetGoogleHtml(latitude: "100", longitude: "10").ToHtmlString();
                string mapQuestResults = Maps.GetMapQuestHtml("somekey", latitude: "100", longitude: "10").ToHtmlString();

                // Assert
                Assert.DoesNotContain("<text>", bingResults);
                Assert.DoesNotContain("<text>", googleResults);
                Assert.DoesNotContain("<text>", mapQuestResults);
            });
        }
    }
}
