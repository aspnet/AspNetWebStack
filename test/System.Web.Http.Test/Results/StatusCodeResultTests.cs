// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Results
{
    public class StatusCodeResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { new StatusCodeResult(statusCode, request); }, "request");
        }

        [Fact]
        public void StatusCode_Returns_ValueProvided()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();

            using (HttpRequestMessage request = CreateRequest())
            {
                StatusCodeResult result = new StatusCodeResult(expectedStatusCode, request);

                // Act
                HttpStatusCode statusCode = result.StatusCode;

                // Assert
                Assert.Equal(expectedStatusCode, statusCode);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                StatusCodeResult result = new StatusCodeResult(statusCode, expectedRequest);

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
            HttpStatusCode expectedStatusCode = CreateStatusCode();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = new StatusCodeResult(expectedStatusCode, expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(expectedStatusCode, response.StatusCode);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { new StatusCodeResult(statusCode, controller); }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                IHttpActionResult result = new StatusCodeResult(expectedStatusCode, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(expectedStatusCode, response.StatusCode);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            ApiController controller = CreateController();
            StatusCodeResult result = new StatusCodeResult(statusCode, controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            ApiController controller = CreateController();
            StatusCodeResult result = new StatusCodeResult(statusCode, controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);
            StatusCodeResult result = new StatusCodeResult(statusCode, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerStatusCode_CreatesCorrectStatusCodeResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            ApiController controller = CreateController();

            // Act
            StatusCodeResult result = controller.StatusCode(expectedStatusCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);

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

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpStatusCode CreateStatusCode()
        {
            return HttpStatusCode.Continue;
        }

        private class FakeController : ApiController
        {
        }
    }
}
