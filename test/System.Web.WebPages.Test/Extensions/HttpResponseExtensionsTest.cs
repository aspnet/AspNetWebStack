// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.WebPages.Test.Helpers
{
    public class HttpResponseExtensionsTest
    {
        HttpResponseBase _response;
        string _redirectUrl;
        StringBuilder _output;
        Stream _outputStream;

        public HttpResponseExtensionsTest()
        {
            _output = new StringBuilder();
            _outputStream = new MemoryStream();
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.SetupProperty(response => response.StatusCode);
            mockResponse.SetupProperty(response => response.ContentType);
            mockResponse.Setup(response => response.Redirect(It.IsAny<string>())).Callback((string url) => _redirectUrl = url);
            mockResponse.Setup(response => response.Write(It.IsAny<string>())).Callback((string str) => _output.Append(str));
            mockResponse.Setup(response => response.OutputStream).Returns(_outputStream);
            mockResponse.Setup(response => response.OutputStream).Returns(_outputStream);
            mockResponse.Setup(response => response.Output).Returns(new StringWriter(_output));
            _response = mockResponse.Object;
        }

        [Fact]
        public void SetStatusWithIntTest()
        {
            int status = 200;
            _response.SetStatus(status);
            Assert.Equal(status, _response.StatusCode);
        }

        [Fact]
        public void SetStatusWithHttpStatusCodeTest()
        {
            HttpStatusCode status = HttpStatusCode.Forbidden;
            _response.SetStatus(status);
            Assert.Equal((int)status, _response.StatusCode);
        }

        [Fact]
        public void WriteBinaryTest()
        {
            string foo = "I am a string, please don't mangle me!";
            _response.WriteBinary(ASCIIEncoding.ASCII.GetBytes(foo));
            _outputStream.Flush();
            _outputStream.Position = 0;
            StreamReader reader = new StreamReader(_outputStream);
            Assert.Equal(foo, reader.ReadToEnd());
        }

        [Fact]
        public void WriteBinaryWithMimeTypeTest()
        {
            string foo = "I am a string, please don't mangle me!";
            string mimeType = "mime/foo";
            _response.WriteBinary(ASCIIEncoding.ASCII.GetBytes(foo), mimeType);
            _outputStream.Flush();
            _outputStream.Position = 0;
            StreamReader reader = new StreamReader(_outputStream);
            Assert.Equal(foo, reader.ReadToEnd());
            Assert.Equal(mimeType, _response.ContentType);
        }

        [Fact]
        public void OutputCacheSetsExpirationTimeBasedOnCurrentContext()
        {
            // Arrange
            var timestamp = new DateTime(2011, 1, 1, 0, 0, 0);
            var context = new Mock<HttpContextBase>();
            context.SetupGet(c => c.Timestamp).Returns(timestamp);
            var response = new Mock<HttpResponseBase>().Object;

            var cache = new Mock<HttpCachePolicyBase>();
            cache.Setup(c => c.SetCacheability(It.Is<HttpCacheability>(p => p == HttpCacheability.Public))).Verifiable();
            cache.Setup(c => c.SetExpires(It.Is<DateTime>(p => p == timestamp.AddSeconds(20)))).Verifiable();
            cache.Setup(c => c.SetMaxAge(It.Is<TimeSpan>(p => p == TimeSpan.FromSeconds(20)))).Verifiable();
            cache.Setup(c => c.SetValidUntilExpires(It.Is<bool>(p => p == true))).Verifiable();
            cache.Setup(c => c.SetLastModified(It.Is<DateTime>(p => p == timestamp))).Verifiable();
            cache.Setup(c => c.SetSlidingExpiration(It.Is<bool>(p => p == false))).Verifiable();

            // Act
            ResponseExtensions.OutputCache(context.Object, cache.Object, 20, false, null, null, null, HttpCacheability.Public);

            // Assert
            cache.VerifyAll();
        }

        [Fact]
        public void OutputCacheSetsVaryByValues()
        {
            // Arrange
            var timestamp = new DateTime(2011, 1, 1, 0, 0, 0);
            var context = new Mock<HttpContextBase>();
            context.SetupGet(c => c.Timestamp).Returns(timestamp);
            var response = new Mock<HttpResponseBase>().Object;

            var varyByParams = new HttpCacheVaryByParams();
            var varyByHeader = new HttpCacheVaryByHeaders();
            var varyByContentEncoding = new HttpCacheVaryByContentEncodings();

            var cache = new Mock<HttpCachePolicyBase>();
            cache.SetupGet(c => c.VaryByParams).Returns(varyByParams);
            cache.SetupGet(c => c.VaryByHeaders).Returns(varyByHeader);
            cache.SetupGet(c => c.VaryByContentEncodings).Returns(varyByContentEncoding);

            // Act
            ResponseExtensions.OutputCache(context.Object, cache.Object, 20, false, new[] { "foo" }, new[] { "bar", "bar2" },
                                           new[] { "baz", "baz2" }, HttpCacheability.Public);

            // Assert
            Assert.True(varyByParams["foo"]);
            Assert.True(varyByHeader["bar"]);
            Assert.True(varyByHeader["bar2"]);
            Assert.True(varyByContentEncoding["baz"]);
            Assert.True(varyByContentEncoding["baz2"]);
        }
    }
}
