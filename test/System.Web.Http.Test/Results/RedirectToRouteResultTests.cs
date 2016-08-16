// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Results
{
    public class RedirectToRouteResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenRouteNameIsNull()
        {
            // Arrange
            string routeName = null;
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper urlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, urlFactory, request);
                }, "routeName");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenUrlFactoryIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper urlFactory = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, urlFactory, request);
                }, "urlFactory");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(routeName, routeValues, urlFactory, request);
            }, "request");
        }

        [Fact]
        public void RouteName_Returns_InstanceProvided()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper urlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage request = CreateRequest())
            {
                RedirectToRouteResult result = CreateProductUnderTest(expectedRouteName, routeValues, urlFactory,
                    request);

                // Act
                string routeName = result.RouteName;

                // Assert
                Assert.Same(expectedRouteName, routeName);
            }
        }

        [Fact]
        public void RouteValues_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            UrlHelper urlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage request = CreateRequest())
            {
                RedirectToRouteResult result = CreateProductUnderTest(routeName, expectedRouteValues, urlFactory,
                    request);

                // Act
                IDictionary<string, object> routeValues = result.RouteValues;

                // Assert
                Assert.Same(expectedRouteValues, routeValues);
            }
        }

        [Fact]
        public void UrlFactory_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper expectedUrlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage request = CreateRequest())
            {
                RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, expectedUrlFactory,
                    request);

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            UrlHelper urlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, urlFactory,
                    expectedRequest);

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Returns_CorrectResponse()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            Mock<UrlHelper> spyUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            string expectedLocation = CreateLocation().AbsoluteUri;
            spyUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns(expectedLocation);
            UrlHelper urlFactory = spyUrlFactory.Object;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues, urlFactory,
                    expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Same(expectedLocation, response.Headers.Location.OriginalString);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task ExecuteAsync_Throws_WhenUrlHelperLinkReturnsNull()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            Mock<UrlHelper> stubUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            stubUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns((string)null);
            UrlHelper urlFactory = stubUrlFactory.Object;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues, urlFactory,
                    expectedRequest);

                // Act & Assert
                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => result.ExecuteAsync(CancellationToken.None));
                Assert.Equal("UrlHelper.Link must not return null.", exception.Message);
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(routeName, routeValues, controller);
            }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            Mock<UrlHelper> spyUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            string expectedLocation = CreateLocation().AbsoluteUri;
            spyUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns(expectedLocation);
            UrlHelper urlFactory = spyUrlFactory.Object;
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                controller.Url = urlFactory;

                IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Same(expectedLocation, response.Headers.Location.OriginalString);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void UrlFactory_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = CreateController();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Request = request;
                controller.Url = CreateDummyUrlFactory();

                RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, controller);

                UrlHelper expectedUrlFactory = CreateDummyUrlFactory();
                controller.Url = expectedUrlFactory;

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = CreateController();
            RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void UrlFactory_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = CreateController();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Request = request;
                UrlHelper expectedUrlFactory = CreateDummyUrlFactory();
                controller.Url = expectedUrlFactory;

                RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, controller);

                UrlHelper ignore = result.UrlFactory;

                controller.Url = CreateDummyUrlFactory();

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = CreateController();

            RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                HttpRequestMessage ignore = result.Request;

                using (HttpRequestMessage otherRequest = CreateRequest())
                {
                    controller.Request = otherRequest;

                    // Act
                    HttpRequestMessage request = result.Request;

                    // Assert
                    Assert.Same(expectedRequest, request);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_Throws_WhenControllerRequestIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);

            RedirectToRouteResult result = CreateProductUnderTest(routeName, routeValues, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerRedirectToRoute_WithStringAndDictionary_CreatesCorrectResult()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            ApiController controller = CreateController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute(expectedRouteName, expectedRouteValues);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedRouteName, result.RouteName);
            Assert.Same(expectedRouteValues, result.RouteValues);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerRedirectToRoute_WithStringAndObject_CreatesCorrectResult()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            object routeValues = new { id = 1 };
            ApiController controller = CreateController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute(expectedRouteName, routeValues);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedRouteName, result.RouteName);
            Assert.IsType<HttpRouteValueDictionary>(result.RouteValues);
            Assert.True(result.RouteValues.ContainsKey("id"));
            Assert.Equal(1, result.RouteValues["id"]);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static UrlHelper CreateDummyUrlFactory()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static Uri CreateLocation()
        {
            return new Uri("aa://b");
        }

        private static RedirectToRouteResult CreateProductUnderTest(string routeName,
            IDictionary<string, object> routeValues, UrlHelper urlFactory, HttpRequestMessage request)
        {
            return new RedirectToRouteResult(routeName, routeValues, urlFactory, request);
        }

        private static RedirectToRouteResult CreateProductUnderTest(string routeName,
            IDictionary<string, object> routeValues, ApiController controller)
        {
            return new RedirectToRouteResult(routeName, routeValues, controller);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static string CreateRouteName()
        {
            return "IgnoreRouteName";
        }

        private static IDictionary<string, object> CreateRouteValues()
        {
            return new Dictionary<string, object>();
        }

        private class FakeController : ApiController
        {
        }
    }
}
