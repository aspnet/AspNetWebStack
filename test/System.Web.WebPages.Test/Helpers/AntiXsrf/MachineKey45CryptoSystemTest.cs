// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class MachineKey45CryptoSystemTest
    {
        [Fact]
        public void Protect()
        {
            // Arrange
            byte[] unprotectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            string unprotectedString = HttpServerUtility.UrlTokenEncode(unprotectedBytes);

            MachineKey45CryptoSystem cryptoSystem = new MachineKey45CryptoSystem();

            // Act
            string protectedString = cryptoSystem.Protect(unprotectedBytes);

            // Assert
            Assert.NotEqual(unprotectedString, protectedString);
        }

        [Fact]
        public void Unprotect()
        {
            // Arrange
            byte[] unprotectedBytes = new byte[] { 1, 2, 3, 4, 5 };

            MachineKey45CryptoSystem cryptoSystem = new MachineKey45CryptoSystem();

            // Act
            string protectedString = cryptoSystem.Protect(unprotectedBytes);
            byte[] output = cryptoSystem.Unprotect(protectedString);

            // Assert
            Assert.Equal(unprotectedBytes, output);
        }
    }
}
