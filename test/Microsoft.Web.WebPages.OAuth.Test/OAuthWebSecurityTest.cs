// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.WebPages.OAuth.Test
{
    public class OAuthWebSecurityTest : IDisposable
    {
        [Fact]
        public void RegisterClientThrowsOnNullValue()
        {
            Assert.ThrowsArgumentNull(() => OAuthWebSecurity.RegisterClient(null), "client");
        }

        [Fact]
        public void RegisterClientThrowsIfProviderNameIsEmpty()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns((string)null);

            // Act & Assert
            Assert.ThrowsArgument(() => OAuthWebSecurity.RegisterClient(client.Object), "client");

            client.Setup(c => c.ProviderName).Returns("");

            // Act & Assert
            Assert.ThrowsArgument(() => OAuthWebSecurity.RegisterClient(client.Object), "client");
        }

        [Fact]
        public void RegisterClientThrowsRegisterMoreThanOneClientWithTheSameName()
        {
            // Arrange
            var client1 = new Mock<IAuthenticationClient>();
            client1.Setup(c => c.ProviderName).Returns("provider");

            var client2 = new Mock<IAuthenticationClient>();
            client2.Setup(c => c.ProviderName).Returns("provider");

            OAuthWebSecurity.RegisterClient(client1.Object);

            // Act & Assert
            Assert.ThrowsArgument(() => OAuthWebSecurity.RegisterClient(client2.Object), null);
        }

        [Fact]
        public void RequestAuthenticationRedirectsToProviderWithNullReturnUrl()
        {
            var cookies = new HttpCookieCollection();

            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Response.Cookies).Returns(cookies);
            context.Setup(c => c.Request.ServerVariables).Returns(new NameValueCollection());
            context.Setup(c => c.Request.Url).Returns(new Uri("http://live.com/login.aspx"));
            context.Setup(c => c.Request.RawUrl).Returns("/login.aspx");
            context.Setup(c => c.User.Identity.IsAuthenticated).Returns(false);

            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("windowslive");
            client.Setup(c => c.RequestAuthentication(
                                    context.Object,
                                    It.Is<Uri>(u => u.AbsoluteUri.StartsWith("http://live.com/login.aspx?__provider__=windowslive", StringComparison.OrdinalIgnoreCase))))
                  .Verifiable();

            OAuthWebSecurity.RegisterClient(client.Object);

            // Act
            OAuthWebSecurity.RequestAuthenticationCore(context.Object, "windowslive", null);

            // Assert
            client.Verify();
        }

        [Fact]
        public void RequestAuthenticationRedirectsToProviderWithReturnUrl()
        {
            var cookies = new HttpCookieCollection();

            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.ServerVariables).Returns(new NameValueCollection());
            context.Setup(c => c.Request.Url).Returns(new Uri("http://live.com/login.aspx"));
            context.Setup(c => c.Request.RawUrl).Returns("/login.aspx");
            context.Setup(c => c.Response.Cookies).Returns(cookies);
            context.Setup(c => c.User.Identity.IsAuthenticated).Returns(false);

            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("yahoo");
            client.Setup(c => c.RequestAuthentication(
                                    context.Object,
                                    It.Is<Uri>(u => u.AbsoluteUri.StartsWith("http://yahoo.com/?__provider__=yahoo", StringComparison.OrdinalIgnoreCase))))
                  .Verifiable();

            OAuthWebSecurity.RegisterClient(client.Object);

            // Act
            OAuthWebSecurity.RequestAuthenticationCore(context.Object, "yahoo", "http://yahoo.com");

            // Assert
            client.Verify();
        }

        [Fact]
        public void VerifyAuthenticationFail()
        {
            // Arrange
            var queryStrings = new NameValueCollection();
            queryStrings.Add("__provider__", "twitter");

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.QueryString).Returns(queryStrings);

            var client = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            client.Setup(c => c.ProviderName).Returns("facebook");
            client.Setup(c => c.VerifyAuthentication(context.Object)).Returns(new AuthenticationResult(true, "facebook", "123",
                                                                                                "super", null));

            var anotherClient = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            anotherClient.Setup(c => c.ProviderName).Returns("twitter");
            anotherClient.Setup(c => c.VerifyAuthentication(context.Object)).Returns(AuthenticationResult.Failed);

            OAuthWebSecurity.RegisterClient(client.Object);
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthenticationCore(context.Object, "one.aspx");

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("twitter", result.Provider);
        }

        [Fact]
        public void VerifyAuthenticationFailIfNoProviderInQueryString()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.QueryString).Returns(new NameValueCollection());

            var client = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            client.Setup(c => c.ProviderName).Returns("facebook");

            var anotherClient = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            anotherClient.Setup(c => c.ProviderName).Returns("twitter");

            OAuthWebSecurity.RegisterClient(client.Object);
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthenticationCore(context.Object, "");

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Null(result.Provider);
        }

        [Fact]
        public void LoginSetAuthenticationTicketIfSuccessful()
        {
            var originalProvider = OAuthWebSecurity.OAuthDataProvider;
            try
            {
                // Arrange
                var cookies = new HttpCookieCollection();
                var context = new Mock<HttpContextBase>();
                context.Setup(c => c.Request.IsSecureConnection).Returns(true);
                context.Setup(c => c.Response.Cookies).Returns(cookies);

                var dataProvider = new Mock<IOpenAuthDataProvider>(MockBehavior.Strict);
                dataProvider.Setup(p => p.GetUserNameFromOpenAuth("twitter", "12345")).Returns("hola");
                OAuthWebSecurity.OAuthDataProvider = dataProvider.Object;

                OAuthWebSecurity.RegisterTwitterClient("sdfdsfsd", "dfdsfdsf");

                // Act
                bool successful = OAuthWebSecurity.LoginCore(context.Object, "twitter", "12345", createPersistentCookie: false);

                // Assert
                Assert.True(successful);

                Assert.Single(cookies);
                HttpCookie addedCookie = cookies[0];

                Assert.Equal(FormsAuthentication.FormsCookieName, addedCookie.Name);
                Assert.True(addedCookie.HttpOnly);
                Assert.Equal("/", addedCookie.Path);
                Assert.False(addedCookie.Secure);
                Assert.False(String.IsNullOrEmpty(addedCookie.Value));

                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(addedCookie.Value);
                Assert.NotNull(ticket);
                Assert.Equal(2, ticket.Version);
                Assert.Equal("hola", ticket.Name);
                Assert.Equal("OAuth", ticket.UserData);
                Assert.False(ticket.IsPersistent);
            }
            finally
            {
                OAuthWebSecurity.OAuthDataProvider = originalProvider;
            }
        }

        [Fact]
        public void LoginFailIfUserIsNotFound()
        {
            var originalProvider = OAuthWebSecurity.OAuthDataProvider;
            try
            {
                // Arrange
                var context = new Mock<HttpContextBase>();
                OAuthWebSecurity.RegisterTwitterClient("consumerKey", "consumerSecrte");

                var dataProvider = new Mock<IOpenAuthDataProvider>();
                dataProvider.Setup(p => p.GetUserNameFromOpenAuth("twitter", "12345")).Returns((string)null);
                OAuthWebSecurity.OAuthDataProvider = dataProvider.Object;

                // Act
                bool successful = OAuthWebSecurity.LoginCore(context.Object, "twitter", "12345", createPersistentCookie: false);

                // Assert
                Assert.False(successful);
            }
            finally
            {
                OAuthWebSecurity.OAuthDataProvider = originalProvider;
            }
        }

        [Fact]
        public void GetOAuthClientReturnsTheCorrectClient()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            var expectedClient = OAuthWebSecurity.GetOAuthClient("facebook");

            // Assert
            Assert.Same(expectedClient, client.Object);
        }

        [Fact]
        public void GetOAuthClientThrowsIfClientIsNotFound()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => OAuthWebSecurity.GetOAuthClient("live"));
        }

        [Fact]
        public void TryGetOAuthClientSucceeds()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            IAuthenticationClient expectedClient;
            bool result = OAuthWebSecurity.TryGetOAuthClient("facebook", out expectedClient);

            // Assert
            Assert.Same(expectedClient, client.Object);
            Assert.True(result);
        }

        [Fact]
        public void TryGetOAuthClientFail()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            IAuthenticationClient expectedClient;
            bool result = OAuthWebSecurity.TryGetOAuthClient("live", out expectedClient);

            // Assert
            Assert.Null(expectedClient);
            Assert.False(result);
        }

        [Fact]
        public void TestRegisterFacebookClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterFacebookClient("one", "two", displayName: "FB", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("facebook", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("FB", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        [Fact]
        public void TestRegisterMicrosoftClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterMicrosoftClient("one", "two", displayName: "MS", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("microsoft", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("MS", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        [Fact]
        public void TestRegisterTwitterClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterTwitterClient("x0", "y0", displayName: "Tweet", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("twitter", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("Tweet", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        [Fact]
        public void TestRegisterLinkedInClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterLinkedInClient("x0", "y0", displayName: "LINKED", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("linkedIn", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("LINKED", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        [Fact]
        public void TestRegisterGoogleClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterGoogleClient(displayName: "GOOG", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("google", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("GOOG", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        [Fact]
        public void TestRegisterYahooClient()
        {
            // Arrange
            OAuthWebSecurity.RegisterYahooClient(displayName: "YHOO", extraData: null);

            // Act
            var data = OAuthWebSecurity.RegisteredClientData;

            // Assert
            Assert.True(data.IsReadOnly);
            Assert.Equal(1, data.Count);
            Assert.Equal("yahoo", data.First().AuthenticationClient.ProviderName);
            Assert.Equal("YHOO", data.First().DisplayName);
            Assert.Null(data.First().ExtraData);
        }

        public void Dispose()
        {
            OAuthWebSecurity.ClearProviders();
        }
    }
}
