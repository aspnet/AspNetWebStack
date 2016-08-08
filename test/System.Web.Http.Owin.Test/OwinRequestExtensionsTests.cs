// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class OwinRequestExtensionsTests
    {
        [Fact]
        public void DisableBuffering_IfActionIsAvailable_CallsAction()
        {
            // Arrange
            bool bufferingDisabled = false;
            Action disableBufferingAction = () => bufferingDisabled = true;
            IDictionary<string, object> environment = CreateStubEnvironment(disableBufferingAction);
            IOwinRequest request = CreateStubRequest(environment);

            // Act
            OwinRequestExtensions.DisableBuffering(request);

            // Assert
            Assert.True(bufferingDisabled);
        }

        [Fact]
        public void DisableBuffering_IfRequestIsNull_Throws()
        {
            // Arrange
            IOwinRequest request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => OwinRequestExtensions.DisableBuffering(request), "request");
        }

        [Fact]
        public void DisableBuffering_IfEnvironmentIsNull_DoesNotThrow()
        {
            // Arrange
            IDictionary<string, object> environment = null;
            IOwinRequest request = CreateStubRequest(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => request.DisableBuffering());
        }

        [Fact]
        public void DisableBuffering_IfServerDisableResponseBufferingIsAbsent_DoesNotThrow()
        {
            // Arrange
            Mock<IDictionary<string, object>> environmentMock = new Mock<IDictionary<string, object>>(MockBehavior.Strict);
            IDictionary<string, object> environment = CreateStubEnvironment(null, hasDisableBufferingAction: false);
            IOwinRequest request = CreateStubRequest(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => request.DisableBuffering());
        }

        [Fact]
        public void DisableBuffering_IfServerDisableResponseBufferingIsNotAction_DoesNotThrow()
        {
            // Arrange
            object nonAction = new object();
            IDictionary<string, object> environment = CreateStubEnvironment(nonAction);
            IOwinRequest request = CreateStubRequest(environment);

            // Act & Assert
            Assert.DoesNotThrow(() => request.DisableBuffering());
        }

        [Fact]
        public void GetContentLength_IfHeadersIsNull_ReturnsNull()
        {
            // Arrange
            IOwinRequest request = CreateStubRequest(headers: null);
            Assert.Null(request.Headers); // Guard

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfHeadersDoesNotContainContentLengthHeader_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders();
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsNull_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", null);
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsEmpty_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[0]);
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsMultiple_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "123", "456" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsSingleNullValue_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { null });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsSingleNonIntegerValue_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "abc" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsNegative_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "-1" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfValid_ReturnsValue()
        {
            int expected = 123;
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { expected.ToString() });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = request.GetContentLength();

            // Assert
            Assert.True(length.HasValue);
            Assert.Equal(expected, length.Value);
        }

        private static IDictionary<string, object> CreateStubEnvironment(object disableBufferingAction)
        {
            return CreateStubEnvironment(disableBufferingAction, hasDisableBufferingAction: true);
        }

        private static IDictionary<string, object> CreateStubEnvironment(object disableBufferingAction, bool hasDisableBufferingAction)
        {
            Mock<IDictionary<string, object>> mock = new Mock<IDictionary<string, object>>(MockBehavior.Strict);
            mock.Setup(d => d.TryGetValue("server.DisableRequestBuffering", out disableBufferingAction)).Returns(hasDisableBufferingAction);
            return mock.Object;
        }

        private static IHeaderDictionary CreateStubHeaders(string key, string[] value)
        {
            Mock<IHeaderDictionary> mock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            mock.Setup(h => h.TryGetValue(key, out value)).Returns(true);
            return mock.Object;
        }

        private static IHeaderDictionary CreateStubHeaders()
        {
            Mock<IHeaderDictionary> mock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            string[] value = null;
            mock.Setup(h => h.TryGetValue(It.IsAny<string>(), out value)).Returns(false);
            return mock.Object;
        }

        private static IOwinRequest CreateStubRequest(IHeaderDictionary headers)
        {
            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.SetupGet(r => r.Headers).Returns(headers);
            return mock.Object;
        }

        private static IOwinRequest CreateStubRequest(IDictionary<string, object> environment)
        {
            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.SetupGet(r => r.Environment).Returns(environment);
            return mock.Object;
        }
    }
}
