// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost
{
    public class WebHostHttpRequestContextTests
    {
        private const string Base64Certificate = "MIFRMIFHAgEAMAIGADAJMQcwBQYAEwFhMB4XDTEzMDkxMDE5NTQ0OVoXDTM5MTIzMT" +
            "IzNTk1OVowCTEHMAUGABMBYTCBBzACBgADgQAwAgYAA4EA";

        [Fact]
        public void ContextGet_ReturnsProvidedInstance()
        {
            // Arrange
            HttpContextBase expectedWebContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                WebHostHttpRequestContext context = CreateProductUnderTest(expectedWebContext, webRequest, request);

                // Act
                HttpContextBase webContext = context.Context;

                // Assert
                Assert.Same(expectedWebContext, webContext);
            }
        }

        [Fact]
        public void WebRequestGet_ReturnsProvidedInstance()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase expectedWebRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                WebHostHttpRequestContext context = CreateProductUnderTest(webContext, expectedWebRequest, request);

                // Act
                HttpRequestBase webRequest = context.WebRequest;

                // Assert
                Assert.Same(expectedWebRequest, webRequest);
            }
        }

        [Fact]
        public void RequestGet_ReturnsProvidedInstance()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                WebHostHttpRequestContext context = CreateProductUnderTest(webContext, webRequest, expectedRequest);

                // Act
                HttpRequestMessage request = context.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_WhenRequestBaseClientCertificateIsNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            Mock<HttpRequestBase> webRequestMock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            webRequestMock.Setup(r => r.ClientCertificate).Returns((HttpClientCertificate)null);
            HttpRequestBase webRequest = webRequestMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_WhenRequestBaseClientCertificateCertificateIsNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpClientCertificate clientCertificate = CreateHttpClientCertificate(null);
            HttpRequestBase webRequest = CreateStubWebRequest(clientCertificate);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_WhenRequestBaseClientCertificateCertificateIsEmpty()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpClientCertificate clientCertificate = CreateHttpClientCertificate(new byte[0]);
            HttpRequestBase webRequest = CreateStubWebRequest(clientCertificate);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsRequestBaseClientCertificate()
        {
            // Arrange
            byte[] expectedCertificateBytes = Convert.FromBase64String(Base64Certificate);
            HttpContextBase webContext = CreateDummyWebContext();
            HttpClientCertificate clientCertificate = CreateHttpClientCertificate(expectedCertificateBytes);
            HttpRequestBase webRequest = CreateStubWebRequest(clientCertificate);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.NotNull(certificate);
                Assert.Equal(expectedCertificateBytes, certificate.RawData);
            }
        }

        [Fact]
        public void ClientCertificateSet_UpdatesClientCertificate()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                X509Certificate2 expectedCertificate = CreateCertificate();

                // Act
                context.ClientCertificate = expectedCertificate;

                // Assert
                X509Certificate2 certificate = context.ClientCertificate;
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void ClientCertificateSet_UpdatesClientCertificate_WhenNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.ClientCertificate = null;

                // Assert
                X509Certificate2 certificate = context.ClientCertificate;
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ConfigurationGet_ReturnsGlobalConfiguration_ByDefault()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.Same(GlobalConfiguration.Configuration, configuration);
            }
        }

        [Fact]
        public void ConfigurationSet_UpdatesConfiguration()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.Configuration = expectedConfiguration;

                // Assert
                HttpConfiguration configuration = context.Configuration;
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationSet_UpdatesConfiguration_WhenNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.Configuration = null;

                // Assert
                HttpConfiguration configuration = context.Configuration;
                Assert.Null(configuration);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsNoCustomErrorEnabled_WhenUnconfigured(bool expectedIncludeErrorDetail)
        {
            // Arrange
            Mock<HttpContextBase> webContextMock = new Mock<HttpContextBase>(MockBehavior.Strict);
            webContextMock.Setup(r => r.IsCustomErrorEnabled).Returns(!expectedIncludeErrorDetail);
            HttpContextBase webContext = webContextMock.Object;
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = null;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsNoCustomErrorEnabled_ForDefaultPolicy(bool expectedIncludeErrorDetail)
        {
            // Arrange
            Mock<HttpContextBase> webContextMock = new Mock<HttpContextBase>(MockBehavior.Strict);
            webContextMock.Setup(r => r.IsCustomErrorEnabled).Returns(!expectedIncludeErrorDetail);
            HttpContextBase webContext = webContextMock.Object;
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsIsLocal_ForLocalOnlyPolicy(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                context.IsLocal = expectedIncludeErrorDetail;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsTrue_ForAlwaysPolicy()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.True(includeErrorDetail);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsFalse_ForNeverPolicy()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.False(includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailSet_UpdatesIncludeErrorDetail(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.IncludeErrorDetail = expectedIncludeErrorDetail;

                // Assert
                bool includeErrorDetail = context.IncludeErrorDetail;
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true, IncludeErrorDetailPolicy.Never)]
        [InlineData(false, IncludeErrorDetailPolicy.Always)]
        public void IncludeErrorDetailSet_OverridesPolicy(bool expected, IncludeErrorDetailPolicy policy)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = policy;

                // Act
                context.IncludeErrorDetail = expected;

                // Assert
                bool includeErrorDetail = context.IncludeErrorDetail;
                Assert.Equal(expected, includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsFirstObservedValue(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                configuration.IncludeErrorDetailPolicy = expectedIncludeErrorDetail
                    ? IncludeErrorDetailPolicy.Always : IncludeErrorDetailPolicy.Never;
                bool ignore = context.IncludeErrorDetail;
                configuration.IncludeErrorDetailPolicy = expectedIncludeErrorDetail
                    ? IncludeErrorDetailPolicy.Never : IncludeErrorDetailPolicy.Always;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
            }
        }

        [Fact]
        public void IsLocalGet_ReturnsFalse_ByDefault()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = new Mock<HttpRequestBase>().Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.False(isLocal);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IsLocal_ReturnsWebRequestIsLocalValue(bool expectedIsLocal)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            Mock<HttpRequestBase> webRequestMock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            webRequestMock.Setup(r => r.IsLocal).Returns(expectedIsLocal);
            HttpRequestBase webRequest = webRequestMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalSet_UpdatesIsLocal(bool expectedIsLocal)
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.IsLocal = expectedIsLocal;

                // Assert
                bool isLocal = context.IsLocal;
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalGet_ReturnsFirstObservedValue(bool expectedIsLocal)
        {
            // Arrange
            bool currentWebRequestIsLocal = expectedIsLocal;
            HttpContextBase webContext = CreateDummyWebContext();
            Mock<HttpRequestBase> webRequestMock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            webRequestMock.Setup(r => r.IsLocal).Returns(() => currentWebRequestIsLocal);
            HttpRequestBase webRequest = webRequestMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                bool ignore = context.IsLocal;
                currentWebRequestIsLocal = !expectedIsLocal;

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Fact]
        public void PrincipalGet_ReturnsWebContextUser()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            Mock<HttpContextBase> webContextMock = new Mock<HttpContextBase>(MockBehavior.Strict);
            webContextMock.Setup(r => r.User).Returns(expectedPrincipal);
            HttpContextBase webContext = webContextMock.Object;
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                IPrincipal principal = context.Principal;

                // Assert
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalSet_UpdatesWebContextUser()
        {
            // Arrange
            Mock<HttpContextBase> webContextMock = new Mock<HttpContextBase>(MockBehavior.Strict);
            IPrincipal principal = null;
            webContextMock.SetupSet(r => r.User = It.IsAny<IPrincipal>()).Callback<IPrincipal>(
                value => { principal = value; });
            HttpContextBase webContext = webContextMock.Object;
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                IPrincipal expectedPrincipal = CreateDummyPrincipal();

                // Act
                context.Principal = expectedPrincipal;

                // Assert
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalSet_UpdatesThreadCurrentPrincipal()
        {
            // Arrange
            HttpContextBase webContext = new Mock<HttpContextBase>().Object;
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                IPrincipal expectedPrincipal = CreateDummyPrincipal();

                // Act
                context.Principal = expectedPrincipal;

                // Assert
                IPrincipal principal = Thread.CurrentPrincipal;
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        public void UrlGet_ReturnsUrlHelperForRequest()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, expectedRequest);

                // Act
                UrlHelper url = context.Url;

                // Assert
                Assert.NotNull(url);
                Assert.Same(expectedRequest, url.Request);
            }
        }

        [Fact]
        public void UrlGet_ReturnsSameInstance()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                UrlHelper firstUrl = context.Url;

                // Act
                UrlHelper url = context.Url;

                // Assert
                Assert.Same(firstUrl, url);
            }
        }

        [Fact]
        public void UrlSet_UpdatesUrl()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                UrlHelper expectedUrl = CreateDummyUrlHelper();

                // Act
                context.Url = expectedUrl;

                // Assert
                UrlHelper url = context.Url;
                Assert.Same(expectedUrl, url);
            }
        }

        [Fact]
        public void UrlSet_UpdatesUrl_WhenNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                context.Url = null;

                // Assert
                UrlHelper url = context.Url;
                Assert.Null(url);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsNull_ByDefault()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Null(virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsNull_WhenConfigurationIsNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = null;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Null(virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsConfigurationVirtualPathRoot()
        {
            // Arrange
            string expectedVirtualPathRoot = "/foo";
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration(expectedVirtualPathRoot))
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal(expectedVirtualPathRoot, virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootSet_UpdatesVirtualPathRoot()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                string expectedVirtualPathRoot = "foo";

                // Act
                context.VirtualPathRoot = expectedVirtualPathRoot;

                // Assert
                string virtualPathRoot = context.VirtualPathRoot;
                Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootSet_UpdatesVirtualPathRoot_WhenNull()
        {
            // Arrange
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration("/other"))
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;

                // Act
                context.VirtualPathRoot = null;

                // Assert
                string virtualPathRoot = context.VirtualPathRoot;
                Assert.Null(virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsFirstObservedConfigurationVirtualPathRoot()
        {
            // Arrange
            string expectedVirtualPathRoot = "/foo";
            HttpContextBase webContext = CreateDummyWebContext();
            HttpRequestBase webRequest = CreateDummyWebRequest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration configuration = CreateConfiguration(expectedVirtualPathRoot))
            using (HttpConfiguration otherConfiguration = CreateConfiguration("/other"))
            {
                HttpRequestContext context = CreateProductUnderTest(webContext, webRequest, request);
                context.Configuration = configuration;
                string ignore = context.VirtualPathRoot;
                context.Configuration = otherConfiguration;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal(expectedVirtualPathRoot, virtualPathRoot);
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

        private static HttpConfiguration CreateConfiguration(string virtualPathRoot)
        {
            return new HttpConfiguration(new HttpRouteCollection(virtualPathRoot));
        }

        private static HttpContextBase CreateDummyWebContext()
        {
            return new Mock<HttpContextBase>(MockBehavior.Strict).Object;
        }

        private static HttpRequestBase CreateDummyWebRequest()
        {
            return new Mock<HttpRequestBase>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static UrlHelper CreateDummyUrlHelper()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static HttpClientCertificate CreateHttpClientCertificate(byte[] bytes)
        {
            Mock<HttpWorkerRequest> workerRequestMock = new Mock<HttpWorkerRequest>();
            workerRequestMock.Setup(wr => wr.GetRawUrl()).Returns("/");
            workerRequestMock.Setup(wr => wr.GetServerVariable("CERT_FLAGS")).Returns("1");
            workerRequestMock.Setup(wr => wr.GetClientCertificate()).Returns(bytes);
            HttpContext context = new HttpContext(workerRequestMock.Object);
            return context.Request.ClientCertificate;
        }

        private static WebHostHttpRequestContext CreateProductUnderTest(HttpContextBase contextBase,
            HttpRequestBase requestBase, HttpRequestMessage request)
        {
            return new WebHostHttpRequestContext(contextBase, requestBase, request);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestBase CreateStubWebRequest(HttpClientCertificate clientCertificate)
        {
            Mock<HttpRequestBase> mock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            mock.Setup(r => r.ClientCertificate).Returns(clientCertificate);
            return mock.Object;
        }
    }
}
