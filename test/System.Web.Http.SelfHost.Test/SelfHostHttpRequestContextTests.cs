// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.SelfHost
{
    public class SelfHostHttpRequestContextTests
    {
        [Fact]
        public void RequestContextGet_ReturnsProvidedInstance()
        {
            // Arrange
            using (RequestContext expectedServiceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                SelfHostHttpRequestContext context = CreateProductUnderTest(expectedServiceModelContext, configuration,
                    request);

                // Act
                RequestContext serviceModelContext = context.RequestContext;

                // Assert
                Assert.Same(expectedServiceModelContext, serviceModelContext);
            }
        }

        [Fact]
        public void RequestGet_ReturnsProvidedInstance()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                SelfHostHttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration,
                    expectedRequest);

                // Act
                HttpRequestMessage request = context.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsNull_ByDefault()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ClientCertificateGet_ReturnsContextClientCertificate()
        {
            // Arrange
            X509Certificate2 expectedCertificate = CreateCertificate();

            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

                request.Properties[HttpSelfHostServer.SecurityKey] = CreateSecurityProperty(expectedCertificate);

                // Act
                X509Certificate2 certificate = context.ClientCertificate;

                // Assert
                // WCF clones the certificate instance, so NotNull is the best we can check without creating a real
                // certificate.
                Assert.NotNull(certificate);
            }
        }

        [Fact]
        public void ClientCertificateSet_UpdatesClientCertificate()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

                request.Properties[HttpSelfHostServer.SecurityKey] = CreateSecurityProperty(CreateCertificate());

                // Act
                context.ClientCertificate = null;

                // Assert
                X509Certificate2 certificate = context.ClientCertificate;
                Assert.Null(certificate);
            }
        }

        [Fact]
        public void ConfigurationGet_ReturnsProvidedInstance()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, expectedConfiguration,
                    request);

                // Act
                HttpConfiguration configuration = context.Configuration;

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void ConfigurationSet_UpdatesConfiguration()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration initialConfiguration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = CreateConfiguration())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, initialConfiguration,
                    request);

                // Act
                context.Configuration = expectedConfiguration;

                // Assert
                HttpConfiguration configuration = context.Configuration;
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void IncludeErrorDetailGet_ReturnsFalse_ByDefault()
        {
            // Arrange
            using (Message message = CreateMessage())
            using (RequestContext serviceModelContext = CreateStubServiceModelContext(message))
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
                context.Configuration = null;

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
                context.Configuration = null;
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (Message message = CreateMessage())
            using (RequestContext serviceModelContext = CreateStubServiceModelContext(message))
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (Message message = CreateMessage(expectedIsLocal))
            using (RequestContext serviceModelContext = CreateStubServiceModelContext(message))
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

                // Act
                context.IsLocal = expectedIsLocal;

                // Assert
                bool isLocal = context.IsLocal;
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IsLocalGet_ReturnsFirstObservedValue(bool expectedIsLocal)
        {
            // Arrange
            using (Message message = CreateMessage(expectedIsLocal))
            using (RequestContext serviceModelContext = CreateStubServiceModelContext(message))
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
                bool ignore = context.IsLocal;
                SetIsLocal(message, !expectedIsLocal);

                // Act
                bool isLocal = context.IsLocal;

                // Assert
                Assert.Equal(expectedIsLocal, isLocal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalGet_ReturnsThreadCurrentPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();

            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
                Thread.CurrentPrincipal = expectedPrincipal;

                // Act
                IPrincipal principal = context.Principal;

                // Assert
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        [RestoreThreadPrincipal]
        public void PrincipalSet_UpdatesThreadCurrentPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();

            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, expectedRequest);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal("/", virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsSlash_WhenConfigurationIsNull()
        {
            // Arrange
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
                context.Configuration = null;

                // Act
                string virtualPathRoot = context.VirtualPathRoot;

                // Assert
                Assert.Equal("/", virtualPathRoot);
            }
        }

        [Fact]
        public void VirtualPathRootGet_ReturnsConfigurationVirtualPathRoot()
        {
            // Arrange
            string expectedVirtualPathRoot = "/foo";

            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration(expectedVirtualPathRoot))
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = CreateRequest())
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);

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
            string expectedVirtualPathRoot = "/expected";
            using (RequestContext serviceModelContext = CreateStubServiceModelContext())
            using (HttpConfiguration configuration = CreateConfiguration(expectedVirtualPathRoot))
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration otherConfiguration = CreateConfiguration("/other"))
            {
                HttpRequestContext context = CreateProductUnderTest(serviceModelContext, configuration, request);
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

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static UrlHelper CreateDummyUrlHelper()
        {
            return new Mock<UrlHelper>(MockBehavior.Strict).Object;
        }

        private static Message CreateMessage()
        {
            return Message.CreateMessage(MessageVersion.None, null);
        }

        private static Message CreateMessage(bool isLocal)
        {
            Message message = CreateMessage();
            SetIsLocal(message, isLocal);
            return message;
        }

        private static SelfHostHttpRequestContext CreateProductUnderTest(RequestContext requestContext,
            HttpConfiguration configuration, HttpRequestMessage request)
        {
            return new SelfHostHttpRequestContext(requestContext, configuration, request);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static SecurityMessageProperty CreateSecurityProperty(X509Certificate2 certificate)
        {
            AuthorizationContext authorizationContext = new X509AuthorizationContext(certificate);
            ServiceSecurityContext securityContext = new ServiceSecurityContext(authorizationContext);
            SecurityMessageProperty securityProperty = new SecurityMessageProperty();
            securityProperty.ServiceSecurityContext = securityContext;
            return securityProperty;
        }

        private static Mock<RequestContext> CreateServiceModelContextMock()
        {
            Mock<RequestContext> mock = new Mock<RequestContext>(MockBehavior.Strict);
            mock.As<IDisposable>().Setup(c => c.Dispose());
            return mock;
        }

        private static RequestContext CreateStubServiceModelContext()
        {
            return CreateServiceModelContextMock().Object;
        }

        private static RequestContext CreateStubServiceModelContext(Message requestMessage)
        {
            Mock<RequestContext> mock = CreateServiceModelContextMock();
            mock.Setup(c => c.RequestMessage).Returns(requestMessage);
            return mock.Object;
        }

        private static void SetIsLocal(Message message, bool value)
        {
            IPAddress address = value ? IPAddress.Loopback : IPAddress.None;
            message.Properties[RemoteEndpointMessageProperty.Name] = new RemoteEndpointMessageProperty(
                address.ToString(), 0);
        }

        private class X509AuthorizationContext : AuthorizationContext
        {
            private readonly ReadOnlyCollection<ClaimSet> _claimSets;

            public X509AuthorizationContext(X509Certificate2 certificate)
            {
                Contract.Assert(certificate != null);
                _claimSets = new ReadOnlyCollection<ClaimSet>(new List<ClaimSet>(new ClaimSet[] {
                    new X509CertificateClaimSet(certificate) }));
            }

            public override ReadOnlyCollection<ClaimSet> ClaimSets
            {
                get { return _claimSets; }
            }

            public override DateTime ExpirationTime
            {
                get { throw new NotImplementedException(); }
            }

            public override string Id
            {
                get { throw new NotImplementedException(); }
            }

            public override IDictionary<string, object> Properties
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
