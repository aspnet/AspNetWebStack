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
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Results
{
    public class InvalidModelStateResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenModelStateIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = null;
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator, request, formatters);
                }, "modelState");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenContentNegotiatorIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator, request, formatters);
                }, "contentNegotiator");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();
            HttpRequestMessage request = null;
            IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator, request, formatters);
            }, "request");
        }

        [Fact]
        public void Constructor_Throws_WhenFormattersIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator, request, formatters);
                }, "formatters");
            }
        }

        [Fact]
        public void ModelState_ReturnsInstanceProvided()
        {
            // Arrange
            ModelStateDictionary expectedModelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                InvalidModelStateResult result = CreateProductUnderTest(expectedModelState, includeErrorDetail,
                    contentNegotiator, request, formatters);

                // Act
                ModelStateDictionary modelState = result.ModelState;

                // Assert
                Assert.Same(expectedModelState, modelState);
            }
        }

        [Fact]
        public void IncludeErrorDetail_ReturnsValueProvided()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            bool expectedIncludeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                InvalidModelStateResult result = CreateProductUnderTest(modelState, expectedIncludeErrorDetail,
                    contentNegotiator, request, formatters);

                // Act
                bool includeErrorDetail = result.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Fact]
        public void ContentNegotiator_ReturnsInstanceProvided()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                InvalidModelStateResult result = CreateProductUnderTest(modelState, includeErrorDetail,
                    expectedContentNegotiator, request, formatters);

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
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                InvalidModelStateResult result = CreateProductUnderTest(modelState, includeErrorDetail,
                    contentNegotiator, expectedRequest, formatters);

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
            ModelStateDictionary modelState = CreateModelState();
            bool includeErrorDetail = true;
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                InvalidModelStateResult result = CreateProductUnderTest(modelState, includeErrorDetail,
                    contentNegotiator, request, expectedFormatters);

                // Act
                IEnumerable<MediaTypeFormatter> formatters = result.Formatters;

                // Assert
                Assert.Same(expectedFormatters, formatters);
            }
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsCorrectResponse_WhenContentNegotiationSucceedsAndIncludeErrorDetailIsTrue()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            string expectedModelStateKey = "ModelStateKey";
            string expectedModelStateExceptionMessage = "ModelStateExceptionMessage";
            modelState.AddModelError(expectedModelStateKey, new InvalidOperationException(
                expectedModelStateExceptionMessage));
            bool includeErrorDetail = true;
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

                IHttpActionResult result = CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator,
                    expectedRequest, expectedFormatters);

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
                    HttpError modelStateError = error.ModelState;
                    Assert.NotNull(modelStateError);
                    Assert.True(modelState.ContainsKey(expectedModelStateKey));
                    object modelStateValue = modelStateError[expectedModelStateKey];
                    string[] typedModelStateValues = Assert.IsType<string[]>(modelStateValue);
                    string typedModelStateValue = Assert.Single(typedModelStateValues);
                    Assert.Same(expectedModelStateExceptionMessage, typedModelStateValue);
                    Assert.Same(expectedFormatter, typedContent.Formatter);
                    Assert.NotNull(typedContent.Headers);
                    Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsCorrectResponse_WhenContentNegotiationSucceedsAndIncludeErrorDetailIsFalse()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            string expectedModelStateKey = "ModelStateKey";
            string expectedModelStateErrorMessage = "ModelStateErrorMessage";
            ModelState originalModelStateItem = new ModelState();
            originalModelStateItem.Errors.Add(new ModelError(new InvalidOperationException(),
                expectedModelStateErrorMessage));
            modelState.Add(expectedModelStateKey, originalModelStateItem);
            bool includeErrorDetail = false;
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

                IHttpActionResult result = CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator,
                    expectedRequest, expectedFormatters);

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
                    HttpError modelStateError = error.ModelState;
                    Assert.NotNull(modelStateError);
                    Assert.True(modelState.ContainsKey(expectedModelStateKey));
                    object modelStateValue = modelStateError[expectedModelStateKey];
                    string[] typedModelStateValues = Assert.IsType<string[]>(modelStateValue);
                    string typedModelStateValue = Assert.Single(typedModelStateValues);
                    Assert.Same(expectedModelStateErrorMessage, typedModelStateValue);
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
            ModelStateDictionary modelState = CreateModelStateWithError();
            bool includeErrorDetail = true;
            ContentNegotiationResult negotiationResult = null;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(ModelStateDictionary), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = CreateProductUnderTest(modelState, includeErrorDetail, contentNegotiator,
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
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(modelState, controller); }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse_WhenContentNegotationSucceeds()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            string expectedModelStateKey = "ModelStateKey";
            string expectedModelStateExceptionMessage = "ModelStateExceptionMessage";
            modelState.AddModelError(expectedModelStateKey, new InvalidOperationException(
                expectedModelStateExceptionMessage));
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
                    controller.RequestContext = new HttpRequestContext
                    {
                        Configuration = configuration,
                        IncludeErrorDetail = true
                    };
                    controller.Request = expectedRequest;

                    IHttpActionResult result = CreateProductUnderTest(modelState, controller);

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
                        HttpError modelStateError = error.ModelState;
                        Assert.NotNull(modelStateError);
                        Assert.True(modelState.ContainsKey(expectedModelStateKey));
                        object modelStateValue = modelStateError[expectedModelStateKey];
                        string[] typedModelStateValues = Assert.IsType<string[]>(modelStateValue);
                        string typedModelStateValue = Assert.Single(typedModelStateValues);
                        Assert.Same(expectedModelStateExceptionMessage, typedModelStateValue);
                        Assert.Same(expectedOutputFormatter, typedContent.Formatter);
                        Assert.NotNull(typedContent.Headers);
                        Assert.Equal(expectedMediaType, typedContent.Headers.ContentType);
                        Assert.Same(expectedRequest, response.RequestMessage);
                    }
                }
            }
        }

        [Fact]
        public void IncludeErrorDetail_ForApiController_EvaluatesLazily()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                controller.Configuration = configuration;

                using (HttpRequestMessage request = CreateRequest())
                {
                    controller.Request = request;

                    InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

                    configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;

                    // Act
                    IContentNegotiator contentNegotiator = result.ContentNegotiator;

                    // Assert
                    Assert.False(result.IncludeErrorDetail);
                }
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesLazily()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                using (HttpRequestMessage request = CreateRequest())
                {
                    controller.Request = request;

                    InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
            ModelStateDictionary modelState = CreateModelState();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(CreateFormatter(), contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
        public void IncludeErrorDetail_ForApiController_EvaluatesOnce()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext requestContext = new HttpRequestContext
                {
                    Configuration = configuration,
                    IncludeErrorDetail = true
                };
                controller.RequestContext = requestContext;
                controller.Request = request;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

                bool ignore = result.IncludeErrorDetail;

                requestContext.IncludeErrorDetail = false;

                // Act
                bool includeErrorDetail = result.IncludeErrorDetail;

                // Assert
                Assert.True(includeErrorDetail);
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesOnce()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), expectedContentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(expectedFormatter, contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
        public void IncludeErrorDetail_ForApiController_Throws_WhenControllerRequestIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    { bool ignore = result.IncludeErrorDetail; }, "ApiController.Request must not be null.");
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_Throws_WhenConfigurationIsNull()
        {
            // Arrange
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();
            HttpControllerContext context = new HttpControllerContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.ControllerContext = context;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

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
            ModelStateDictionary modelState = CreateModelState();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), null))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Request = request;
                controller.Configuration = configuration;

                InvalidModelStateResult result = CreateProductUnderTest(modelState, controller);

                // Act & Assert
                Assert.Throws<InvalidOperationException>(
                    () => { IContentNegotiator ignore = result.ContentNegotiator; },
                    "The provided configuration does not have an instance of the " +
                    "'System.Net.Http.Formatting.IContentNegotiator' service registered.");
            }
        }

        [Fact]
        public void ApiControllerBadRequest_WithModelStateDictionary_CreatesCorrectResult()
        {
            // Arrange
            ModelStateDictionary expectedModelState = CreateModelState();
            ApiController controller = CreateController();

            // Act
            InvalidModelStateResult result = controller.BadRequest(expectedModelState);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedModelState, result.ModelState);

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

        private static ModelStateDictionary CreateModelState()
        {
            return new ModelStateDictionary();
        }

        private static ModelStateDictionary CreateModelStateWithError()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError(String.Empty, String.Empty);
            return modelState;
        }

        private static InvalidModelStateResult CreateProductUnderTest(ModelStateDictionary modelState,
            bool includeErrorDetail, IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
        {
            return new InvalidModelStateResult(modelState, includeErrorDetail, contentNegotiator,
                request, formatters);
        }

        private static InvalidModelStateResult CreateProductUnderTest(ModelStateDictionary modelState,
            ApiController controller)
        {
            return new InvalidModelStateResult(modelState, controller);
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
