// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class HttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetRequestContext_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.GetRequestContext(), "request");
        }

        [Fact]
        public void GetRequestContext_ReturnsProperty()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext expectedContext = CreateContext();
                request.Properties[HttpPropertyKeys.RequestContextKey] = expectedContext;

                // Act
                HttpRequestContext context = request.GetRequestContext();

                // Assert
                Assert.Same(expectedContext, context);
            }
        }

        [Fact]
        public void SetRequestContext_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;
            HttpRequestContext context = CreateContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { request.SetRequestContext(context); }, "request");
        }

        [Fact]
        public void SetRequestContext_Throws_WhenContextIsNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => { request.SetRequestContext(context); }, "context");
            }
        }

        [Fact]
        public void SetRequestContext_AddsProperty()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext expectedContext = CreateContext();

                // Act
                request.SetRequestContext(expectedContext);

                // Assert
                Assert.Same(expectedContext, request.Properties[HttpPropertyKeys.RequestContextKey]);
            }
        }

        [Fact]
        public void GetConfigurationThrowsOnNull()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetConfiguration(), "request");
        }

        [Fact]
        public void GetConfiguration_ReturnsConfigurationFromContext_WhenOnlyContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                request.SetRequestContext(new HttpRequestContext
                {
                    Configuration = expectedConfiguration
                });

                // Act
                HttpConfiguration configuration = request.GetConfiguration();

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void GetConfiguration_ReturnsConfigurationFromProperty_WhenOnlyPropertyIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = expectedConfiguration;

                // Act
                HttpConfiguration configuration = request.GetConfiguration();

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void GetConfiguration_ReturnsConfigurationFromContext_WhenBothContextAndPropertyArePresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            using (HttpConfiguration otherConfiguration = CreateConfiguration())
            {
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = otherConfiguration;
                request.SetRequestContext(new HttpRequestContext
                {
                    Configuration = expectedConfiguration
                });

                // Act
                HttpConfiguration configuration = request.GetConfiguration();

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void SetConfiguration_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;

            Assert.ThrowsArgumentNull(
                () => request.SetConfiguration(CreateConfiguration()),
                "request");
        }

        [Fact]
        public void SetConfiguration_ThrowsArgumentNull_Configuration()
        {
            HttpRequestMessage request = CreateRequest();

            Assert.ThrowsArgumentNull(
                () => request.SetConfiguration(null),
                "configuration");
        }

        [Fact]
        public void SetConfiguration_AddsProperty()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                // Act
                request.SetConfiguration(expectedConfiguration);

                // Assert
                Assert.Same(expectedConfiguration, request.Properties[HttpPropertyKeys.HttpConfigurationKey]);
            }
        }

        [Fact]
        public void SetConfiguration_UpdatesContextAndAddsProperty_WhenContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateContext();
                request.SetRequestContext(context);

                // Act
                request.SetConfiguration(expectedConfiguration);

                // Assert
                Assert.Same(expectedConfiguration, context.Configuration);
                Assert.Same(expectedConfiguration, request.Properties[HttpPropertyKeys.HttpConfigurationKey]);
            }
        }

        [Fact]
        public void GetSynchronizationContextThrowsOnNull()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetSynchronizationContext(), "request");
        }

        [Fact]
        public void GetSynchronizationContext()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            Mock<SynchronizationContext> syncContextMock = new Mock<SynchronizationContext>();
            SynchronizationContext beforeSyncContext = syncContextMock.Object;
            request.Properties.Add(HttpPropertyKeys.SynchronizationContextKey, beforeSyncContext);

            // Act
            SynchronizationContext afterSyncContext = request.GetSynchronizationContext();

            // Assert
            Assert.Same(beforeSyncContext, afterSyncContext);
        }

        [Fact]
        public void GetRouteData_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.GetRouteData(), "request");
        }

        [Fact]
        public void GetRouteData_ReturnsRouteDataFromContext_WhenOnlyContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpRouteData expectedRouteData = CreateDummyRouteData();
                request.SetRequestContext(new HttpRequestContext
                {
                    RouteData = expectedRouteData
                });

                // Act
                IHttpRouteData routeData = request.GetRouteData();

                // Assert
                Assert.Same(expectedRouteData, routeData);
            }
        }

        [Fact]
        public void GetRouteData_ReturnsRouteDataFromProperty_WhenOnlyPropertyIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpRouteData expectedRouteData = CreateDummyRouteData();
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = expectedRouteData;

                // Act
                IHttpRouteData routeData = request.GetRouteData();

                // Assert
                Assert.Same(expectedRouteData, routeData);
            }
        }

        [Fact]
        public void GetRouteData_ReturnsRouteDataFromContext_WhenBothContextAndPropertyArePresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpRouteData expectedRouteData = CreateDummyRouteData();
                request.SetRequestContext(new HttpRequestContext
                {
                    RouteData = expectedRouteData
                });
                IHttpRouteData otherRouteData = CreateDummyRouteData();
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = otherRouteData;

                // Act
                IHttpRouteData routeData = request.GetRouteData();

                // Assert
                Assert.Same(expectedRouteData, routeData);
            }
        }

        [Fact]
        public void SetRouteData_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;
            IHttpRouteData routeData = CreateDummyRouteData();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.SetRouteData(routeData), "request");
        }

        [Fact]
        public void SetRouteData_Throws_WhenRouteDataIsNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpRouteData routeData = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => request.SetRouteData(routeData), "routeData");
            }
        }

        [Fact]
        public void SetRouteData_AddsProperty()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpRouteData expectedRouteData = CreateDummyRouteData();

                // Act
                request.SetRouteData(expectedRouteData);

                // Assert
                Assert.Same(expectedRouteData, request.Properties[HttpPropertyKeys.HttpRouteDataKey]);
            }
        }

        [Fact]
        public void SetRouteData_UpdatesContextAndAddsProperty_WhenContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateContext();
                request.SetRequestContext(context);
                IHttpRouteData expectedRouteData = CreateDummyRouteData();

                // Act
                request.SetRouteData(expectedRouteData);

                // Assert
                Assert.Same(expectedRouteData, context.RouteData);
                Assert.Same(expectedRouteData, request.Properties[HttpPropertyKeys.HttpRouteDataKey]);
            }
        }

        [Fact]
        public void GetUrlHelper_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.GetUrlHelper(), "request");
        }

        [Fact]
        public void GetUrlHelper_ReturnsUrlFromContext_WhenContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                UrlHelper expectedUrl = new Mock<UrlHelper>().Object;
                request.SetRequestContext(new HttpRequestContext
                {
                    Url = expectedUrl
                });

                // Act
                UrlHelper url = request.GetUrlHelper();

                // Assert
                Assert.Same(expectedUrl, url);
            }
        }

        [Fact]
        public void GetUrlHelper_ReturnsNewUrlHelperForRequest_WhenContextIsAbsent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                // Act
                UrlHelper urlHelper = request.GetUrlHelper();

                // Assert
                Assert.NotNull(urlHelper);
                Assert.Same(request, urlHelper.Request);
            }
        }

        [Fact]
        public void GetClientCertificate_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.GetClientCertificate(), "request");
        }

        [Fact]
        public void GetClientCertificate_ReturnsClientCertificateFromContext_WhenOnlyContextIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                X509Certificate2 expectedCertificate = CreateCertificate();
                request.SetRequestContext(new HttpRequestContext
                {
                    ClientCertificate = expectedCertificate
                });

                // Act
                X509Certificate2 certificate = request.GetClientCertificate();

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void GetClientCertificate_ReturnsClientCertificateFromProperty_WhenOnlyPropertyIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                X509Certificate2 expectedCertificate = CreateCertificate();
                request.Properties[HttpPropertyKeys.ClientCertificateKey] = expectedCertificate;

                // Act
                X509Certificate2 certificate = request.GetClientCertificate();

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void GetClientCertificate_ReturnsClientCertificateFromContext_WhenBothContextAndPropertyArePresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                X509Certificate2 expectedCertificate = CreateCertificate();
                request.SetRequestContext(new HttpRequestContext
                {
                    ClientCertificate = expectedCertificate
                });
                X509Certificate2 otherCertificate = CreateCertificate();
                request.Properties[HttpPropertyKeys.ClientCertificateKey] = otherCertificate;

                // Act
                X509Certificate2 certificate = request.GetClientCertificate();

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void CreateResponse_OnNullRequest_ThrowsException()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(CreateValue());
            }, "request");

            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(HttpStatusCode.OK, CreateValue());
            }, "request");

            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(HttpStatusCode.OK, CreateValue(), configuration: null);
            }, "request");
        }

        [Fact]
        public void CreateResponse_OnNullConfiguration_ThrowsException()
        {
            HttpRequestMessage request = CreateRequest();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                request.CreateResponse(CreateValue());
            }, "The request does not have an associated configuration object or the provided configuration was null.");

            Assert.Throws<InvalidOperationException>(() =>
            {
                request.CreateResponse(HttpStatusCode.OK, CreateValue(), configuration: null);
            }, "The request does not have an associated configuration object or the provided configuration was null.");
        }

        [Fact]
        public void CreateResponse_DoingConneg_OnlyContent_RetrievesContentNegotiatorFromServiceResolver()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            servicesMock.Setup(s => s.GetService(typeof(IContentNegotiator)))
                        .Returns(new Mock<IContentNegotiator>().Object)
                        .Verifiable();
            config.Services = servicesMock.Object;

            // Act
            request.CreateResponse(CreateValue());

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void CreateResponse_DoingConneg_RetrievesContentNegotiatorFromServiceResolver()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            servicesMock.Setup(s => s.GetService(typeof(IContentNegotiator)))
                        .Returns(new Mock<IContentNegotiator>().Object)
                        .Verifiable();
            config.Services = servicesMock.Object;

            // Act
            request.CreateResponse(HttpStatusCode.OK, CreateValue(), config);

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void CreateResponse_DoingConneg_WhenNoContentNegotiatorInstanceRegistered_Throws()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            config.Services.Clear(typeof(IContentNegotiator));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => request.CreateResponse(CreateValue()),
                "The provided configuration does not have an instance of the 'System.Net.Http.Formatting.IContentNegotiator' service registered.");

            Assert.Throws<InvalidOperationException>(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), config),
                "The provided configuration does not have an instance of the 'System.Net.Http.Formatting.IContentNegotiator' service registered.");
        }

        [Fact]
        public void CreateResponse_DoingConneg_WhenContentNegotiatorReturnsNullResult_Throws()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            Mock<IContentNegotiator> negotiatorMock = new Mock<IContentNegotiator>();
            negotiatorMock.Setup(r => r.Negotiate(typeof(string), request, config.Formatters)).Returns(value: null);
            config.Services.Replace(typeof(IContentNegotiator), negotiatorMock.Object);

            // Act
            var response = request.CreateResponse<string>(HttpStatusCode.OK, "", config);

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
            Assert.Same(request, response.RequestMessage);
        }

        [Fact]
        public void CreateResponse_DoingConneg_PerformsContentNegotiationAndCreatesContentUsingResults()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            Mock<IContentNegotiator> negotiatorMock = new Mock<IContentNegotiator>();
            negotiatorMock.Setup(r => r.Negotiate(typeof(string), request, config.Formatters))
                        .Returns(new ContentNegotiationResult(formatter, null));
            config.Services.Replace(typeof(IContentNegotiator), negotiatorMock.Object);

            // Act
            var response = request.CreateResponse<string>(HttpStatusCode.NoContent, "42", config);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Same(request, response.RequestMessage);
            var objectContent = Assert.IsType<ObjectContent<string>>(response.Content);
            Assert.Equal("42", objectContent.Value);
            Assert.Same(formatter, objectContent.Formatter);
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), "foo/bar"), "request");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeHeaderIsNull_Throws()
        {
            HttpRequestMessage request = CreateRequest();

            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), (MediaTypeHeaderValue)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsNull_Throws()
        {
            HttpRequestMessage request = CreateRequest();

            Assert.ThrowsArgument(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), (string)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsEmpty_Throws()
        {
            HttpRequestMessage request = CreateRequest();

            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), (MediaTypeHeaderValue)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsInvalidFormat_Throws()
        {
            HttpRequestMessage request = CreateRequest();

            Assert.Throws<FormatException>(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), "foo/bar; param=value"), "The format of value 'foo/bar; param=value' is invalid.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenRequestDoesNotHaveConfiguration_Throws()
        {
            HttpRequestMessage request = CreateRequest();

            // Arrange
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = null;

            // Act
            Assert.Throws<InvalidOperationException>(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), mediaType: "foo/bar"),
                "The request does not have an associated configuration object or the provided configuration was null.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeDoesNotMatch_Throws()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.SetConfiguration(new HttpConfiguration());

            // Act
            Assert.Throws<InvalidOperationException>(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), mediaType: "foo/bar"),
                "Could not find a formatter matching the media type 'foo/bar' that can write an instance of 'Object'.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_FindsMatchingFormatterAndCreatesResponse()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            var config = new HttpConfiguration();
            request.SetConfiguration(config);
            config.Formatters.Clear();
            Mock<MediaTypeFormatter> formatterMock = new Mock<MediaTypeFormatter> { CallBase = true };
            var formatter = formatterMock.Object;
            formatterMock.Setup(f => f.CanWriteType(typeof(object))).Returns(true).Verifiable();
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));
            config.Formatters.Add(formatter);
            object expectedValue = CreateValue();

            // Act
            var response = request.CreateResponse(HttpStatusCode.Gone, expectedValue, mediaType: "foo/bar");

            // Assert
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            var content = Assert.IsType<ObjectContent<object>>(response.Content);
            Assert.Same(expectedValue, content.Value);
            Assert.Same(formatter, content.Formatter);
            Assert.Equal("foo/bar", content.Headers.ContentType.MediaType);
            formatterMock.Verify();
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), new MockMediaTypeFormatter()), "request");
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WhenFormatterIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();

            // Act
            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, CreateValue(), formatter: null), "formatter");
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_CreatesResponseWithDefaultMediaType()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));
            object expectedValue = CreateValue();

            // Act
            var response = request.CreateResponse(HttpStatusCode.MultipleChoices, expectedValue, formatter, mediaType: (string)null);

            // Assert
            Assert.Equal(HttpStatusCode.MultipleChoices, response.StatusCode);
            var content = Assert.IsType<ObjectContent<object>>(response.Content);
            Assert.Same(expectedValue, content.Value);
            Assert.Same(formatter, content.Formatter);
            Assert.Equal("foo/bar", content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WithOverridenMediaTypeString_CreatesResponse()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));

            // Act
            var response = request.CreateResponse(HttpStatusCode.MultipleChoices, CreateValue(), formatter, mediaType: "bin/baz");

            // Assert
            Assert.Equal("bin/baz", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WithOverridenMediaTypeHeader_CreatesResponse()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));

            // Act
            var response = request.CreateResponse(HttpStatusCode.MultipleChoices, CreateValue(), formatter, mediaType: new MediaTypeHeaderValue("bin/baz"));

            // Assert
            Assert.Equal("bin/baz", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void RegisterForDispose_WhenRequestParameterIsNull_Throws()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.RegisterForDispose(resource: null), "request");
        }

        [Fact]
        public void RegisterForDispose_WhenResourceParamterIsNull_DoesNothing()
        {
            HttpRequestMessage request = CreateRequest();
            request.RegisterForDispose(resource: null);

            Assert.False(request.Properties.ContainsKey(HttpPropertyKeys.DisposableRequestResourcesKey));
        }

        [Fact]
        public void RegisterForDispose_WhenResourceListDoesNotExist_CreatesListAndAddsResource()
        {
            HttpRequestMessage request = CreateRequest();
            request.Properties.Remove(HttpPropertyKeys.DisposableRequestResourcesKey);
            IDisposable disposable = CreateStubDisposable();

            request.RegisterForDispose(disposable);

            var list = Assert.IsType<List<IDisposable>>(request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey]);
            IDisposable item = Assert.Single(list);
            Assert.Same(disposable, item);
        }

        [Fact]
        public void RegisterForDispose_WhenResourceListExists_AddsResource()
        {
            HttpRequestMessage request = CreateRequest();
            var list = new List<IDisposable>();
            request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;
            IDisposable disposable = CreateStubDisposable();

            request.RegisterForDispose(disposable);

            Assert.Same(list, request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey]);
            IDisposable item = Assert.Single(list);
            Assert.Same(disposable, item);
        }

        [Fact]
        public void DisposeRequestResources_WhenRequestParameterIsNull_Throws()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.DisposeRequestResources(), "request");
        }

        [Fact]
        public void DisposeRequestResources_WhenResourceListDoesNotExists_DoesNothing()
        {
            HttpRequestMessage request = CreateRequest();
            request.Properties.Remove(HttpPropertyKeys.DisposableRequestResourcesKey);

            request.DisposeRequestResources();

            Assert.False(request.Properties.ContainsKey(HttpPropertyKeys.DisposableRequestResourcesKey));
        }

        [Fact]
        public void DisposeRequestResources_WhenResourceListExists_DisposesResourceAndClearsReferences()
        {
            HttpRequestMessage request = CreateRequest();
            Mock<IDisposable> disposableMock = new Mock<IDisposable>();
            IDisposable disposable = disposableMock.Object;
            var list = new List<IDisposable> { disposable };
            request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;

            request.DisposeRequestResources();

            disposableMock.Verify(d => d.Dispose());
            Assert.Empty(list);
        }

        [Fact]
        public void DisposeRequestResources_WhenResourcesDisposeMethodThrowsException_IgnoresExceptionsAndContinuesDisposingOtherResources()
        {
            HttpRequestMessage request = CreateRequest();
            Mock<IDisposable> throwingDisposableMock = new Mock<IDisposable>();
            throwingDisposableMock.Setup(d => d.Dispose()).Throws(new Exception());
            Mock<IDisposable> disposableMock = new Mock<IDisposable>();
            IDisposable disposable = disposableMock.Object;
            var list = new List<IDisposable> { throwingDisposableMock.Object, disposable };
            request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;

            request.DisposeRequestResources();

            throwingDisposableMock.Verify(d => d.Dispose());
            disposableMock.Verify(d => d.Dispose());
            Assert.Empty(list);
        }

        [Fact]
        public void CreateErrorResponseRangeNotSatisfiable_ThrowsOnNullException()
        {
            HttpRequestMessage request = CreateRequest();
            Assert.ThrowsArgumentNull(() => request.CreateErrorResponse(invalidByteRangeException: null), "invalidByteRangeException");
        }

        [Fact]
        public void CreateErrorResponseRangeNotSatisfiable_SetsCorrectStatusCodeAndContentRangeHeader()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            ContentRangeHeaderValue expectedContentRange = new ContentRangeHeaderValue(length: 128);
            InvalidByteRangeException invalidByteRangeException = new InvalidByteRangeException(expectedContentRange);

            // Act
            HttpResponseMessage response = request.CreateErrorResponse(invalidByteRangeException);

            // Assert
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.Same(expectedContentRange, response.Content.Headers.ContentRange);
        }

        [Fact]
        public void IsLocal_When_Request_From_Local_Address()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => true));

            // Act
            bool isLocal = request.IsLocal();

            // Assert
            Assert.True(isLocal);
        }

        [Fact]
        public void IsLocal_When_Request_Not_From_Local_Address()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => false));

            // Act
            bool isLocal = request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_With_Property_Value_Null_Returns_False()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, null);

            // Act
            bool isLocal = request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_With_Property_Value_String_Returns_False()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, "Test String");

            // Act
            bool isLocal = request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.IsLocal(), "request");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocal_ReturnsValueFromContext_WhenOnlyContextIsPresent(bool expectedIsLocal)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(new HttpRequestContext
                {
                    IsLocal = expectedIsLocal
                });

                // Act
                bool isLocal = request.IsLocal();

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocal_ReturnsValueFromProperty_WhenOnlyPropertyIsPresent(bool expectedIsLocal)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.Properties[HttpPropertyKeys.IsLocalKey] = new Lazy<bool>(() => expectedIsLocal);

                // Act
                bool isLocal = request.IsLocal();

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocal_ReturnsValueFromContext_WhenBothContextAndPropertyArePresent(bool expectedIsLocal)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(new HttpRequestContext
                {
                    IsLocal = expectedIsLocal
                });
                request.Properties[HttpPropertyKeys.IsLocalKey] = new Lazy<bool>(() => !expectedIsLocal);

                // Act
                bool isLocal = request.IsLocal();

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Fact]
        public void IsLocal_ReturnsFalse_WhenNeitherContextNorPropertyArePresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                // Act
                bool isLocal = request.IsLocal();

                // Assert
                Assert.False(isLocal);
            }
        }

        [Fact]
        public void ShouldIncludeErrorDetail_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.ShouldIncludeErrorDetail(), "request");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIncludeErrorDetail_ReturnsValueFromContext_WhenOnlyContextIsPresent(bool expected)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = expected
                });

                // Act
                bool actual = request.ShouldIncludeErrorDetail();

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIncludeErrorDetail_ReturnsValueFromProperty_WhenOnlyPropertyIsPresent(bool expected)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.Properties[HttpPropertyKeys.IncludeErrorDetailKey] = new Lazy<bool>(() => expected);

                // Act
                bool actual = request.ShouldIncludeErrorDetail();

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIncludeErrorDetail_ReturnsValueFromContext_WhenBothArePresent(bool expected)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = expected
                });
                request.Properties[HttpPropertyKeys.IncludeErrorDetailKey] = new Lazy<bool>(() => !expected);

                // Act
                bool actual = request.ShouldIncludeErrorDetail();

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(IncludeErrorDetailPolicy.Default, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, true, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, false, false)]
        [InlineData(null, false, false, false)]
        public void ShouldIncludeErrorDetail_WhenContextIsAbsent(IncludeErrorDetailPolicy errorDetail, bool isLocal, bool includeErrorDetail, bool expectedResult)
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            HttpConfiguration config = CreateAndAddConfiguration(request);
            config.IncludeErrorDetailPolicy = errorDetail;
            request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => isLocal));
            request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => includeErrorDetail));

            // Act
            bool includeError = request.ShouldIncludeErrorDetail();

            // Assert
            Assert.Equal(includeError, expectedResult);
        }

        [Fact]
        public void ShouldIncludeErrorDetail_Returns_False_WhenConfigIsNull_includeErrorDetail_Null()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();

            // Act
            bool includeError = request.ShouldIncludeErrorDetail();

            // Assert
            Assert.False(includeError);
        }

        [Fact]
        public void ShouldIncludeErrorDetail_Returns_Value_WhenConfigIsNull_includeErrorDetail_HasValue()
        {
            // Arrange
            HttpRequestMessage request = CreateRequest();
            request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => true));

            // Act
            bool includeError = request.ShouldIncludeErrorDetail();

            // Assert
            Assert.True(includeError);
        }

        [Fact]
        public void GetCorrelationId_ReturnsTraceCorrelationManagerId_IfSet()
        {
            Guid traceId = Guid.NewGuid();
            using (var scope = new TraceIdScope(traceId))
            {
                Assert.Equal(traceId, CreateRequest().GetCorrelationId());
            }
        }

        [Fact]
        public void GetCorrelationId_ReturnsNewGuid_IfTraceCorrelationManagerIdNotSet()
        {
            Guid traceId = Guid.Empty;
            using (var scope = new TraceIdScope(traceId))
            {
                Assert.NotEqual(traceId, CreateRequest().GetCorrelationId());
            }
        }

        [Fact]
        public void GetQueryNameValuePairs_ParsesQueryString()
        {
            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=cool");

            // Act
            var actual = request.GetQueryNameValuePairs();

            // Assert
            Assert.IsType<KeyValuePair<string, string>[]>(actual); // We call ToArray to ensure that we're not caching an iterator block.

            Assert.Single(actual, kvp => kvp.Key == "x" && kvp.Value == "7");
            Assert.Single(actual, kvp => kvp.Key == "y" && kvp.Value == "cool");
        }

        [Fact]
        public void GetQueryNameValuePairs_StoresResult()
        {
            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=cool");

            // Act
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));

            Assert.Same(returned, cached);
            Assert.Same(returned, request.GetQueryNameValuePairs());
        }

        [Fact]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChanges()
        {
            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=cool");
            request.GetQueryNameValuePairs();

            // Act
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=verycool");
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString));

            Assert.Same(returned, cached);
            Assert.Same(returned, request.GetQueryNameValuePairs());
            Assert.Same(request.RequestUri.Query, queryString);
        }

        [InlineData("http://localhost/api/Person/10?", "http://localhost/api/Person/10?x=7&y=verycool")]
        [InlineData("http://localhost/api/Person/10", "http://localhost/api/Person/10?x=7&y=verycool")]
        [Theory]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChangesFromEmpty(string firstUri, string secondUri)
        {
            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri(firstUri);
            request.GetQueryNameValuePairs();

            // Act
            request.RequestUri = new Uri(secondUri);
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString));

            Assert.Same(returned, cached);
            Assert.Same(returned, request.GetQueryNameValuePairs());
            Assert.Same(request.RequestUri.Query, queryString);
        }

        [InlineData("http://localhost/api/Person/10?x=7&y=verycool", "http://localhost/api/Person/10?")]
        [InlineData("http://localhost/api/Person/10?x=7&y=verycool", "http://localhost/api/Person/10")]
        [Theory]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChangesToEmpty(string firstUri, string secondUri)
        {
            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri(firstUri);
            IEnumerable<KeyValuePair<string, string>> original = request.GetQueryNameValuePairs();

            // Act
            request.RequestUri = new Uri(secondUri);
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            // Cache is not cleared when the query is empty, but it is when the query is ?.
            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString));

            if (request.RequestUri.Query == "?")
            {
                Assert.Same(returned, cached);
                Assert.Same(request.RequestUri.Query, queryString);
                Assert.Equal(returned, Enumerable.Empty<KeyValuePair<string, string>>());
            }
            else
            {
                Assert.Same(original, cached);
                Assert.NotSame(returned, cached);
                Assert.NotSame(request.RequestUri.Query, queryString);
                Assert.Same(returned, Enumerable.Empty<KeyValuePair<string, string>>());
            }

            Assert.Same(returned, request.GetQueryNameValuePairs());
        }

        [Fact]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChanges_AndUserRemovedCache()
        {
            // This test verifies that users following the existing workaround for the issue
            // https://aspnetwebstack.codeplex.com/workitem/2200 are not broken.

            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=cool");
            request.GetQueryNameValuePairs();
            request.Properties.Remove(HttpPropertyKeys.RequestQueryNameValuePairsKey);

            Assert.False(request.Properties.ContainsKey(HttpPropertyKeys.RequestQueryNameValuePairsKey));

            // Act
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=verycool");
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString));

            Assert.Same(returned, cached);
            Assert.Same(returned, request.GetQueryNameValuePairs());
            Assert.Same(request.RequestUri.Query, queryString);
        }

        [Fact]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChangesToEmpty_AndUserRemovedCache()
        {
            // This test verifies that users following the existing workaround for the issue
            // https://aspnetwebstack.codeplex.com/workitem/2200 are not broken.

            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=cool");
            request.GetQueryNameValuePairs();
            request.Properties.Remove(HttpPropertyKeys.RequestQueryNameValuePairsKey);

            Assert.False(request.Properties.ContainsKey(HttpPropertyKeys.RequestQueryNameValuePairsKey));

            // Act
            request.RequestUri = new Uri("http://localhost/api/Person/10");
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            Assert.False(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString)); // this will not get cleared.

            Assert.Same(returned, Enumerable.Empty<KeyValuePair<string, string>>());
            Assert.Same(returned, request.GetQueryNameValuePairs());
            Assert.NotSame(request.RequestUri.Query, queryString);
        }

        [Fact]
        public void GetQueryNameValuePairs_ReplacesResultWhenUriChangesFromEmpty_AndUserRemovedCache()
        {
            // This test verifies that users following the existing workaround for the issue
            // https://aspnetwebstack.codeplex.com/workitem/2200 are not broken.

            // Arrange
            var request = CreateRequest();
            request.RequestUri = new Uri("http://localhost/api/Person/10");
            request.GetQueryNameValuePairs();
            request.Properties.Remove(HttpPropertyKeys.RequestQueryNameValuePairsKey);

            Assert.False(request.Properties.ContainsKey(HttpPropertyKeys.RequestQueryNameValuePairsKey));

            // Act
            request.RequestUri = new Uri("http://localhost/api/Person/10?x=7&y=verycool");
            var returned = request.GetQueryNameValuePairs();

            // Assert
            IEnumerable<KeyValuePair<string, string>> cached;
            string queryString;

            Assert.True(request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out cached));
            Assert.True(request.Properties.TryGetValue<string>(HttpPropertyKeys.CachedRequestQueryKey, out queryString));

            Assert.Same(returned, cached);
            Assert.Same(returned, request.GetQueryNameValuePairs());
            Assert.Same(request.RequestUri.Query, queryString);
        }

        private class TraceIdScope : IDisposable
        {
            Guid _oldValue;

            public TraceIdScope(Guid traceId)
            {
                _oldValue = Trace.CorrelationManager.ActivityId;
                Trace.CorrelationManager.ActivityId = traceId;
            }

            public void Dispose()
            {
                Trace.CorrelationManager.ActivityId = _oldValue;
            }
        }

        private static HttpConfiguration CreateAndAddConfiguration(HttpRequestMessage request)
        {
            HttpConfiguration configuration = CreateConfiguration();
            request.SetConfiguration(configuration);
            return configuration;
        }

        private static X509Certificate2 CreateCertificate()
        {
            return new X509Certificate2();
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static HttpRequestContext CreateContext()
        {
            return new HttpRequestContext();
        }

        private static IHttpRouteData CreateDummyRouteData()
        {
            return new Mock<IHttpRouteData>(MockBehavior.Strict).Object;
        }

        private static IDisposable CreateStubDisposable()
        {
            return new Mock<IDisposable>().Object;
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static object CreateValue()
        {
            return new object();
        }
    }
}
