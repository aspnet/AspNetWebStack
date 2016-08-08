// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Claims.Test
{
    public class ClaimsIdentityTest
    {
        [Fact]
        public void TryConvert_GetClaims()
        {
            // Act
            ClaimsIdentity claimsIdentity = ClaimsIdentity.TryConvert<IClaimsIdentity, IClaim>(new MyClaimsIdentity());
            var claims = claimsIdentity.GetClaims().ToArray();

            // Assert
            Assert.Equal(2, claims.Length);
            Assert.Equal("claim-type-1", claims[0].ClaimType);
            Assert.Equal("claim-value-1", claims[0].Value);
            Assert.Equal("claim-type-2", claims[1].ClaimType);
            Assert.Equal("claim-value-2", claims[1].Value);
        }

        private interface IClaimsIdentity : IIdentity
        {
            IEnumerable<IClaim> Claims { get; }
        }

        private interface IClaim
        {
            string ClaimType { get; }
            string Value { get; }
        }

        private sealed class MyClaimsIdentity : IClaimsIdentity, IIdentity
        {
            IEnumerable<IClaim> IClaimsIdentity.Claims
            {
                get
                {
                    return new MyClaim[]
                    {
                        new MyClaim() { ClaimType = "claim-type-1", Value = "claim-value-1" },
                        new MyClaim() { ClaimType = "claim-type-2", Value = "claim-value-2" }
                    };
                }
            }

            string IIdentity.AuthenticationType
            {
                get { throw new NotImplementedException(); }
            }

            bool IIdentity.IsAuthenticated
            {
                get { throw new NotImplementedException(); }
            }

            string IIdentity.Name
            {
                get { throw new NotImplementedException(); }
            }

            private sealed class MyClaim : IClaim
            {
                public string ClaimType { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
