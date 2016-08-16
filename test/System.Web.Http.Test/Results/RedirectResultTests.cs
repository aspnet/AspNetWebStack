// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Results
{
    public class RedirectResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenLocationIsNull()
        {
            // Arrange
            Uri location = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(location, request); }, "location");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            Uri location = CreateLocation();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(location, request); }, "request");
        }

        [Fact]
        public void Location_Returns_InstanceProvided()
        {
            // Arrange
            Uri expectedLocation = CreateLocation();

            using (HttpRequestMessage request = CreateRequest())
            {
                RedirectResult result = CreateProductUnderTest(expectedLocation, request);

                // Act
                Uri location = result.Location;

                // Assert
                Assert.Same(expectedLocation, location);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            Uri location = CreateLocation();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                RedirectResult result = CreateProductUnderTest(location, expectedRequest);

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
            Uri expectedLocation = CreateLocation();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = CreateProductUnderTest(expectedLocation, expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Same(expectedLocation, response.Headers.Location);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            Uri location = CreateLocation();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(location, controller); }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            Uri expectedLocation = CreateLocation();
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;

                IHttpActionResult result = CreateProductUnderTest(expectedLocation, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Same(expectedLocation, response.Headers.Location);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            Uri location = CreateLocation();
            ApiController controller = CreateController();
            RedirectResult result = CreateProductUnderTest(location, controller);

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
        public void Request_ForApiController_EvaluatesOnce()
        {
            // Arrange
            Uri location = CreateLocation();
            ApiController controller = CreateController();
            RedirectResult result = CreateProductUnderTest(location, controller);

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
            Uri location = CreateLocation();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);
            RedirectResult result = CreateProductUnderTest(location, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerRedirect_WithUri_CreatesCorrectResult()
        {
            // Arrange
            Uri expectedLocation = CreateLocation();
            ApiController controller = CreateController();

            // Act
            RedirectResult result = controller.Redirect(expectedLocation);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedLocation, result.Location);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerRedirect_WithString_Throws_WhenLocationIsNull()
        {
            // Arrange
            string location = null;
            ApiController controller = CreateController();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { controller.Redirect(location); }, "location");
        }

        [Fact]
        public void ApiControllerRedirect_WithString_CreatesCorrectResult()
        {
            // Arrange
            string expectedLocation = CreateLocation().OriginalString;
            ApiController controller = CreateController();

            // Act
            RedirectResult result = controller.Redirect(expectedLocation);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedLocation, result.Location.OriginalString);

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

        private static Uri CreateLocation()
        {
            return new Uri("aa://b");
        }

        private static RedirectResult CreateProductUnderTest(Uri location, HttpRequestMessage request)
        {
            return new RedirectResult(location, request);
        }

        private static RedirectResult CreateProductUnderTest(Uri location, ApiController controller)
        {
            return new RedirectResult(location, controller);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private class FakeController : ApiController
        {
        }
    }
}
