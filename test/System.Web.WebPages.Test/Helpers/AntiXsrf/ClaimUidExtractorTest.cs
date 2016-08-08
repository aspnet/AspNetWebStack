// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Web.Helpers.Claims;
using System.Web.Helpers.Claims.Test;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class ClaimUidExtractorTest
    {
        [Fact]
        public void ExtractClaimUid_NullIdentity()
        {
            // Arrange
            ClaimUidExtractor extractor = new ClaimUidExtractor(
                config: null,
                claimsIdentityConverter: null);

            // Act
            BinaryBlob retVal = extractor.ExtractClaimUid(null);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_Unauthenticated()
        {
            // Arrange
            ClaimUidExtractor extractor = new ClaimUidExtractor(
                config: null,
                claimsIdentityConverter: null);

            Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(false);

            // Act
            BinaryBlob retVal = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_ClaimsIdentityHeuristicsSuppressed()
        {
            // Arrange
            GenericIdentity identity = new GenericIdentity("the-user");
            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                SuppressIdentityHeuristicChecks = true
            };

            ClaimUidExtractor extractor = new ClaimUidExtractor(
                config: config,
                claimsIdentityConverter: null);

            // Act
            BinaryBlob retVal = extractor.ExtractClaimUid(identity);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_NotAClaimsIdentity()
        {
            // Arrange
            Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(true);
            MockAntiForgeryConfig config = new MockAntiForgeryConfig();
            ClaimsIdentityConverter converter = new ClaimsIdentityConverter(new Func<IIdentity, ClaimsIdentity>[0]);

            ClaimUidExtractor extractor = new ClaimUidExtractor(
                config: config,
                claimsIdentityConverter: converter);

            // Act
            BinaryBlob retVal = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void ExtractClaimUid_ClaimsIdentity()
        {
            // Arrange
            Mock<IIdentity> mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(true);
            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                UniqueClaimTypeIdentifier = "unique-identifier"
            };
            ClaimsIdentityConverter converter = new ClaimsIdentityConverter(new Func<IIdentity, ClaimsIdentity>[] {
               identity =>
               {
                   Assert.Equal(mockIdentity.Object, identity);
                   MockClaimsIdentity claimsIdentity = new MockClaimsIdentity();
                   claimsIdentity.AddClaim("unique-identifier", "some-value");
                   return claimsIdentity;
               }
            });

            ClaimUidExtractor extractor = new ClaimUidExtractor(
                config: config,
                claimsIdentityConverter: converter);

            // Act
            BinaryBlob retVal = extractor.ExtractClaimUid(mockIdentity.Object);

            // Assert
            Assert.NotNull(retVal);
            Assert.Equal("CA9CCFF86F903FBB7505BAAA9F222E49EC2A1E8FAD630AE73DE180BD679751ED", HexUtil.HexEncode(retVal.GetData()));
        }

        [Theory]
        [DefaultUniqueClaimTypes_NotPresent_Data]
        public void DefaultUniqueClaimTypes_NotPresent_Throws(object identity)
        {
            // Arrange
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)identity;

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => ClaimUidExtractor.GetUniqueIdentifierParameters(claimsIdentity, null));
            Assert.Equal(@"A claim of type 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier' or 'http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider' was not present on the provided ClaimsIdentity. To enable anti-forgery token support with claims-based authentication, please verify that the configured claims provider is providing both of these claims on the ClaimsIdentity instances it generates. If the configured claims provider instead uses a different claim type as a unique identifier, it can be configured by setting the static property AntiForgeryConfig.UniqueClaimTypeIdentifier.", ex.Message);
        }

        [Fact]
        public void DefaultUniqueClaimTypes_Present()
        {
            // Arrange
            MockClaimsIdentity identity = new MockClaimsIdentity();
            identity.AddClaim("fooClaim", "fooClaimValue");
            identity.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, "nameIdentifierValue");
            identity.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, "identityProviderValue");

            // Act
            var retVal = ClaimUidExtractor.GetUniqueIdentifierParameters(identity, null);

            // Assert
            Assert.Equal(new string[] {
                ClaimUidExtractor.NameIdentifierClaimType,
                "nameIdentifierValue",
                ClaimUidExtractor.IdentityProviderClaimType,
                "identityProviderValue"
            }, retVal);
        }

        [Fact]
        public void ExplicitUniqueClaimType_Present()
        {
            // Arrange
            MockClaimsIdentity identity = new MockClaimsIdentity();
            identity.AddClaim("fooClaim", "fooClaimValue");
            identity.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, "nameIdentifierValue");
            identity.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, "identityProviderValue");

            // Act
            var retVal = ClaimUidExtractor.GetUniqueIdentifierParameters(identity, "fooClaim");

            // Assert
            Assert.Equal(new string[] {
                "fooClaim",
                "fooClaimValue"
            }, retVal);
        }

        [Theory]
        [ExplicitUniqueClaimType_NotPresent_Data]
        public void ExplicitUniqueClaimType_NotPresent_Throws(object identity)
        {
            // Arrange
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)identity;

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => ClaimUidExtractor.GetUniqueIdentifierParameters(claimsIdentity, "fooClaim"));
            Assert.Equal(@"A claim of type 'fooClaim' was not present on the provided ClaimsIdentity.", ex.Message);
        }

        private sealed class DefaultUniqueClaimTypes_NotPresent_DataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                MockClaimsIdentity identity1 = new MockClaimsIdentity();
                identity1.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, "identityProviderValue");
                yield return new object[] { identity1 };

                MockClaimsIdentity identity2 = new MockClaimsIdentity();
                identity2.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, String.Empty);
                identity2.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, "identityProviderValue");
                yield return new object[] { identity2 };

                MockClaimsIdentity identity3 = new MockClaimsIdentity();
                identity3.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, "nameIdentifierValue");
                yield return new object[] { identity3 };

                MockClaimsIdentity identity4 = new MockClaimsIdentity();
                identity4.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, "nameIdentifierValue");
                identity4.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, String.Empty);
                yield return new object[] { identity4 };

                MockClaimsIdentity identity5 = new MockClaimsIdentity();
                identity5.AddClaim(ClaimUidExtractor.NameIdentifierClaimType.ToUpper(), "nameIdentifierValue");
                identity5.AddClaim(ClaimUidExtractor.IdentityProviderClaimType.ToUpper(), "identityProviderValue");
                yield return new object[] { identity5 };
            }
        }

        private sealed class ExplicitUniqueClaimType_NotPresent_DataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                MockClaimsIdentity identity1 = new MockClaimsIdentity();
                yield return new object[] { identity1 };

                MockClaimsIdentity identity2 = new MockClaimsIdentity();
                identity2.AddClaim("fooClaim", String.Empty);
                yield return new object[] { identity2 };

                MockClaimsIdentity identity3 = new MockClaimsIdentity();
                identity3.AddClaim("FOOCLAIM", "fooClaimValue");
                yield return new object[] { identity3 };

                MockClaimsIdentity identity4 = new MockClaimsIdentity();
                identity4.AddClaim(ClaimUidExtractor.NameIdentifierClaimType, "nameIdentifierValue");
                identity4.AddClaim(ClaimUidExtractor.IdentityProviderClaimType, "identityProviderValue");
                yield return new object[] { identity4 };
            }
        }
    }
}
