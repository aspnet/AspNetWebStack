// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Batch
{
    public class BatchHttpRequestContextTests
    {
        [Fact]
        public void Constructor_Throws_WhenBatchContextIsNull()
        {
            // Arrange
            HttpRequestContext batchContext = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(batchContext), "batchContext");
        }

        [Fact]
        public void BatchContext_ReturnsProvidedInstance()
        {
            // Arrange
            HttpRequestContext expectedBatchContext = CreateDummyContext();
            BatchHttpRequestContext context = CreateProductUnderTest(expectedBatchContext);

            // Act
            HttpRequestContext batchContext = context.BatchContext;

            // Assert
            Assert.Same(expectedBatchContext, batchContext);
        }

        [Fact]
        public void ClientCertificateGet_ReturnsBatchContextClientCertificate()
        {
            // Arrange
            X509Certificate2 expectedCertificate = CreateCertificate();
            HttpRequestContext batchContext = CreateContext();
            batchContext.ClientCertificate = expectedCertificate;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            X509Certificate2 certificate = context.ClientCertificate;

            // Assert
            Assert.Same(expectedCertificate, certificate);
        }

        [Fact]
        public void ClientCertificateSet_UpdatesBatchContextClientCertificate()
        {
            // Arrange
            X509Certificate2 expectedCertificate = CreateCertificate();
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.ClientCertificate = expectedCertificate;

            // Assert
            X509Certificate2 certificate = batchContext.ClientCertificate;
            Assert.Same(expectedCertificate, certificate);
        }

        [Fact]
        public void ConfigurationGet_DoesNotReturnBatchContextConfiguration()
        {
            // Arrange
            using (HttpConfiguration unexpectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext batchContext = CreateContext();
                batchContext.Configuration = unexpectedConfiguration;
                HttpRequestContext context = CreateProductUnderTest(batchContext);

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.NotSame(unexpectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationSet_UpdatesConfiguration()
        {
            // Arrange
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext batchContext = CreateContext();
                HttpRequestContext context = CreateProductUnderTest(batchContext);

                // Act
                context.Configuration = expectedConfiguration;

                // Assert
                HttpConfiguration configuration = context.Configuration;
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailGet_ReturnsBatchContextIncludeErrorDetail(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpRequestContext batchContext = CreateContext();
            batchContext.IncludeErrorDetail = expectedIncludeErrorDetail;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            bool includeErrorDetail = context.IncludeErrorDetail;

            // Assert
            Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IncludeErrorDetailSet_UpdatesBatchContextIncludeErrorDetail(bool expectedIncludeErrorDetail)
        {
            // Arrange
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.IncludeErrorDetail = expectedIncludeErrorDetail;

            // Assert
            bool includeErrorDetail = batchContext.IncludeErrorDetail;
            Assert.Equal(expectedIncludeErrorDetail, includeErrorDetail);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalGet_ReturnsBatchContextIsLocal(bool expectedIsLocal)
        {
            // Arrange
            HttpRequestContext batchContext = CreateContext();
            batchContext.IsLocal = expectedIsLocal;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            bool isLocal = context.IsLocal;

            // Assert
            Assert.Equal(expectedIsLocal, isLocal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsLocalSet_UpdatesBatchContextIsLocal(bool expectedIsLocal)
        {
            // Arrange
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.IsLocal = expectedIsLocal;

            // Assert
            bool isLocal = batchContext.IsLocal;
            Assert.Equal(expectedIsLocal, isLocal);
        }

        [Fact]
        public void PrincipalGet_ReturnsBatchContextPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpRequestContext batchContext = CreateContext();
            batchContext.Principal = expectedPrincipal;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            IPrincipal principal = context.Principal;

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void PrincipalSet_UpdatesBatchContextPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.Principal = expectedPrincipal;

            // Assert
            IPrincipal principal = batchContext.Principal;
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void RouteDataGet_DoesNotReturnBatchContextRouteData()
        {
            // Arrange
            IHttpRouteData unexpectedRouteData = CreateDummyRouteData();
            HttpRequestContext batchContext = CreateContext();
            batchContext.RouteData = unexpectedRouteData;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            IHttpRouteData routeData = context.RouteData;

            // Assert
            Assert.NotSame(unexpectedRouteData, routeData);
        }

        [Fact]
        public void RouteDataSet_UpdatesRouteData()
        {
            // Arrange
            IHttpRouteData expectedRouteData = CreateDummyRouteData();
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.RouteData = expectedRouteData;

            // Assert
            IHttpRouteData routeData = context.RouteData;
            Assert.Same(expectedRouteData, routeData);
        }

        [Fact]
        public void UrlGet_DoesNotReturnBatchContextUrl()
        {
            // Arrange
            UrlHelper unexpectedUrl = CreateDummyUrlHelper();
            HttpRequestContext batchContext = CreateContext();
            batchContext.Url = unexpectedUrl;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            UrlHelper url = context.Url;

            // Assert
            Assert.NotSame(unexpectedUrl, url);
        }

        [Fact]
        public void UrlSet_UpdatesUrl()
        {
            // Arrange
            UrlHelper expectedUrl = CreateDummyUrlHelper();
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.Url = expectedUrl;

            // Assert
            UrlHelper url = context.Url;
            Assert.Same(expectedUrl, url);
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsBatchContextVirtualPathRoot()
        {
            // Arrange
            string expectedVirtualPathRoot = "foo";
            HttpRequestContext batchContext = CreateContext();
            batchContext.VirtualPathRoot = expectedVirtualPathRoot;
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            string virtualPathRoot = context.VirtualPathRoot;

            // Assert
            Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
        }

        [Fact]
        public void VirtualPathRootSet_UpdatesBatchContextVirtualPathRoot()
        {
            // Arrange
            string expectedVirtualPathRoot = "foo";
            HttpRequestContext batchContext = CreateContext();
            HttpRequestContext context = CreateProductUnderTest(batchContext);

            // Act
            context.VirtualPathRoot = expectedVirtualPathRoot;

            // Assert
            string virtualPathRoot = batchContext.VirtualPathRoot;
            Assert.Same(expectedVirtualPathRoot, virtualPathRoot);
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

        private static HttpRequestContext CreateDummyContext()
        {
            return new Mock<HttpRequestContext>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static IHttpRouteData CreateDummyRouteData()
        {
            return new Mock<IHttpRouteData>(MockBehavior.Strict).Object;
        }

        private static UrlHelper CreateDummyUrlHelper()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static BatchHttpRequestContext CreateProductUnderTest(HttpRequestContext batchContext)
        {
            return new BatchHttpRequestContext(batchContext);
        }
    }
}
