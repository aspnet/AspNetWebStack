// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class TokenValidatorTest
    {
        [Fact]
        public void GenerateCookieToken()
        {
            // Arrange
            TokenValidator tokenValidator = new TokenValidator(
                config: null,
                claimUidExtractor: null);

            // Act
            AntiForgeryToken retVal = tokenValidator.GenerateCookieToken();

            // Assert
            Assert.NotNull(retVal);
        }

        [Fact]
        public void GenerateFormToken_AnonymousUser()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(false);

            IAntiForgeryConfig config = new MockAntiForgeryConfig();

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act
            var fieldToken = validator.GenerateFormToken(httpContext, mockIdentity.Object, cookieToken);

            // Assert
            Assert.NotNull(fieldToken);
            Assert.Equal(cookieToken.SecurityToken, fieldToken.SecurityToken);
            Assert.False(fieldToken.IsSessionToken);
            Assert.Equal("", fieldToken.Username);
            Assert.Null(fieldToken.ClaimUid);
            Assert.Equal("", fieldToken.AdditionalData);
        }

        [Fact]
        public void GenerateFormToken_AuthenticatedWithoutUsernameAndNoAdditionalData_NoAdditionalData()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken()
            {
                IsSessionToken = true
            };

            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new MyAuthenticatedIdentityWithoutUsername();
            IAntiForgeryConfig config = new MockAntiForgeryConfig();
            IClaimUidExtractor claimUidExtractor = new Mock<MockableClaimUidExtractor>().Object;

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: claimUidExtractor);

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => validator.GenerateFormToken(httpContext, identity, cookieToken));
            Assert.Equal(@"The provided identity of type 'System.Web.Helpers.AntiXsrf.Test.TokenValidatorTest+MyAuthenticatedIdentityWithoutUsername' is marked IsAuthenticated = true but does not have a value for Name. By default, the anti-forgery system requires that all authenticated identities have a unique Name. If it is not possible to provide a unique Name for this identity, consider setting the static property AntiForgeryConfig.AdditionalDataProvider to an instance of a type that can provide some form of unique identifier for the current user.", ex.Message);
        }

        [Fact]
        public void GenerateFormToken_AuthenticatedWithoutUsernameAndNoAdditionalData_NoAdditionalData_SuppressHeuristics()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new MyAuthenticatedIdentityWithoutUsername();

            IAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                SuppressIdentityHeuristicChecks = true
            };
            IClaimUidExtractor claimUidExtractor = new Mock<MockableClaimUidExtractor>().Object;

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: claimUidExtractor);

            // Act
            var fieldToken = validator.GenerateFormToken(httpContext, identity, cookieToken);

            // Assert
            Assert.NotNull(fieldToken);
            Assert.Equal(cookieToken.SecurityToken, fieldToken.SecurityToken);
            Assert.False(fieldToken.IsSessionToken);
            Assert.Equal("", fieldToken.Username);
            Assert.Null(fieldToken.ClaimUid);
            Assert.Equal("", fieldToken.AdditionalData);
        }

        [Fact]
        public void GenerateFormToken_AuthenticatedWithoutUsername_WithAdditionalData()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new MyAuthenticatedIdentityWithoutUsername();

            Mock<IAntiForgeryAdditionalDataProvider> mockAdditionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            mockAdditionalDataProvider.Setup(o => o.GetAdditionalData(httpContext)).Returns("additional-data");

            IAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                AdditionalDataProvider = mockAdditionalDataProvider.Object
            };
            IClaimUidExtractor claimUidExtractor = new Mock<MockableClaimUidExtractor>().Object;

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: claimUidExtractor);

            // Act
            var fieldToken = validator.GenerateFormToken(httpContext, identity, cookieToken);

            // Assert
            Assert.NotNull(fieldToken);
            Assert.Equal(cookieToken.SecurityToken, fieldToken.SecurityToken);
            Assert.False(fieldToken.IsSessionToken);
            Assert.Equal("", fieldToken.Username);
            Assert.Null(fieldToken.ClaimUid);
            Assert.Equal("additional-data", fieldToken.AdditionalData);
        }

        [Fact]
        public void GenerateFormToken_ClaimsBasedIdentity()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity("some-identity");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                UniqueClaimTypeIdentifier = "unique-identifier"
            };

            BinaryBlob expectedClaimUid = new BinaryBlob(256);
            Mock<MockableClaimUidExtractor> mockClaimUidExtractor = new Mock<MockableClaimUidExtractor>();
            mockClaimUidExtractor.Setup(o => o.ExtractClaimUid(identity)).Returns((object)expectedClaimUid);

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: mockClaimUidExtractor.Object);

            // Act
            var fieldToken = validator.GenerateFormToken(httpContext, identity, cookieToken);

            // Assert
            Assert.NotNull(fieldToken);
            Assert.Equal(cookieToken.SecurityToken, fieldToken.SecurityToken);
            Assert.False(fieldToken.IsSessionToken);
            Assert.Equal("", fieldToken.Username);
            Assert.Equal(expectedClaimUid, fieldToken.ClaimUid);
            Assert.Equal("", fieldToken.AdditionalData);
        }

        [Fact]
        public void GenerateFormToken_RegularUserWithUsername()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken() { IsSessionToken = true };

            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(true);
            mockIdentity.Setup(o => o.Name).Returns("my-username");

            IAntiForgeryConfig config = new MockAntiForgeryConfig();
            IClaimUidExtractor claimUidExtractor = new Mock<MockableClaimUidExtractor>().Object;

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: claimUidExtractor);

            // Act
            var fieldToken = validator.GenerateFormToken(httpContext, mockIdentity.Object, cookieToken);

            // Assert
            Assert.NotNull(fieldToken);
            Assert.Equal(cookieToken.SecurityToken, fieldToken.SecurityToken);
            Assert.False(fieldToken.IsSessionToken);
            Assert.Equal("my-username", fieldToken.Username);
            Assert.Null(fieldToken.ClaimUid);
            Assert.Equal("", fieldToken.AdditionalData);
        }

        [Fact]
        public void IsCookieTokenValid_FieldToken_ReturnsFalse()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken()
            {
                IsSessionToken = false
            };

            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: null);

            // Act
            bool retVal = validator.IsCookieTokenValid(cookieToken);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCookieTokenValid_NullToken_ReturnsFalse()
        {
            // Arrange
            AntiForgeryToken cookieToken = null;
            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: null);

            // Act
            bool retVal = validator.IsCookieTokenValid(cookieToken);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCookieTokenValid_ValidToken_ReturnsTrue()
        {
            // Arrange
            AntiForgeryToken cookieToken = new AntiForgeryToken()
            {
                IsSessionToken = true
            };

            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: null);

            // Act
            bool retVal = validator.IsCookieTokenValid(cookieToken);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void ValidateTokens_SessionTokenMissing()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new Mock<IIdentity>().Object;
            AntiForgeryToken sessionToken = null;
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { IsSessionToken = false };

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "my-cookie-name"
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The required anti-forgery cookie ""my-cookie-name"" is not present.", ex.Message);
        }

        [Fact]
        public void ValidateTokens_FieldTokenMissing()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new Mock<IIdentity>().Object;
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = null;

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "my-form-field-name"
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The required anti-forgery form field ""my-form-field-name"" is not present.", ex.Message);
        }

        [Fact]
        public void ValidateTokens_FieldAndSessionTokensSwapped()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new Mock<IIdentity>().Object;
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { IsSessionToken = false };

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = "my-cookie-name",
                FormFieldName = "my-form-field-name"
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act & assert
            var ex1 = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, fieldtoken, fieldtoken));
            Assert.Equal(@"Validation of the provided anti-forgery token failed. The cookie ""my-cookie-name"" and the form field ""my-form-field-name"" were swapped.", ex1.Message);

            var ex2 = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, sessionToken));
            Assert.Equal(@"Validation of the provided anti-forgery token failed. The cookie ""my-cookie-name"" and the form field ""my-form-field-name"" were swapped.", ex2.Message);
        }

        [Fact]
        public void ValidateTokens_FieldAndSessionTokensHaveDifferentSecurityKeys()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new Mock<IIdentity>().Object;
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { IsSessionToken = false };

            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: null);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The anti-forgery cookie token and form field token do not match.", ex.Message);
        }

        [Theory]
        [InlineData("the-user", "the-other-user")]
        [InlineData("http://example.com/uri-casing", "http://example.com/URI-casing")]
        [InlineData("https://example.com/secure-uri-casing", "https://example.com/secure-URI-casing")]
        public void ValidateTokens_UsernameMismatch(string identityUsername, string embeddedUsername)
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity(identityUsername);
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, Username = embeddedUsername, IsSessionToken = false };

            Mock<MockableClaimUidExtractor> mockClaimUidExtractor = new Mock<MockableClaimUidExtractor>();
            mockClaimUidExtractor.Setup(o => o.ExtractClaimUid(identity)).Returns((object)null);

            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: mockClaimUidExtractor.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The provided anti-forgery token was meant for user """ + embeddedUsername + @""", but the current user is """ + identityUsername + @""".", ex.Message);
        }

        [Fact]
        public void ValidateTokens_ClaimUidMismatch()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity("the-user");
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, IsSessionToken = false, ClaimUid = new BinaryBlob(256) };

            Mock<MockableClaimUidExtractor> mockClaimUidExtractor = new Mock<MockableClaimUidExtractor>();
            mockClaimUidExtractor.Setup(o => o.ExtractClaimUid(identity)).Returns(new BinaryBlob(256));

            TokenValidator validator = new TokenValidator(
                config: null,
                claimUidExtractor: mockClaimUidExtractor.Object);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The provided anti-forgery token was meant for a different claims-based user than the current user.", ex.Message);
        }

        [Fact]
        public void ValidateTokens_AdditionalDataRejected()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity(String.Empty);
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, Username = String.Empty, IsSessionToken = false, AdditionalData = "some-additional-data" };

            Mock<IAntiForgeryAdditionalDataProvider> mockAdditionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            mockAdditionalDataProvider.Setup(o => o.ValidateAdditionalData(httpContext, "some-additional-data")).Returns(false);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                AdditionalDataProvider = mockAdditionalDataProvider.Object
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act & assert
            var ex = Assert.Throws<HttpAntiForgeryException>(() => validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken));
            Assert.Equal(@"The provided anti-forgery token failed a custom data check.", ex.Message);
        }

        [Fact]
        public void ValidateTokens_Success_AnonymousUser()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity(String.Empty);
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, Username = String.Empty, IsSessionToken = false, AdditionalData = "some-additional-data" };

            Mock<IAntiForgeryAdditionalDataProvider> mockAdditionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            mockAdditionalDataProvider.Setup(o => o.ValidateAdditionalData(httpContext, "some-additional-data")).Returns(true);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                AdditionalDataProvider = mockAdditionalDataProvider.Object
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: null);

            // Act
            validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken);

            // Assert
            // Nothing to assert - if we got this far, success!
        }

        [Fact]
        public void ValidateTokens_Success_AuthenticatedUserWithUsername()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity("the-user");
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, Username = "THE-USER", IsSessionToken = false, AdditionalData = "some-additional-data" };

            Mock<IAntiForgeryAdditionalDataProvider> mockAdditionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            mockAdditionalDataProvider.Setup(o => o.ValidateAdditionalData(httpContext, "some-additional-data")).Returns(true);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                AdditionalDataProvider = mockAdditionalDataProvider.Object
            };
            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: new Mock<MockableClaimUidExtractor>().Object);

            // Act
            validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken);

            // Assert
            // Nothing to assert - if we got this far, success!
        }

        [Fact]
        public void ValidateTokens_Success_ClaimsBasedUser()
        {
            // Arrange
            HttpContextBase httpContext = new Mock<HttpContextBase>().Object;
            IIdentity identity = new GenericIdentity("the-user");
            AntiForgeryToken sessionToken = new AntiForgeryToken() { IsSessionToken = true };
            AntiForgeryToken fieldtoken = new AntiForgeryToken() { SecurityToken = sessionToken.SecurityToken, IsSessionToken = false, ClaimUid = new BinaryBlob(256) };

            Mock<MockableClaimUidExtractor> mockClaimUidExtractor = new Mock<MockableClaimUidExtractor>();
            mockClaimUidExtractor.Setup(o => o.ExtractClaimUid(identity)).Returns(fieldtoken.ClaimUid);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig();

            TokenValidator validator = new TokenValidator(
                config: config,
                claimUidExtractor: mockClaimUidExtractor.Object);

            // Act
            validator.ValidateTokens(httpContext, identity, sessionToken, fieldtoken);

            // Assert
            // Nothing to assert - if we got this far, success!
        }

        private sealed class MyAuthenticatedIdentityWithoutUsername : IIdentity
        {
            public string AuthenticationType
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsAuthenticated
            {
                get { return true; }
            }

            public string Name
            {
                get { return String.Empty; }
            }
        }
    }
}
