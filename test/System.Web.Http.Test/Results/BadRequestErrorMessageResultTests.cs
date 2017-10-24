// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Results
{
    public class BadRequestErrorMessageResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenMessageIsNull()
        {
            // Arrange
            string message = null;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(message, contentNegotiator, request, formatters);
                }, "message");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenContentNegotiatorIsNull()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator contentNegotiator = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(message, contentNegotiator, request, formatters);
                }, "contentNegotiator");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();
            HttpRequestMessage request = null;
            IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(message, contentNegotiator, request, formatters);
            }, "request");
        }

        [Fact]
        public void Constructor_Throws_WhenFormattersIsNull()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(message, contentNegotiator, request, formatters);
                }, "formatters");
            }
        }

        [Fact]
        public void Message_ReturnsInstanceProvided()
        {
            // Arrange
            string expectedMessage = CreateMessage();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                BadRequestErrorMessageResult result = CreateProductUnderTest(expectedMessage, contentNegotiator,
                    request, formatters);

                // Act
                string message = result.Message;

                // Assert
                Assert.Same(expectedMessage, message);
            }
        }

        [Fact]
        public void ContentNegotiator_ReturnsInstanceProvided()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, expectedContentNegotiator,
                    request, formatters);

                // Act
                IContentNegotiator contentNegotiator = result.ContentNegotiator;

                // Assert
                Assert.Same(expectedContentNegotiator, contentNegotiator);
            }
        }

        [Fact]
        public void Request_ReturnsInstanceProvided()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, contentNegotiator,
                    expectedRequest, formatters);

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void Formatters_ReturnsInstanceProvided()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, contentNegotiator, request,
                    expectedFormatters);

                // Act
                IEnumerable<MediaTypeFormatter> formatters = result.Formatters;

                // Assert
                Assert.Same(expectedFormatters, formatters);
            }
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsCorrectResponse_WhenContentNegotiationSucceeds()
        {
            // Arrange
            string expectedMessage = CreateMessage();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ContentNegotiationResult negotiationResult = new ContentNegotiationResult(expectedFormatter,
                expectedMediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(HttpError), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = CreateProductUnderTest(expectedMessage, contentNegotiator, expectedRequest,
                    expectedFormatters);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    HttpContent content = response.Content;
                    ObjectContent<HttpError> typedContent = Assert.IsType<ObjectContent<HttpError>>(content);
                    HttpError error = (HttpError)typedContent.Value;
                    Assert.NotNull(error);
                    Assert.Same(expectedMessage, error.Message);
                    Assert.Same(expectedFormatter, typedContent.Formatter);
                    Assert.NotNull(typedContent.Headers);
                    Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsCorrectResponse_WhenContentNegotiationFails()
        {
            // Arrange
            string message = CreateMessage();
            ContentNegotiationResult negotiationResult = null;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(HttpError), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = CreateProductUnderTest(message, contentNegotiator, expectedRequest,
                    expectedFormatters);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(message, controller); }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse_WhenContentNegotationSucceeds()
        {
            // Arrange
            string expectedMessage = CreateMessage();
            ApiController controller = CreateController();
            MediaTypeFormatter expectedInputFormatter = CreateFormatter();
            MediaTypeFormatter expectedOutputFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ContentNegotiationResult negotiationResult = new ContentNegotiationResult(expectedOutputFormatter,
                expectedMediaType);

            Expression<Func<IEnumerable<MediaTypeFormatter>, bool>> formattersMatch = (f) =>
                f != null && f.AsArray().Length == 1 && f.AsArray()[0] == expectedInputFormatter;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(HttpError), expectedRequest, It.Is(formattersMatch))).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                using (HttpConfiguration configuration = CreateConfiguration(expectedInputFormatter,
                    contentNegotiator))
                {
                    configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    controller.Configuration = configuration;
                    controller.Request = expectedRequest;

                    IHttpActionResult result = CreateProductUnderTest(expectedMessage, controller);

                    // Act
                    Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                    // Assert
                    Assert.NotNull(task);

                    using (HttpResponseMessage response = await task)
                    {
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                        HttpContent content = response.Content;
                        ObjectContent<HttpError> typedContent = Assert.IsType<ObjectContent<HttpError>>(content);
                        HttpError error = (HttpError)typedContent.Value;
                        Assert.NotNull(error);
                        Assert.Same(expectedMessage, error.Message);
                        Assert.NotNull(typedContent.Headers);
                        Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                        Assert.Same(expectedRequest, response.RequestMessage);
                    }
                }
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                using (HttpRequestMessage request = CreateRequest())
                {
                    controller.Request = request;

                    BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                    IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();
                    configuration.Services.Replace(typeof(IContentNegotiator), expectedContentNegotiator);

                    // Act
                    IContentNegotiator contentNegotiator = result.ContentNegotiator;

                    // Assert
                    Assert.Same(expectedContentNegotiator, contentNegotiator);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string message = CreateMessage();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                using (HttpRequestMessage expectedRequest = CreateRequest())
                {
                    controller.Request = expectedRequest;

                    // Act
                    HttpRequestMessage request = result.Request;

                    // Assert
                    Assert.Same(expectedRequest, request);
                }
            }
        }

        [Fact]
        public void Formatters_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(CreateFormatter(), contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                MediaTypeFormatter expectedFormatter = CreateFormatter();

                using (HttpConfiguration lateConfiguration = CreateConfiguration(expectedFormatter, contentNegotiator))
                {
                    controller.Configuration = lateConfiguration;

                    // Act
                    IEnumerable<MediaTypeFormatter> formatters = result.Formatters;

                    // Assert
                    Assert.NotNull(formatters);
                    MediaTypeFormatter formatter = Assert.Single(formatters);
                    Assert.Same(expectedFormatter, formatter);
                }
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string message = CreateMessage();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), expectedContentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                IContentNegotiator ignore = result.ContentNegotiator;

                configuration.Services.Replace(typeof(IContentNegotiator), CreateDummyContentNegotiator());

                // Act
                IContentNegotiator contentNegotiator = result.ContentNegotiator;

                // Assert
                Assert.Same(expectedContentNegotiator, contentNegotiator);
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

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
        }

        [Fact]
        public void Formatters_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(expectedFormatter, contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                IEnumerable<MediaTypeFormatter> ignore = result.Formatters;

                using (HttpConfiguration lateConfiguration = CreateConfiguration(CreateFormatter(), contentNegotiator))
                {
                    controller.Configuration = lateConfiguration;

                    // Act
                    IEnumerable<MediaTypeFormatter> formatters = result.Formatters;

                    // Assert
                    Assert.NotNull(formatters);
                    MediaTypeFormatter formatter = Assert.Single(formatters);
                    Assert.Same(expectedFormatter, formatter);
                }
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_Throws_WhenConfigurationIsNull()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();
            HttpControllerContext context = new HttpControllerContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.ControllerContext = context;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                // Act & Assert
                Assert.Throws<InvalidOperationException>(
                    () => { IContentNegotiator ignore = result.ContentNegotiator; },
                    "HttpControllerContext.Configuration must not be null.");
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_Throws_WhenServiceIsNull()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), null))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Request = request;
                controller.Configuration = configuration;

                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                // Act & Assert
                Assert.Throws<InvalidOperationException>(
                    () => { IContentNegotiator ignore = result.ContentNegotiator; },
                    "The provided configuration does not have an instance of the " +
                    "'System.Net.Http.Formatting.IContentNegotiator' service registered.");
            }
        }

        [Fact]
        public void Request_ForApiController_Throws_WhenControllerRequestIsNull()
        {
            // Arrange
            string message = CreateMessage();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                BadRequestErrorMessageResult result = CreateProductUnderTest(message, controller);

                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
            }
        }

        [Fact]
        public void ApiControllerBadRequest_WithString_CreatesCorrectResult()
        {
            // Arrange
            string expectedMessage = CreateMessage();
            ApiController controller = CreateController();

            // Act
            BadRequestErrorMessageResult result = controller.BadRequest(expectedMessage);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedMessage, result.Message);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        private static HttpConfiguration CreateConfiguration(MediaTypeFormatter formatter,
            IContentNegotiator contentNegotiator)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Formatters.Clear();
            configuration.Formatters.Add(formatter);
            configuration.Services.Replace(typeof(IContentNegotiator), contentNegotiator);
            return configuration;
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static IContentNegotiator CreateDummyContentNegotiator()
        {
            return new Mock<IContentNegotiator>(MockBehavior.Strict).Object;
        }

        private static MediaTypeFormatter CreateFormatter()
        {
            return new StubMediaTypeFormatter();
        }

        private static IEnumerable<MediaTypeFormatter> CreateFormatters()
        {
            return new MediaTypeFormatter[0];
        }

        private static MediaTypeHeaderValue CreateMediaType()
        {
            return new MediaTypeHeaderValue("text/plain");
        }

        private static string CreateMessage()
        {
            return "IgnoreMessage";
        }

        private static BadRequestErrorMessageResult CreateProductUnderTest(string message,
            IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
        {
            return new BadRequestErrorMessageResult(message, contentNegotiator, request, formatters);
        }

        private static BadRequestErrorMessageResult CreateProductUnderTest(string message, ApiController controller)
        {
            return new BadRequestErrorMessageResult(message, controller);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private class StubMediaTypeFormatter : MediaTypeFormatter
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
