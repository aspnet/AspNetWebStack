// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class SessionStateTempDataProviderTest
    {
        [Fact]
        public void Load_NullSession_ReturnsEmptyDictionary()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();

            // Act
            IDictionary<string, object> tempDataDictionary = testProvider.LoadTempData(GetControllerContext());

            // Assert
            Assert.Empty(tempDataDictionary);
        }

        [Fact]
        public void Load_NonNullSession_NoSessionData_ReturnsEmptyDictionary()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            Mock<HttpSessionStateBase> mockSessionStateBase = new Mock<HttpSessionStateBase>();
            mockControllerContext.Setup(c => c.HttpContext.Session).Returns(mockSessionStateBase.Object);

            // Act
            IDictionary<string, object> result = testProvider.LoadTempData(mockControllerContext.Object);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Load_NonNullSession_IncorrectSessionDataType_ReturnsEmptyDictionary()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            Mock<HttpSessionStateBase> mockSessionStateBase = new Mock<HttpSessionStateBase>();
            mockControllerContext.Setup(c => c.HttpContext.Session).Returns(mockSessionStateBase.Object);
            mockSessionStateBase.Setup(ssb => ssb[SessionStateTempDataProvider.TempDataSessionStateKey]).Returns(42);

            // Act
            IDictionary<string, object> result = testProvider.LoadTempData(mockControllerContext.Object);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Load_NonNullSession_CorrectSessionDataType_ReturnsSessionData()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Dictionary<string, object> tempData = new Dictionary<string, object> { { "foo", "bar" } };
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            Mock<HttpSessionStateBase> mockSessionStateBase = new Mock<HttpSessionStateBase>();
            mockControllerContext.Setup(c => c.HttpContext.Session).Returns(mockSessionStateBase.Object);
            mockSessionStateBase.Setup(ssb => ssb[SessionStateTempDataProvider.TempDataSessionStateKey]).Returns(tempData);

            // Act
            var result = testProvider.LoadTempData(mockControllerContext.Object);

            // Assert
            Assert.Same(tempData, result);
        }

        [Fact]
        public void Save_NullSession_NullDictionary_DoesNotThrow()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();

            // Act
            testProvider.SaveTempData(GetControllerContext(), null);
        }

        [Fact]
        public void Save_NullSession_EmptyDictionary_DoesNotThrow()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();

            // Act
            testProvider.SaveTempData(GetControllerContext(), new Dictionary<string, object>());
        }

        [Fact]
        public void Save_NullSession_NonEmptyDictionary_Throws()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { testProvider.SaveTempData(GetControllerContext(), new Dictionary<string, object> { { "foo", "bar" } }); },
                "The SessionStateTempDataProvider class requires session state to be enabled.");
        }

        [Fact]
        public void Save_NonNullSession_TempDataIsDirty_AssignsTempDataDictionaryIntoSession()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Dictionary<string, object> tempData = new Dictionary<string, object> { { "foo", "bar" } };
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            Mock<HttpSessionStateBase> mockSessionStateBase = new Mock<HttpSessionStateBase>();
            mockControllerContext.Setup(c => c.HttpContext.Session).Returns(mockSessionStateBase.Object);
            mockSessionStateBase.SetupSet(ssb => ssb[SessionStateTempDataProvider.TempDataSessionStateKey] = tempData);

            // Act
            testProvider.SaveTempData(mockControllerContext.Object, tempData);

            // Assert
            mockSessionStateBase.VerifyAll();
        }

        [Fact]
        public void Save_NonNullSession_TempDataIsNotDirty_KeyDoesNotExistInSession_SessionRemainsUntouched()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Dictionary<string, object> tempData = new Dictionary<string, object>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.Setup(o => o.HttpContext.Session[SessionStateTempDataProvider.TempDataSessionStateKey]).Returns(null);

            // Act
            testProvider.SaveTempData(mockControllerContext.Object, tempData);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void Save_NonNullSession_TempDataIsNotDirty_KeyExistsInSession_KeyRemovedFromSession()
        {
            // Arrange
            SessionStateTempDataProvider testProvider = new SessionStateTempDataProvider();
            Dictionary<string, object> tempData = new Dictionary<string, object>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.Setup(o => o.HttpContext.Session[SessionStateTempDataProvider.TempDataSessionStateKey]).Returns(new object());
            mockControllerContext.Setup(o => o.HttpContext.Session.Remove(SessionStateTempDataProvider.TempDataSessionStateKey)).Verifiable();

            // Act
            testProvider.SaveTempData(mockControllerContext.Object, tempData);

            // Assert
            mockControllerContext.Verify();
        }

        private static ControllerContext GetControllerContext()
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Session).Returns((HttpSessionStateBase)null);
            return mockControllerContext.Object;
        }
    }
}
