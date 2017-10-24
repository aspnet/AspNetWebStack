// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class AntiForgeryTokenStoreTest
    {
        [Fact]
        public void GetCookieToken_CookieDoesNotExist_ReturnsNull()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Cookies).Returns(new HttpCookieCollection());

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "cookie-name"
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = tokenStore.GetCookieToken(mockHttpContext.Object);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetCookieToken_CookieIsEmpty_ReturnsNull()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Cookies).Returns(new HttpCookieCollection()
            {
                new HttpCookie("cookie-name", "")
            });

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "cookie-name"
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = tokenStore.GetCookieToken(mockHttpContext.Object);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetCookieToken_CookieIsInvalid_PropagatesException()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Cookies).Returns(new HttpCookieCollection()
            {
                new HttpCookie("cookie-name", "invalid-value")
            });

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "cookie-name"
            };

            HttpAntiForgeryException expectedException = new HttpAntiForgeryException("some exception");
            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("invalid-value")).Throws(expectedException);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => tokenStore.GetCookieToken(mockHttpContext.Object));
            Assert.Equal(expectedException, ex);
        }

        [Fact]
        public void GetCookieToken_CookieIsValid_ReturnsToken()
        {
            // Arrange
            AntiForgeryToken expectedToken = new AntiForgeryToken();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Cookies).Returns(new HttpCookieCollection()
            {
                new HttpCookie("cookie-name", "valid-value")
            });

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "cookie-name"
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("valid-value")).Returns((object)expectedToken);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            AntiForgeryToken retVal = tokenStore.GetCookieToken(mockHttpContext.Object);

            // Assert
            Assert.Same(expectedToken, retVal);
        }

        [Fact]
        public void GetFormToken_FormFieldIsEmpty_ReturnsNull()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Form.Get("form-field-name")).Returns("");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = tokenStore.GetFormToken(mockHttpContext.Object);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetFormToken_FormFieldIsInvalid_PropagatesException()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Form.Get("form-field-name")).Returns("invalid-value");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            HttpAntiForgeryException expectedException = new HttpAntiForgeryException("some exception");
            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("invalid-value")).Throws(expectedException);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => tokenStore.GetFormToken(mockHttpContext.Object));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public void GetFormToken_FormFieldIsValid_ReturnsToken()
        {
            // Arrange
            AntiForgeryToken expectedToken = new AntiForgeryToken();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.Form.Get("form-field-name")).Returns("valid-value");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("valid-value")).Returns((object)expectedToken);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            AntiForgeryToken retVal = tokenStore.GetFormToken(mockHttpContext.Object);

            // Assert
            Assert.Same(expectedToken, retVal);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, null)]
        public void SaveCookieToken(bool requireSsl, bool? expectedCookieSecureFlag)
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();
            HttpCookieCollection cookies = new HttpCookieCollection();
            bool defaultCookieSecureValue = expectedCookieSecureFlag ?? new HttpCookie("name", "value").Secure; // pulled from config; set by ctor

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Response.Cookies).Returns(cookies);

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Serialize(token)).Returns("serialized-value");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "cookie-name",
                RequireSSL = requireSsl
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            tokenStore.SaveCookieToken(mockHttpContext.Object, token);

            // Assert
            Assert.Single(cookies);
            HttpCookie cookie = cookies["cookie-name"];

            Assert.NotNull(cookie);
            Assert.Equal("serialized-value", cookie.Value);
            Assert.True(cookie.HttpOnly);
            Assert.Equal(defaultCookieSecureValue, cookie.Secure);
        }
    }
}
