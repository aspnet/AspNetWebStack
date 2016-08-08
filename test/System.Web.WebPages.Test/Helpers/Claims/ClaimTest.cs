// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.Claims.Test
{
    public class ClaimTest
    {
        [Fact]
        public void CtorAndProperties()
        {
            // Act
            Claim claim = new Claim("claim-type", "claim-value");

            // Assert
            Assert.Equal("claim-type", claim.ClaimType);
            Assert.Equal("claim-value", claim.Value);
        }

        [Fact]
        public void Create_WithClaimTypeProperty()
        {
            // Act
            Claim claim = Claim.Create<IClaimType1>(new MyClaimType());

            // Assert
            Assert.Equal("my-claim-type-1", claim.ClaimType);
            Assert.Equal("my-claim-value-1", claim.Value);
        }

        [Fact]
        public void Create_WithTypeProperty()
        {
            // Act
            Claim claim = Claim.Create<IClaimType2>(new MyClaimType());

            // Assert
            Assert.Equal("my-claim-type-2", claim.ClaimType);
            Assert.Equal("my-claim-value-2", claim.Value);
        }

        private interface IClaimType1
        {
            string ClaimType { get; }
            string Value { get; }
        }

        private interface IClaimType2
        {
            string Type { get; }
            string Value { get; }
        }

        private sealed class MyClaimType : IClaimType1, IClaimType2
        {
            string IClaimType1.ClaimType { get { return "my-claim-type-1"; } }
            string IClaimType1.Value { get { return "my-claim-value-1"; } }

            string IClaimType2.Type { get { return "my-claim-type-2"; } }
            string IClaimType2.Value { get { return "my-claim-value-2"; } }
        }
    }
}
