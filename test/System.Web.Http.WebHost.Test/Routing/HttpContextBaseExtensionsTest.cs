// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost.Routing
{
    public class HttpContextBaseExtensionsTest
    {
        [Fact]
        public void GetOrCreateHttpRequestMessageFromHttpContextCopiesHeaders()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            Dictionary<string, object> items = new Dictionary<string, object>();
            contextMock.Setup(o => o.Items).Returns(items);
            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.HttpMethod).Returns("GET");
            requestMock.Setup(r => r.InputStream).Returns(new MemoryStream());
            NameValueCollection col = new NameValueCollection();
            col.Add("customHeader", "customHeaderValue");
            requestMock.Setup(r => r.Headers).Returns(col);
            contextMock.Setup(o => o.Request).Returns(requestMock.Object);

            // Act
            contextMock.Object.GetOrCreateHttpRequestMessage();

            // Assert
            HttpRequestMessage request = contextMock.Object.GetHttpRequestMessage();
            Assert.NotNull(request);
            Assert.Equal(HttpMethod.Get, request.Method);
            IEnumerable<string> headerValues;
            Assert.True(request.Headers.TryGetValues("customHeader", out headerValues));
            string headerValue = Assert.Single(headerValues);
            Assert.Equal("customHeaderValue", headerValue);
        }
    }
}
