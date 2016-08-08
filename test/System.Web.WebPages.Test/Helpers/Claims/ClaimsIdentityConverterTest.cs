// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Web.Security;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Claims.Test
{
    public class ClaimsIdentityConverterTest
    {
        [Fact]
        public void TryConvert_NoMatches_ReturnsNull()
        {
            // Arrange
            IIdentity identity = new Mock<IIdentity>().Object;
            ClaimsIdentityConverter converter = new ClaimsIdentityConverter(new Func<IIdentity, ClaimsIdentity>[0]);

            // Act
            ClaimsIdentity retVal = converter.TryConvert(identity);

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void TryConvert_ReturnsFirstMatch()
        {
            // Arrange
            IIdentity identity = new Mock<IIdentity>().Object;
            ClaimsIdentity claimsIdentity = new MockClaimsIdentity();

            ClaimsIdentityConverter converter = new ClaimsIdentityConverter(new Func<IIdentity, ClaimsIdentity>[]
            {
                _ => null,
                i => (i == identity) ? claimsIdentity : null
            });

            // Act
            ClaimsIdentity retVal = converter.TryConvert(identity);

            // Assert
            Assert.Same(claimsIdentity, retVal);
        }

        [Theory]
        [GrandfatheredTypesData]
        public void TryConvert_SkipsGrandfatheredTypes(IIdentity identity)
        {
            // Arrange
            ClaimsIdentityConverter converter = new ClaimsIdentityConverter(new Func<IIdentity, ClaimsIdentity>[]
            {
                _ => { throw new Exception("Should never be called."); }
            });

            // Act
            ClaimsIdentity retVal = converter.TryConvert(identity);

            // Assert
            Assert.Null(retVal);
        }

        private sealed class GrandfatheredTypesDataAttribute : DataAttribute
        {
            // We need to subclass these types so that they implement the
            // appropriate interface to be claims-based.
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                yield return new object[] { new SubclassedFormsIdentity() };
                yield return new object[] { new SubclassedGenericIdentity() };

                SubclassedWindowsIdentity subclassedWindowsIdentity = null;
                using (WindowsIdentity originalIdentity = WindowsIdentity.GetCurrent())
                {
                    subclassedWindowsIdentity = new SubclassedWindowsIdentity(originalIdentity.Token);
                }
                yield return new object[] { subclassedWindowsIdentity };
            }
        }

        private sealed class SubclassedFormsIdentity : FormsIdentity
        {
            public SubclassedFormsIdentity() : base(new FormsAuthenticationTicket("my-name", false, 60)) { }
        }

        private sealed class SubclassedGenericIdentity : GenericIdentity
        {
            public SubclassedGenericIdentity() : base("my-name") { }
        }

        private sealed class SubclassedWindowsIdentity : WindowsIdentity
        {
            public SubclassedWindowsIdentity(IntPtr userToken) : base(userToken) { }
        }
    }
}
