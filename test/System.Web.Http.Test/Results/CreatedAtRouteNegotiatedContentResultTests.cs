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
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Results
{
    public class CreatedAtRouteNegotiatedContentResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenRouteNameIsNull()
        {
            // Arrange
            string routeName = null;
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();
            UrlHelper urlFactory = CreateDummyUrlFactory();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, content, urlFactory, contentNegotiator, request,
                        formatters);
                }, "routeName");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenUrlFactoryIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();
            UrlHelper urlFactory = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, content, urlFactory, contentNegotiator, request,
                        formatters);
                }, "urlFactory");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenContentNegotiatorIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, content, urlFactory, contentNegotiator, request,
                        formatters);
                }, "contentNegotiator");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();
            HttpRequestMessage request = null;
            IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(routeName, routeValues, content, urlFactory, contentNegotiator, request,
                    formatters);
            }, "request");
        }

        [Fact]
        public void Constructor_Throws_WhenFormattersIsNull()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(routeName, routeValues, content, urlFactory, contentNegotiator, request,
                        formatters);
                }, "formatters");
            }
        }

        [Fact]
        public void RouteName_Returns_InstanceProvided()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(expectedRouteName,
                    routeValues, content, urlFactory, contentNegotiator, request, formatters);

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
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName,
                    expectedRouteValues, content, urlFactory, contentNegotiator, request, formatters);

                // Act
                IDictionary<string, object> routeValues = result.RouteValues;

                // Assert
                Assert.Same(expectedRouteValues, routeValues);
            }
        }

        [Fact]
        public void Content_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object expectedContent = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    expectedContent, urlFactory, contentNegotiator, request, formatters);

                // Act
                object content = result.Content;

                // Assert
                Assert.Same(expectedContent, content);
            }
        }

        [Fact]
        public void UrlFactory_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper expectedUrlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, expectedUrlFactory, contentNegotiator, request, formatters);

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void ContentNegotiator_Returns_InstanceProvided()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, urlFactory, expectedContentNegotiator, request, formatters);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> formatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, urlFactory, contentNegotiator, expectedRequest, formatters);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpRequestMessage request = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, urlFactory, contentNegotiator, request, expectedFormatters);

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
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            object expectedContent = CreateContent();
            Mock<UrlHelper> spyUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            string expectedLocation = CreateLocation().AbsoluteUri;
            spyUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns(expectedLocation);
            UrlHelper urlFactory = spyUrlFactory.Object;
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ContentNegotiationResult negotiationResult = new ContentNegotiationResult(expectedFormatter,
                expectedMediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spyContentNegotiator = new Mock<IContentNegotiator>();
                spyContentNegotiator.Setup(n => n.Negotiate(typeof(object), expectedRequest, expectedFormatters))
                    .Returns(negotiationResult);
                IContentNegotiator contentNegotiator = spyContentNegotiator.Object;

                IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues,
                    expectedContent, urlFactory, contentNegotiator, expectedRequest, expectedFormatters);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Same(expectedLocation, response.Headers.Location.OriginalString);
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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            UrlHelper urlFactory = CreateDummyUrlFactory();
            ContentNegotiationResult negotiationResult = null;

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spy = new Mock<IContentNegotiator>();
                spy.Setup(n => n.Negotiate(typeof(object), expectedRequest, expectedFormatters)).Returns(
                    negotiationResult);
                IContentNegotiator contentNegotiator = spy.Object;

                IHttpActionResult result = CreateProductUnderTest(routeName, routeValues, content, urlFactory,
                    contentNegotiator, expectedRequest, expectedFormatters);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
                    Assert.Null(response.Headers.Location);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public async Task ExecuteAsync_Throws_WhenUrlHelperLinkReturnsNull_AfterContentNegotiationSucceeds()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            object expectedContent = CreateContent();
            Mock<UrlHelper> stubUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            stubUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns((string)null);
            UrlHelper urlFactory = stubUrlFactory.Object;
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            MediaTypeHeaderValue expectedMediaType = CreateMediaType();
            ContentNegotiationResult negotiationResult = new ContentNegotiationResult(expectedFormatter,
                expectedMediaType);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IEnumerable<MediaTypeFormatter> expectedFormatters = CreateFormatters();

                Mock<IContentNegotiator> spyContentNegotiator = new Mock<IContentNegotiator>();
                spyContentNegotiator.Setup(n => n.Negotiate(typeof(object), expectedRequest, expectedFormatters))
                    .Returns(negotiationResult);
                IContentNegotiator contentNegotiator = spyContentNegotiator.Object;

                IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues,
                    expectedContent, urlFactory, contentNegotiator, expectedRequest, expectedFormatters);

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
            object content = CreateContent();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(routeName, routeValues, content, controller);
            }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse_WhenContentNegotationSucceeds()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            object expectedContent = CreateContent();
            Mock<UrlHelper> spyUrlFactory = new Mock<UrlHelper>(MockBehavior.Strict);
            string expectedLocation = CreateLocation().AbsoluteUri;
            spyUrlFactory.Setup(f => f.Link(expectedRouteName, expectedRouteValues)).Returns(expectedLocation);
            UrlHelper urlFactory = spyUrlFactory.Object;
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
                    controller.Url = urlFactory;

                    IHttpActionResult result = CreateProductUnderTest(expectedRouteName, expectedRouteValues,
                        expectedContent, controller);

                    // Act
                    Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                    // Assert
                    Assert.NotNull(task);

                    using (HttpResponseMessage response = await task)
                    {
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                        Assert.Same(expectedLocation, response.Headers.Location.OriginalString);
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
        public void UrlFactory_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;
                controller.Url = CreateDummyUrlFactory();

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName,
                    routeValues, content, controller);

                UrlHelper expectedUrlFactory = CreateDummyUrlFactory();
                controller.Url = expectedUrlFactory;

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesLazily()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                using (HttpRequestMessage request = CreateRequest())
                {
                    controller.Request = request;

                    CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName,
                        routeValues, content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            MediaTypeFormatter formatter = CreateFormatter();
            MediaTypeHeaderValue mediaType = CreateMediaType();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(CreateFormatter(), contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
        public void UrlFactory_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;
                UrlHelper expectedUrlFactory = CreateDummyUrlFactory();
                controller.Url = expectedUrlFactory;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

                UrlHelper ignore = result.UrlFactory;

                controller.Url = CreateDummyUrlFactory();

                // Act
                UrlHelper urlFactory = result.UrlFactory;

                // Assert
                Assert.Same(expectedUrlFactory, urlFactory);
            }
        }

        [Fact]
        public void ContentNegotiator_ForApiController_EvaluatesOnce()
        {
            // Arrange
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            IContentNegotiator expectedContentNegotiator = CreateDummyContentNegotiator();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), expectedContentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();
            MediaTypeFormatter expectedFormatter = CreateFormatter();
            IContentNegotiator contentNegotiator = CreateDummyContentNegotiator();

            using (HttpConfiguration earlyConfiguration = CreateConfiguration(expectedFormatter, contentNegotiator))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = earlyConfiguration;
                controller.Request = request;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();
            HttpControllerContext context = new HttpControllerContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                controller.ControllerContext = context;
                controller.Request = request;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(), null))
            using (HttpRequestMessage request = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = request;

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

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
            string routeName = CreateRouteName();
            IDictionary<string, object> routeValues = CreateRouteValues();
            object content = CreateContent();
            ApiController controller = CreateController();

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            {
                controller.Configuration = configuration;
                Assert.Null(controller.Request);

                CreatedAtRouteNegotiatedContentResult<object> result = CreateProductUnderTest(routeName, routeValues,
                    content, controller);

                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
            }
        }

        [Fact]
        public void ApiControllerCreatedAtRoute_WithStringAndDictionary_CreatesCorrectResult()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            IDictionary<string, object> expectedRouteValues = CreateRouteValues();
            object expectedContent = CreateContent();
            ApiController controller = CreateController();

            // Act
            CreatedAtRouteNegotiatedContentResult<object> result = controller.CreatedAtRoute(expectedRouteName,
                expectedRouteValues, expectedContent);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedRouteName, result.RouteName);
            Assert.Same(expectedRouteValues, result.RouteValues);
            Assert.Same(expectedContent, result.Content);

            using (HttpConfiguration configuration = CreateConfiguration(CreateFormatter(),
                CreateDummyContentNegotiator()))
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Configuration = configuration;
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerCreatedAtRoute_WithStringAndObject_CreatesCorrectResult()
        {
            // Arrange
            string expectedRouteName = CreateRouteName();
            object routeValues = new { id = 1 };
            object expectedContent = CreateContent();
            ApiController controller = CreateController();

            // Act
            CreatedAtRouteNegotiatedContentResult<object> result = controller.CreatedAtRoute(expectedRouteName,
                routeValues, expectedContent);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedRouteName, result.RouteName);
            Assert.IsType<HttpRouteValueDictionary>(result.RouteValues);
            Assert.True(result.RouteValues.ContainsKey("id"));
            Assert.Equal(1, result.RouteValues["id"]);
            Assert.Same(expectedContent, result.Content);

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

        private static object CreateContent()
        {
            return new object();
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static IContentNegotiator CreateDummyContentNegotiator()
        {
            return new Mock<IContentNegotiator>(MockBehavior.Strict).Object;
        }

        private static UrlHelper CreateDummyUrlFactory()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static MediaTypeFormatter CreateFormatter()
        {
            return new StubMediaTypeFormatter();
        }

        private static IEnumerable<MediaTypeFormatter> CreateFormatters()
        {
            return new MediaTypeFormatter[0];
        }

        private static Uri CreateLocation()
        {
            return new Uri("aa://b");
        }

        private static MediaTypeHeaderValue CreateMediaType()
        {
            return new MediaTypeHeaderValue("text/plain");
        }

        private static CreatedAtRouteNegotiatedContentResult<object> CreateProductUnderTest(string routeName,
            IDictionary<string, object> routeValues, object content, UrlHelper urlFactory,
            IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
        {
            return new CreatedAtRouteNegotiatedContentResult<object>(routeName, routeValues, content, urlFactory,
                contentNegotiator, request, formatters);
        }

        private static CreatedAtRouteNegotiatedContentResult<object> CreateProductUnderTest(string routeName,
            IDictionary<string, object> routeValues, object content, ApiController controller)
        {
            return new CreatedAtRouteNegotiatedContentResult<object>(routeName, routeValues, content, controller);
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
