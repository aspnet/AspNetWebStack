// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Results
{
    public class FormattedContentResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenFormatterIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = null;
            MediaTypeHeaderValue mediaType = CreateMediaType();

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    new FormattedContentResult<object>(statusCode, content, formatter, mediaType, request);
                }, "formatter");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                new FormattedContentResult<object>(statusCode, content, formatter, mediaType, request);
            }, "request");
        }

        [Fact]
        public void StatusCode_Returns_ValueProvided()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();

            using (HttpRequestMessage request = CreateRequest())
            {
                FormattedContentResult<object> result = new FormattedContentResult<object>(expectedStatusCode, content,
                    formatter, mediaType, request);

                // Act
                HttpStatusCode statusCode = result.StatusCode;

                // Assert
                Assert.Equal(expectedStatusCode, statusCode);
            }
        }

        [Fact]
        public void Content_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();

            using (HttpRequestMessage request = CreateRequest())
            {
                FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, expectedContent,
                    formatter, mediaType, request);

                // Act
                object content = result.Content;

                // Assert
                Assert.Same(expectedContent, content);
            }
        }

        [Fact]
        public void Formatter_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();

            using (HttpRequestMessage request = CreateRequest())
            {
                FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content,
                    expectedFormatter, mediaType, request);

                // Act
                MediaTypeFormatter formatter = result.Formatter;

                // Assert
                Assert.Same(expectedFormatter, formatter);
            }
        }

        [Fact]
        public void MediaType_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();

            using (HttpRequestMessage request = CreateRequest())
            {
                FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content,
                    formatter, expectedMediaType, request);

                // Act
                MediaTypeHeaderValue mediaType = result.MediaType;

                // Assert
                Assert.Same(expectedMediaType, mediaType);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content,
                    formatter, mediaType, expectedRequest);

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
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = new FormattedContentResult<object>(expectedStatusCode, expectedContent,
                    expectedFormatter, expectedMediaType, expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(expectedStatusCode, response.StatusCode);
                    HttpContent content = response.Content;
                    ObjectContent<object> typedContent = Assert.IsType<ObjectContent<object>>(content);
                    Assert.Same(expectedContent, typedContent.Value);
                    Assert.Same(expectedFormatter, typedContent.Formatter);
                    Assert.NotNull(typedContent.Headers);
                    Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                new FormattedContentResult<object>(statusCode, content, formatter, mediaType, controller);
            }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                IHttpActionResult result = new FormattedContentResult<object>(expectedStatusCode, expectedContent,
                    expectedFormatter, expectedMediaType, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(expectedStatusCode, response.StatusCode);
                    HttpContent content = response.Content;
                    ObjectContent<object> typedContent = Assert.IsType<ObjectContent<object>>(content);
                    Assert.Same(expectedContent, typedContent.Value);
                    Assert.Same(expectedFormatter, typedContent.Formatter);
                    Assert.NotNull(typedContent.Headers);
                    Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                    Assert.Equal(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();
            FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content, formatter,
                mediaType, controller);

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
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();
            FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content, formatter,
                mediaType, controller);

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
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);
            FormattedContentResult<object> result = new FormattedContentResult<object>(statusCode, content, formatter,
                mediaType, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerContent_WithFormatter_CreatesCorrectFormattedContentResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ApiController controller = CreateController();

            // Act
            FormattedContentResult<object> result = controller.Content(expectedStatusCode, expectedContent,
                expectedFormatter, expectedMediaType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);
            Assert.Same(expectedContent, result.Content);
            Assert.Same(expectedFormatter, result.Formatter);
            Assert.Same(expectedMediaType, result.MediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerContent_WithFormatterAndStringMediaType_CreatesCorrectFormattedContentResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            string expectedMediaType = CreateMediaType().MediaType;
            ApiController controller = CreateController();

            // Act
            FormattedContentResult<object> result = controller.Content(expectedStatusCode, expectedContent,
                expectedFormatter, expectedMediaType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);
            Assert.Same(expectedContent, result.Content);
            Assert.Same(expectedFormatter, result.Formatter);
            MediaTypeHeaderValue mediaType = result.MediaType;
            Assert.NotNull(mediaType);
            Assert.Equal(expectedMediaType, mediaType.MediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerContent_WithFormatterButNotMediaType_CreatesCorrectFormattedContentResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            ApiController controller = CreateController();

            // Act
            FormattedContentResult<object> result = controller.Content(expectedStatusCode, expectedContent,
                expectedFormatter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);
            Assert.Same(expectedContent, result.Content);
            Assert.Same(expectedFormatter, result.Formatter);
            Assert.Null(result.MediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        private static object CreateContent()
        {
            return new object();
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static MediaTypeFormatter CreateFormatter()
        {
            return new StubFormatter();
        }

        private static MediaTypeHeaderValue CreateMediaType()
        {
            return new MediaTypeHeaderValue("text/plain");
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpStatusCode CreateStatusCode()
        {
            return HttpStatusCode.Continue;
        }

        private class StubFormatter : MediaTypeFormatter
        {
            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }
        }

        private class FakeController : ApiController
        {
        }
    }
}
