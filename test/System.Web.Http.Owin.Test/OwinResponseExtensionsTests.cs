// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class OwinResponseExtensionsTests
    {
        [Fact]
        public void DisableBuffering_IfActionIsAvailable_CallsAction()
        {
            // Arrange
            bool bufferingDisabled = false;
            Action disableBufferingAction = () => bufferingDisabled = true;
            IDictionary<string, object> environment = CreateStubEnvironment(disableBufferingAction);
            IOwinResponse response = CreateStubResponse(environment);

            // Act
            OwinResponseExtensions.DisableBuffering(response);

            // Assert
            Assert.True(bufferingDisabled);
        }

        [Fact]
        public void DisableBuffering_IfResponseIsNull_Throws()
        {
            // Arrange
            IOwinResponse response = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => OwinResponseExtensions.DisableBuffering(response), "response");
        }

        [Fact]
        public void DisableBuffering_IfEnvironmentIsNull_DoesNotThrow()
        {
            // Arrange
            IDictionary<string, object> environment = null;
            IOwinResponse response = CreateStubResponse(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => response.DisableBuffering());
        }

        [Fact]
        public void DisableBuffering_IfServerDisableResponseBufferingIsAbsent_DoesNotThrow()
        {
            // Arrange
            Mock<IDictionary<string, object>> environmentMock = new Mock<IDictionary<string,object>>(MockBehavior.Strict);
            IDictionary<string, object> environment = CreateStubEnvironment(null, hasDisableBufferingAction: false);
            IOwinResponse response = CreateStubResponse(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => response.DisableBuffering());
        }

        [Fact]
        public void DisableBuffering_IfServerDisableResponseBufferingIsNotAction_DoesNotThrow()
        {
            // Arrange
            object nonAction = new object();
            IDictionary<string, object> environment = CreateStubEnvironment(nonAction);
            IOwinResponse response = CreateStubResponse(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => response.DisableBuffering());
        }

        private static IDictionary<string, object> CreateStubEnvironment(object disableBufferingAction)
        {
            return CreateStubEnvironment(disableBufferingAction, hasDisableBufferingAction: true);
        }

        private static IDictionary<string, object> CreateStubEnvironment(object disableBufferingAction, bool hasDisableBufferingAction)
        {
            Mock<IDictionary<string, object>> mock = new Mock<IDictionary<string, object>>(MockBehavior.Strict);
            mock.Setup(d => d.TryGetValue("server.DisableResponseBuffering", out disableBufferingAction)).Returns(hasDisableBufferingAction);
            return mock.Object;
        }

        private static IOwinResponse CreateStubResponse(IDictionary<string, object> environment)
        {
            Mock<IOwinResponse> mock = new Mock<IOwinResponse>(MockBehavior.Strict);
            mock.SetupGet(r => r.Environment).Returns(environment);
            return mock.Object;
        }
    }
}
