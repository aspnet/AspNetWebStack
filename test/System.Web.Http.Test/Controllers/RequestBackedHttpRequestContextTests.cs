// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class RequestBackedHttpRequestContextTests
    {
        [Fact]
        public void ConstructorWithRequest_Throws_WhenRequestIsNull()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => new RequestBackedHttpRequestContext(request), "request");
        }

        [Fact]
        public void RequestGet_ReturnsInstanceProvidedInConstructorWithRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                RequestBackedHttpRequestContext context = CreateProductUnderTest(expectedRequest);

                // Act
                HttpRequestMessage request = context.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void RequestSet_UpdatesRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                RequestBackedHttpRequestContext context = CreateProductUnderTest();

                // Act
                context.Request = expectedRequest;

                // Assert
                Assert.Same(expectedRequest, context.Request);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            X509Certificate2 certificate = context.ClientCertificate;

            // Assert
            Assert.Null(certificate);
        }

        [Fact]
        public void ClientCertificateSet_UpdatesClientCertificate()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();
            X509Certificate2 expectedCertificate = CreateCertificate();

            // Act
            context.ClientCertificate = expectedCertificate;

            // Assert
            X509Certificate2 certificate = context.ClientCertificate;
            Assert.Same(expectedCertificate, certificate);
        }

        [Fact]
        public void ClientCertificateGet_ReturnsClientCertificateFromProperty_WhenRequestIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                X509Certificate2 expectedCertificate = CreateCertificate();
                request.Properties[HttpPropertyKeys.ClientCertificateKey] = expectedCertificate;

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_IgnoresRequest_AfterClientCertificateSet()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                X509Certificate2 expectedCertificate = CreateCertificate();
                context.ClientCertificate = expectedCertificate;
                request.Properties[HttpPropertyKeys.ClientCertificateKey] = CreateCertificate();

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_IgnoresRequest_AfterClientCertificateSetNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.ClientCertificate = null;
                request.Properties[HttpPropertyKeys.ClientCertificateKey] = CreateCertificate();

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ConfigurationGet_ReturnsNull_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            HttpConfiguration configuration = context.Configuration;

            // Assert
            Assert.Null(configuration);
        }

        [Fact]
        public void ConfigurationSet_UpdatesConfiguration()
        {
            // Arrange
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest();

                // Act
                context.Configuration = expectedConfiguration;

                // Assert
                HttpConfiguration configuration = context.Configuration;
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationGet_ReturnsConfigurationFromProperty_WhenRequestIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = expectedConfiguration;

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationGet_IgnoresRequest_AfterConfigurationSet()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            using (HttpConfiguration otherConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.Configuration = expectedConfiguration;
                request.SetConfiguration(otherConfiguration);

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationGet_IgnoresRequest_AfterConfigurationSetNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration otherConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.Configuration = null;
                request.SetConfiguration(otherConfiguration);

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.Null(configuration);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsFalse_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            bool includeErrorDetail = context.IncludeErrorDetail;

            // Assert
            Assert.False(includeErrorDetail);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailSet_UpdatesIncludeErrorDetail(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            context.IncludeErrorDetail = expectedIncludeErrorDetail;

            // Assert
            bool includeErrorDetail = context.IncludeErrorDetail;
            Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsIncludeErrorDetailFromProperty_WhenRequestIsPresent(bool expected)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                request.Properties[HttpPropertyKeys.IncludeErrorDetailKey] = new Lazy<bool>(() => expected);

                // Act
                bool actual = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_IgnoresRequest_AfterIncludeErrorDetailSet(bool expectedIncludeErrorDetail)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.IncludeErrorDetail = expectedIncludeErrorDetail;
                request.Properties[HttpPropertyKeys.IncludeErrorDetailKey] =
                    new Lazy<bool>(() => !expectedIncludeErrorDetail);

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Fact]
        public void IsLocalGet_ReturnsFalse_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            bool isLocal = context.IsLocal;

            // Assert
            Assert.False(isLocal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalSet_UpdatesIsLocal(bool expectedIsLocal)
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            context.IsLocal = expectedIsLocal;

            // Assert
            bool isLocal = context.IsLocal;
            Assert.Equal(expectedIsLocal, isLocal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalGet_ReturnsIsLocalFromProperty_WhenRequestIsPresent(bool expected)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                request.Properties[HttpPropertyKeys.IsLocalKey] = new Lazy<bool>(() => expected);

                // Act
                bool actual = context.IsLocal;

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalGet_IgnoresRequest_AfterIsLocalSet(bool expectedIsLocal)
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.IsLocal = expectedIsLocal;
                request.Properties[HttpPropertyKeys.IsLocalKey] =
                    new Lazy<bool>(() => !expectedIsLocal);

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalGet_ReturnsThreadCurrentPrincipalFromConstructor()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            Thread.CurrentPrincipal = expectedPrincipal;

            // Act
            HttpRequestContext context = CreateProductUnderTest();

            // Assert
            Thread.CurrentPrincipal = CreateDummyPrincipal();
            Assert.Same(expectedPrincipal, context.Principal);
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalGet_ReturnsThreadCurrentPrincipalFromConstructorWithRequest()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                IPrincipal expectedPrincipal = CreateDummyPrincipal();
                Thread.CurrentPrincipal = expectedPrincipal;

                // Act
                HttpRequestContext context = CreateProductUnderTest(request);

                // Assert
                Thread.CurrentPrincipal = CreateDummyPrincipal();
                Assert.Same(expectedPrincipal, context.Principal);
            }
        }

        [Fact]
        public void PrincipalSet_UpdatesPrincipal()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();

            // Act
            context.Principal = expectedPrincipal;

            // Assert
            Assert.Same(expectedPrincipal, context.Principal);
        }

        [Fact]
        public void RouteDataGet_ReturnsNull_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            IHttpRouteData routeData = context.RouteData;

            // Assert
            Assert.Null(routeData);
        }

        [Fact]
        public void RouteDataSet_UpdatesRouteData()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();
            IHttpRouteData expectedRouteData = CreateDummyRouteData();

            // Act
            context.RouteData = expectedRouteData;

            // Assert
            IHttpRouteData routeData = context.RouteData;
            Assert.Same(expectedRouteData, routeData);
        }

        [Fact]
        public void RouteDataGet_ReturnsRouteDataFromProperty_WhenRequestIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                IHttpRouteData expectedRouteData = CreateDummyRouteData();
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = expectedRouteData;

                // Act
                IHttpRouteData routeData = context.RouteData;

                // Assert
                Assert.Same(expectedRouteData, routeData);
            }
        }

        [Fact]
        public void RouteDataGet_IgnoresRequest_AfterRouteDataSet()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                IHttpRouteData expectedRouteData = CreateDummyRouteData();
                context.RouteData = expectedRouteData;
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = CreateDummyRouteData();

                // Act
                IHttpRouteData routeData = context.RouteData;

                // Assert
                Assert.Same(expectedRouteData, routeData);
            }
        }

        [Fact]
        public void RouteDataGet_IgnoresRequest_AfterRouteDataSetNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.RouteData = null;
                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = CreateDummyRouteData();

                // Act
                IHttpRouteData routeData = context.RouteData;

                // Assert
                Assert.Null(routeData);
            }
        }

        [Fact]
        public void UrlGet_ReturnsNull_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            UrlHelper url = context.Url;

            // Assert
            Assert.Null(url);
        }

        [Fact]
        public void UrlSet_UpdatesUrl()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();
            UrlHelper expectedUrl = CreateUrlHelper();

            // Act
            context.Url = expectedUrl;

            // Assert
            UrlHelper url = context.Url;
            Assert.Same(expectedUrl, url);
        }

        [Fact]
        public void UrlGet_ReturnsNewInstanceForRequest_WhenRequestIsPresent()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(expectedRequest);

                // Act
                UrlHelper url = context.Url;

                // Assert
                Assert.NotNull(url);
                Assert.Same(expectedRequest, url.Request);
            }
        }

        [Fact]
        public void UrlGet_IgnoresRequest_AfterUrlSet()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                UrlHelper expectedUrl = CreateUrlHelper();
                context.Url = expectedUrl;

                // Act
                UrlHelper url = context.Url;

                // Assert
                Assert.Same(expectedUrl, url);
            }
        }

        [Fact]
        public void UrlGet_IgnoresRequest_AfterUrlSetNull()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(request);
                context.Url = null;

                // Act
                UrlHelper url = context.Url;

                // Assert
                Assert.Null(url);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsNull_AsDefault()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();

            // Act
            string virtualPathRoot = context.VirtualPathRoot;

            // Assert
            Assert.Null(virtualPathRoot);
        }

        [Fact]
        public void VirtualPathRootSet_UpdatesVirtualPathRoot()
        {
            // Arrange
            HttpRequestContext context = CreateProductUnderTest();
            string expectedVirtualPathRoot = "/";

            // Act
            context.VirtualPathRoot = expectedVirtualPathRoot;

            // Assert
            string virtualPathRoot = context.VirtualPathRoot;
            Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsConfigurationVirtualPathRoot_WhenConfigurationIsPresent()
        {
            // Arrange
            string expectedVirtualPathRoot = "/";
            
            using (HttpConfiguration configuration = new HttpConfiguration(new HttpRouteCollection(
                expectedVirtualPathRoot)))
            {
                HttpRequestContext context = CreateProductUnderTest();
                context.Configuration = configuration;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_IgnoresConfiguration_AfterVirtualPathRootSet()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration(new HttpRouteCollection("other")))
            {
                HttpRequestContext context = CreateProductUnderTest();
                context.Configuration = configuration;
                string expectedVirtualPathRoot = "/";
                context.VirtualPathRoot = expectedVirtualPathRoot;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_IgnoresConfiguration_AfterVirtualPathRootSetNull()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration(new HttpRouteCollection("other")))
            {
                HttpRequestContext context = CreateProductUnderTest();
                context.Configuration = configuration;
                context.VirtualPathRoot = null;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Null(virtualPathRoot);
            }
        }

        private static X509Certificate2 CreateCertificate()
        {
            return new X509Certificate2();
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static IHttpRouteData CreateDummyRouteData()
        {
            return new Mock<IHttpRouteData>(MockBehavior.Strict).Object;
        }

        private static RequestBackedHttpRequestContext CreateProductUnderTest()
        {
            return new RequestBackedHttpRequestContext();
        }

        private static RequestBackedHttpRequestContext CreateProductUnderTest(HttpRequestMessage request)
        {
            return new RequestBackedHttpRequestContext(request);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static UrlHelper CreateUrlHelper()
        {
            return new UrlHelper();
        }
    }
}
