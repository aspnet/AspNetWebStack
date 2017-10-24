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
    public class NegotiatedContentResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenContentNegotiatorIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    new NegotiatedContentResult<object>(statusCode, content, contentNegotiator, request, formatters);
                }, "contentNegotiator");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();
            HttpRequestMessage request = null;
            IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                new NegotiatedContentResult<object>(statusCode, content, contentNegotiator, request, formatters);
            }, "request");
        }

        [Fact]
        public void Constructor_Throws_WhenFormattersIsNull()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    new NegotiatedContentResult<object>(statusCode, content, contentNegotiator, request, formatters);
                }, "formatters");
            }
        }

        [Fact]
        public void StatusCode_Returns_ValueProvided()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(expectedStatusCode,
                    content, contentNegotiator, request, formatters);

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
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode,
                    expectedContent, contentNegotiator, request, formatters);

                // Act
                object content = result.Content;

                // Assert
                Assert.Same(expectedContent, content);
            }
        }

        [Fact]
        public void ContentNegotiator_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator expectedContentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    expectedContentNegotiator, request, formatters);

                // Act
                IContentNegotiator contentNegotiator = result.ContentNegotiator;

                // Assert
                Assert.Same(expectedContentNegotiator, contentNegotiator);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    contentNegotiator, expectedRequest, formatters);

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void Formatters_Returns_InstanceProvided()
        {
            // Arrange
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    contentNegotiator, request, expectedFormatters);

                // Act
                IEnumerable<MediaTypeFormatter> formatters = result.Formatters;

                // Assert
                Assert.Same(expectedFormatters, formatters);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Returns_CorrectResponse_WhenContentNegotiationSucceeds()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ContentNegotiationResult negotiationResult = new ContentNegotiationResult(expectedFormatter, expectedMediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(object), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = new NegotiatedContentResult<object>(expectedStatusCode, expectedContent,
                    contentNegotiator, expectedRequest, expectedFormatters);

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
        public async Task ExecuteAsync_Returns_CorrectResponse_WhenContentNegotiationFails()
        {
            // Arrange
            HttpStatusCode statusCode = HttpStatusCode.Conflict;
            object content = CreateContent();
            ContentNegotiationResult negotiationResult = null;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(object), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = new NegotiatedContentResult<object>(statusCode, content, contentNegotiator,
                    expectedRequest, expectedFormatters);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                new NegotiatedContentResult<object>(statusCode, content, controller);
            }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse_WhenContentNegotationSucceeds()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
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
                spy.Setup(n => n.Negotiate(typeof(object), expectedRequest, It.Is(formattersMatch))).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                using (HttpConfiguration configuration = CreateConfiguration(expectedInputFormatter,
                    contentNegotiator))
                {
                    controller.Configuration = configuration;
                    controller.Request = expectedRequest;

                    IHttpActionResult result = new NegotiatedContentResult<object>(expectedStatusCode, expectedContent,
                        controller);

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
                        Assert.Same(expectedOutputFormatter, typedContent.Formatter);
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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), CreateContentNegotiator()))
            {
                controller.Configuration = configuration;

                using (HttpRequestMessage request = CreateRequest())
                {
                    controller.Request = request;

                    NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                        controller);

                    IContentNegotiator expectedContentNegotiator = CreateContentNegotiator();
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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), CreateContentNegotiator()))
            {
                controller.Configuration = configuration;
                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(CreateFormatter(), contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            IContentNegotiator expectedContentNegotiator = CreateContentNegotiator();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), expectedContentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

                IContentNegotiator ignore = result.ContentNegotiator;

                configuration.Services.Replace(typeof(IContentNegotiator), CreateContentNegotiator());

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), CreateContentNegotiator()))
            {
                controller.Configuration = configuration;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            IContentNegotiator contentNegotiator = CreateContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(expectedFormatter, contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();
            HttpControllerContext context = new HttpControllerContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.ControllerContext = context;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), null))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;

                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

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
            HttpStatusCode statusCode = CreateStatusCode();
            object content = CreateContent();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), CreateContentNegotiator()))
            {
                controller.Configuration = configuration;
                NegotiatedContentResult<object> result = new NegotiatedContentResult<object>(statusCode, content,
                    controller);

                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
            }
        }

        [Fact]
        public void ApiControllerContent_WithContent_CreatesCorrectNegotiatedContentResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = CreateStatusCode();
            object expectedContent = CreateContent();
            ApiController controller = CreateController();

            // Act
            NegotiatedContentResult<object> result = controller.Content(expectedStatusCode, expectedContent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);
            Assert.Same(expectedContent, result.Content);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), CreateContentNegotiator()))
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

        private static object CreateContent()
        {
            return new object();
        }

        private static IContentNegotiator CreateContentNegotiator()
        {
            return new DummyContentNegotiator();
        }

        private static ApiController CreateController()
        {
            return new FakeController();
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

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpStatusCode CreateStatusCode()
        {
            return HttpStatusCode.Continue;
        }

        private class DummyContentNegotiator : IContentNegotiator
        {
            public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request,
                IEnumerable<MediaTypeFormatter> formatters)
            {
                throw new NotImplementedException();
            }
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
