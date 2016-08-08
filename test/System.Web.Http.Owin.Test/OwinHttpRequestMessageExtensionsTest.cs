// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.TestCommon;

namespace System.Web.Http.Owin
{
    public class OwinHttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetOwinContext_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.GetOwinContext(null); }, "request");
        }

        [Fact]
        public void GetOwinContext_ReturnsNull_WhenNotSet()
        {
            // Act & Assert
            Assert.Null(new HttpRequestMessage().GetOwinContext());
        }

        [Fact]
        public void GetOwinContext_ReturnsSetContextInstance()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var context = new OwinContext();
            request.SetOwinContext(context);

            // Act & Asert
            Assert.Same(context, request.GetOwinContext());
        }

        [Fact]
        public void SetOwinContext_Throws_WhenRequestIsNull()
        {
            // Arrange
            var context = new OwinContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.SetOwinContext(null, context); },
                "request");
        }

        [Fact]
        public void GetOwinContext_ReturnsOwinContextAndRemovesEnvironmentProperty_AfterSetEnvironmentProperty()
        {
            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                var environment = new Dictionary<string, object>();
                request.Properties.Add("MS_OwinEnvironment", environment);

                // Act
                var context = request.GetOwinContext();

                // Assert
                Assert.NotNull(context);
                Assert.Same(environment, context.Environment);
                Assert.False(request.Properties.ContainsKey("MS_OwinEnvironment"));
            }
        }

        [Fact]
        public void SetOwinContext_RemovesOwinEnvironmentProperty_WhenPresent()
        {
            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                var environment = new Dictionary<string, object>();
                request.Properties.Add("MS_OwinEnvironment", environment);
                var context = new OwinContext();

                // Act
                request.SetOwinContext(context);

                // Assert
                Assert.False(request.Properties.ContainsKey("MS_OwinEnvironment"));
            }
        }

        [Fact]
        public void SetOwinContext_Throws_WhenContextIsNull()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.SetOwinContext(request, null); },
                "context");
        }

        [Fact]
        public void GetOwinEnvironment_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.GetOwinEnvironment(null); }, "request");
        }

        [Fact]
        public void GetOwinEnvironment_ReturnsNull_WhenNotSet()
        {
            // Act & Assert
            Assert.Null(new HttpRequestMessage().GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinEnvironment_ReturnsSetEnvironmentInstance()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var environment = new Dictionary<string, object>();
            request.SetOwinEnvironment(environment);

            // Act & Assert
            Assert.Same(environment, request.GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinEnvironment_ReturnsSetContextEnvironmentInstance()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var environment = new Dictionary<string, object>();
            var context = new OwinContext(environment);
            request.SetOwinContext(context);

            // Act & Assert
            Assert.Same(environment, request.GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinContext_ReturnsOwinContext_AfterSetContextEnvironment()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var environment = new Dictionary<string, object>();
            request.SetOwinEnvironment(environment);

            // Act
            IOwinContext context = request.GetOwinContext();

            // Assert
            Assert.IsType<OwinContext>(context);
            Assert.Same(environment, context.Environment);
        }

        [Fact]
        public void SetOwinEnvironment_Throws_WhenRequestIsNull()
        {
            // Arrange
            var environment = new Dictionary<string, object>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => { OwinHttpRequestMessageExtensions.SetOwinEnvironment(null, environment); }, "request");
        }

        [Fact]
        public void SetOwinEnvironment_Throws_WhenEnvironmentIsNull()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.SetOwinEnvironment(request, null); },
                "environment");
        }
    }
}
