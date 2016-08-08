// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class OwinHttpConfigurationExtensionsTest
    {
        [Fact]
        public void SuppressDefaultHostAuthentication_InsertsPassiveAuthenticationMessageHandler()
        {
            // Arrange
            DelegatingHandler existingHandler = new Mock<DelegatingHandler>(MockBehavior.Strict).Object;

            using (HttpConfiguration configuration = new HttpConfiguration())
            {
                configuration.MessageHandlers.Add(existingHandler);

                // Act
                configuration.SuppressDefaultHostAuthentication();

                // Assert
                Assert.Equal(2, configuration.MessageHandlers.Count);
                DelegatingHandler firstHandler = configuration.MessageHandlers[0];
                Assert.IsType<PassiveAuthenticationMessageHandler>(firstHandler);
            }
        }

        [Fact]
        public void SuppressDefaultHostAuthentication_Throws_WhenConfigurationIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                OwinHttpConfigurationExtensions.SuppressDefaultHostAuthentication(null);
            }, "configuration");
        }
    }
}
