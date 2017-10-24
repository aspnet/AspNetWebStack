// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class OwinHttpRequestContextTests
    {
        [Fact]
        public void ContextGet_ReturnsProvidedInstance()
        {
            // Arrange
            IOwinContext expectedOwinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                OwinHttpRequestContext context = CreateProductUnderTest(expectedOwinContext, request);

                // Act
                IOwinContext owinContext = context.Context;

                // Assert
                Assert.Same(expectedOwinContext, owinContext);
            }
        }

        [Fact]
        public void RequestGet_ReturnsProvidedInstance()
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                OwinHttpRequestContext context = CreateProductUnderTest(owinContext, expectedRequest);

                // Act
                HttpRequestMessage request = context.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsContextClientCertificate()
        {
            // Arrange
            X509Certificate2 expectedCertificate = CreateCertificate();
            Mock<IOwinContext> owinContextMock = CreateOwinContextMock();
            owinContextMock
                .Setup(c => c.Get<X509Certificate2>(OwinConstants.ClientCertifiateKey))
                .Returns(expectedCertificate);
            IOwinContext owinContext = owinContextMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_ByDefault()
        {
            // Arrange
            IOwinContext owinContext = new OwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateSet_UpdatesClientCertificate()
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            Mock<IOwinContext> owinContextMock = CreateOwinContextMock();
            owinContextMock
                .Setup(c => c.Get<X509Certificate2>(OwinConstants.ClientCertifiateKey))
                .Returns(CreateCertificate());
            IOwinContext owinContext = owinContextMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                context.ClientCertificate = null;

                // Assert
                X509Certificate2 certificate = context.ClientCertificate;
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsFirstObservedContextClientCertificate()
        {
            // Arrange
            X509Certificate2 expectedCertificate = CreateCertificate();
            X509Certificate2 currentCertificate = expectedCertificate;
            Mock<IOwinContext> owinContextMock = CreateOwinContextMock();
            owinContextMock
                .Setup(c => c.Get<X509Certificate2>(OwinConstants.ClientCertifiateKey))
                .Returns(() => currentCertificate);
            IOwinContext owinContext = owinContextMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                X509Certificate2 ignore = context.ClientCertificate;
                currentCertificate = CreateCertificate();

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Same(expectedCertificate, certificate);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsFalse_ByDefault()
        {
            // Arrange
            IOwinContext owinContext = new OwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.False(includeErrorDetail);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsTrue_WhenUnconfiguredAndIsLocal()
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                context.IsLocal = true;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.True(includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true, IncludeErrorDetailPolicy.Always, true)]
        [InlineData(true, IncludeErrorDetailPolicy.Always, false)]
        [InlineData(false, IncludeErrorDetailPolicy.Never, true)]
        [InlineData(false, IncludeErrorDetailPolicy.Never, false)]
        [InlineData(true, IncludeErrorDetailPolicy.LocalOnly, true)]
        [InlineData(false, IncludeErrorDetailPolicy.LocalOnly, false)]
        [InlineData(true, IncludeErrorDetailPolicy.Default, true)]
        [InlineData(false, IncludeErrorDetailPolicy.Default, false)]
        public void IncludeErrorDetailGet_ForPolicy(bool expected, IncludeErrorDetailPolicy policy, bool isLocal)
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                context.Configuration = configuration;
                context.IsLocal = isLocal;
                configuration.IncludeErrorDetailPolicy = policy;

                // Act
                bool includeErrorDetail = context.IncludeErrorDetail;

                // Assert
                Assert.Equal(expected, includeErrorDetail);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailSet_UpdatesIncludeErrorDetail(bool expectedIncludeErrorDetail)
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                context.IsLocal = expectedIncludeErrorDetail;
                bool ignore = context.IncludeErrorDetail;
                context.IsLocal = !expectedIncludeErrorDetail;

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
            IOwinContext owinContext = new OwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.False(isLocal);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IsLocal_ReturnsContextIsLocalValue(bool expectedIsLocal)
        {
            // Arrange
            Mock<IOwinContext> owinContextMock = CreateOwinContextMock();
            owinContextMock.Setup(c => c.Get<bool>(OwinConstants.IsLocalKey)).Returns(expectedIsLocal);
            IOwinContext owinContext = owinContextMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

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
            Mock<IOwinContext> owinContextMock = CreateOwinContextMock();
            bool currentOwinContextIsLocal = expectedIsLocal;
            owinContextMock.Setup(c => c.Get<bool>(OwinConstants.IsLocalKey)).Returns(() => currentOwinContextIsLocal);
            IOwinContext owinContext = owinContextMock.Object;

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                bool ignore = context.IsLocal;
                currentOwinContextIsLocal = !expectedIsLocal;

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Fact]
        public void PrincipalGet_ReturnsContextRequestUser()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            owinRequestMock.Setup(r => r.User).Returns(expectedPrincipal);
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                IPrincipal principal = context.Principal;

                // Assert
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalSet_UpdatesContextRequestUser()
        {
            // Arrange
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            IPrincipal principal = null;
            owinRequestMock.SetupSet((r) => r.User = It.IsAny<IPrincipal>()).Callback<IPrincipal>(
                value => { principal = value; });
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            IOwinContext owinContext = CreateStubOwinContext(new Mock<IOwinRequest>().Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, expectedRequest);

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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                context.Url = null;

                // Assert
                UrlHelper url = context.Url;
                Assert.Null(url);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsSlash_ByDefault()
        {
            // Arrange
            IOwinContext owinContext = new OwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal("/", virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsContextRequestPathBase()
        {
            // Arrange
            string expectedVirtualPathRoot = "/foo";
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            owinRequestMock.Setup(r => r.PathBase).Returns(new PathString(expectedVirtualPathRoot));
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal(expectedVirtualPathRoot, virtualPathRoot);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void VirtualPathRootGet_ReturnsSlash_WhenContextRequestPathBaseIsValue(string contextRequestPathBase)
        {
            // Arrange
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            owinRequestMock.Setup(r => r.PathBase).Returns(new PathString(contextRequestPathBase));
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal("/", virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootSet_UpdatesVirtualPathRoot()
        {
            // Arrange
            IOwinContext owinContext = CreateStubOwinContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
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
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            owinRequestMock.Setup(r => r.PathBase).Returns(new PathString("/other"));
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);

                // Act
                context.VirtualPathRoot = null;

                // Assert
                string virtualPathRoot = context.VirtualPathRoot;
                Assert.Null(virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsFirstObservedContextRequestPathBase()
        {
            // Arrange
            string expectedVirtualPathRoot = "/expected";
            string currentVirtualPathRoot = expectedVirtualPathRoot;
            Mock<IOwinRequest> owinRequestMock = new Mock<IOwinRequest>(MockBehavior.Strict);
            owinRequestMock.Setup(r => r.PathBase).Returns(() => new PathString(currentVirtualPathRoot));
            IOwinContext owinContext = CreateStubOwinContext(owinRequestMock.Object);

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(owinContext, request);
                string ignore = context.VirtualPathRoot;
                currentVirtualPathRoot = "/other";

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

        private static IOwinRequest CreateDummyOwinRequest()
        {
            return new Mock<IOwinRequest>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static UrlHelper CreateDummyUrlHelper()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static Mock<IOwinContext> CreateOwinContextMock()
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(CreateDummyOwinRequest());
            return mock;
        }

        private static OwinHttpRequestContext CreateProductUnderTest(IOwinContext context, HttpRequestMessage request)
        {
            return new OwinHttpRequestContext(context, request);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static IOwinContext CreateStubOwinContext()
        {
            return CreateOwinContextMock().Object;
        }

        private static IOwinContext CreateStubOwinContext(IOwinRequest request)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.Setup(c => c.Request).Returns(request);
            return mock.Object;
        }
    }
}
