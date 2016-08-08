// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class AntiForgeryWorkerTest
    {
        [Fact]
        public void ChecksSSL()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request.IsSecureConnection).Returns(false);

            IAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                RequireSSL = true
            };

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: null,
                tokenStore: null,
                validator: null);

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => worker.Validate(mockHttpContext.Object, "session-token", "field-token"));
            Assert.Equal(@"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => worker.Validate(mockHttpContext.Object));
            Assert.Equal(@"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => worker.GetFormInputElement(mockHttpContext.Object));
            Assert.Equal(@"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => { string dummy1, dummy2; worker.GetTokens(mockHttpContext.Object, "cookie-token", out dummy1, out dummy2); });
            Assert.Equal(@"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.", ex.Message);
        }

        [Fact]
        public void GetFormInputElement_ExistingInvalidCookieToken()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.Headers).Returns(new NameValueCollection());
            mockHttpContext.Setup(o => o.Response).Returns(mockResponse.Object);

            AntiForgeryToken oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Returns(oldCookieToken);
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken)).Verifiable();

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken)).Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken)).Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken()).Returns(newCookieToken);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);

            // Act
            TagBuilder retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />", retVal.ToString(TagRenderMode.SelfClosing));
            mockTokenStore.Verify();
        }

        [Fact]
        public void GetFormInputElement_ExistingInvalidCookieToken_SwallowsExceptions()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.Headers).Returns(new NameValueCollection());
            mockHttpContext.Setup(o => o.Response).Returns(mockResponse.Object);

            AntiForgeryToken oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Throws(new Exception("should be swallowed"));
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken)).Verifiable();

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(null)).Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken)).Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken()).Returns(newCookieToken);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);

            // Act
            TagBuilder retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />", retVal.ToString(TagRenderMode.SelfClosing));
            mockTokenStore.Verify();
        }

        [Fact]
        public void GetFormInputElement_ExistingValidCookieToken()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.Headers).Returns(new NameValueCollection());
            mockHttpContext.Setup(o => o.Response).Returns(mockResponse.Object);
            
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Returns(cookieToken);

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, cookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(cookieToken)).Returns(true);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);

            // Act
            TagBuilder retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />", retVal.ToString(TagRenderMode.SelfClosing));
        }

        [Theory]
        [InlineData(false, "SAMEORIGIN")]
        [InlineData(true, null)]
        public void GetFormInputElement_AddsXFrameOptionsHeader(bool suppressXFrameOptions, string expectedHeaderValue)
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            NameValueCollection headers = new NameValueCollection();
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.Headers).Returns(headers);
            mockResponse.Setup(r => r.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((k, v) =>
            {
                headers.Add(k, v);
            });
            mockHttpContext.Setup(o => o.Response).Returns(mockResponse.Object);

            AntiForgeryToken oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name",
                SuppressXFrameOptionsHeader = suppressXFrameOptions
            };

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Returns(oldCookieToken);
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken)).Verifiable();

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken)).Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken)).Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken()).Returns(newCookieToken);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);
            HttpContextBase context = mockHttpContext.Object;

            // Act
            TagBuilder retVal = worker.GetFormInputElement(context);

            // Assert
            string xFrameOptions = context.Response.Headers["X-FRAME-OPTIONS"];
            Assert.Equal(expectedHeaderValue, xFrameOptions);
        }

        [Fact]
        public void GetTokens_ExistingInvalidCookieToken()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token")).Returns(oldCookieToken);
            mockSerializer.Setup(o => o.Serialize(newCookieToken)).Returns("serialized-new-cookie-token");
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken)).Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken)).Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken()).Returns(newCookieToken);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                validator: mockValidator.Object);

            // Act
            string serializedNewCookieToken, serializedFormToken;
            worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token", out serializedNewCookieToken, out serializedFormToken);

            // Assert
            Assert.Equal("serialized-new-cookie-token", serializedNewCookieToken);
            Assert.Equal("serialized-form-token", serializedFormToken);
        }

        [Fact]
        public void GetTokens_ExistingInvalidCookieToken_SwallowsExceptions()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token")).Throws(new Exception("should be swallowed"));
            mockSerializer.Setup(o => o.Serialize(newCookieToken)).Returns("serialized-new-cookie-token");
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(null)).Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken)).Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken()).Returns(newCookieToken);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                validator: mockValidator.Object);

            // Act
            string serializedNewCookieToken, serializedFormToken;
            worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token", out serializedNewCookieToken, out serializedFormToken);

            // Assert
            Assert.Equal("serialized-new-cookie-token", serializedNewCookieToken);
            Assert.Equal("serialized-form-token", serializedFormToken);
        }

        [Fact]
        public void GetTokens_ExistingValidCookieToken()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token")).Returns(cookieToken);
            mockSerializer.Setup(o => o.Serialize(formToken)).Returns("serialized-form-token");

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, cookieToken)).Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(cookieToken)).Returns(true);

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                validator: mockValidator.Object);

            // Act
            string serializedNewCookieToken, serializedFormToken;
            worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token", out serializedNewCookieToken, out serializedFormToken);

            // Assert
            Assert.Null(serializedNewCookieToken);
            Assert.Equal("serialized-form-token", serializedFormToken);
        }

        [Fact]
        public void Validate_FromStrings_Failure()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken cookieToken = new AntiForgeryToken();
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("cookie-token")).Returns(cookieToken);
            mockSerializer.Setup(o => o.Deserialize("form-token")).Returns(formToken);

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken)).Throws(new HttpAntiForgeryException("my-message"));

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                validator: mockValidator.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => worker.Validate(mockHttpContext.Object, "cookie-token", "form-token"));
            Assert.Equal("my-message", ex.Message);
        }

        [Fact]
        public void Validate_FromStrings_Success()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken cookieToken = new AntiForgeryToken();
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableAntiForgeryTokenSerializer> mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("cookie-token")).Returns(cookieToken);
            mockSerializer.Setup(o => o.Deserialize("form-token")).Returns(formToken);

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken)).Verifiable();

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                validator: mockValidator.Object);

            // Act
            worker.Validate(mockHttpContext.Object, "cookie-token", "form-token");

            // Assert
            mockValidator.Verify();
        }

        [Fact]
        public void Validate_FromStore_Failure()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken cookieToken = new AntiForgeryToken();
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>();
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Returns(cookieToken);
            mockTokenStore.Setup(o => o.GetFormToken(mockHttpContext.Object)).Returns(formToken);

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken)).Throws(new HttpAntiForgeryException("my-message"));

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: null,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => worker.Validate(mockHttpContext.Object));
            Assert.Equal("my-message", ex.Message);
        }

        [Fact]
        public void Validate_FromStore_Success()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("some-user");
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.User).Returns(new GenericPrincipal(identity, new string[0]));

            AntiForgeryToken cookieToken = new AntiForgeryToken();
            AntiForgeryToken formToken = new AntiForgeryToken();

            Mock<MockableTokenStore> mockTokenStore = new Mock<MockableTokenStore>();
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object)).Returns(cookieToken);
            mockTokenStore.Setup(o => o.GetFormToken(mockHttpContext.Object)).Returns(formToken);

            Mock<MockableTokenValidator> mockValidator = new Mock<MockableTokenValidator>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken)).Verifiable();

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: null,
                tokenStore: mockTokenStore.Object,
                validator: mockValidator.Object);

            // Act
            worker.Validate(mockHttpContext.Object);

            // Assert
            mockValidator.Verify();
        }
    }
}
