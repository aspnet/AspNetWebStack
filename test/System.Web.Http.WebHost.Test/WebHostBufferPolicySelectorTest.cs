// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost
{
    public class WebHostBufferPolicySelectorTest
    {
        private const string testString = "testString";

        public static TheoryDataSet<HttpContent, bool> OutputBufferingTestData
        {
            get
            {
                Mock<Stream> mockStream = new Mock<Stream>() { CallBase = true };
                return new TheoryDataSet<HttpContent, bool>()
                {
                    // Known length HttpContents other than OC should not buffer
                    { new StringContent(testString), false },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(testString)), false },
                    { new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(testString))), false },

                    // StreamContent (unknown length) should not buffer
                    { new StreamContent(mockStream.Object), false },

                    // PushStreamContent (unknown length) should not buffer
                    { new PushStreamContent((stream, headers, context) => {}), false },

                    // ObjectContent (unknown length) should buffer
                    { new ObjectContent<string>(testString, new XmlMediaTypeFormatter()), true }
                };
            }
        }

        public static TheoryDataSet<HttpContent> OutputBufferingTestData_NoExpectedResult
        {
            get
            {
                var dataSet = new TheoryDataSet<HttpContent>();
                foreach (var item in OutputBufferingTestData)
                {
                    dataSet.Add((HttpContent)item[0]);
                }

                return dataSet;
            }
        }

        [Fact]
        void UseBufferedInputStream_Returns_True()
        {
            // Arrange
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>() { CallBase = true };

            // Act & Assert
            Assert.True(new WebHostBufferPolicySelector().UseBufferedInputStream(mockContext.Object));
        }

        [Fact]
        void UseBufferedInputStream_ThrowsOnNull()
        {
            WebHostBufferPolicySelector selector = new WebHostBufferPolicySelector();
            Assert.ThrowsArgumentNull(() => selector.UseBufferedInputStream(null), "hostContext");
        }

        [Fact]
        void UseBufferedInputStream_Can_Be_Overridden()
        {
            // Arrange
            Mock<WebHostBufferPolicySelector> mockSelector = new Mock<WebHostBufferPolicySelector>();
            mockSelector.Setup((w) => w.UseBufferedInputStream(It.IsAny<HttpContextBase>())).Returns(false);
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>() { CallBase = true };

            // Act & Assert
            Assert.False(mockSelector.Object.UseBufferedInputStream(mockContext.Object));
        }

        [Fact]
        public void UseBufferedOutputStream_ThrowsOnNull()
        {
            WebHostBufferPolicySelector selector = new WebHostBufferPolicySelector();
            Assert.ThrowsArgumentNull(() => selector.UseBufferedOutputStream(null), "response");
        }

        [Theory]
        [PropertyData("OutputBufferingTestData")]
        public void UseBufferedOutputStream_ReturnsCorrectValue(HttpContent content, bool expectedResult)
        {
            // Arrange
            WebHostBufferPolicySelector selector = new WebHostBufferPolicySelector();
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = content;

            // Act
            bool actualResult = selector.UseBufferedOutputStream(response);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [PropertyData("OutputBufferingTestData_NoExpectedResult")]
        public void UseBufferedOutputStream_CausesContentLengthHeaderToBeSet(HttpContent content)
        {
            // Arrange & Act
            WebHostBufferPolicySelector selector = new WebHostBufferPolicySelector();
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = content;

            selector.UseBufferedOutputStream(response);

            IEnumerable<string> contentLengthEnumerable;
            bool isContentLengthInHeaders = content.Headers.TryGetValues("Content-Length", out contentLengthEnumerable);
            string[] contentLengthStrings = isContentLengthInHeaders ? contentLengthEnumerable.ToArray() : new string[0];
            long? contentLength = content.Headers.ContentLength;

            // Assert
            if (contentLength.HasValue && contentLength.Value >= 0)
            {
                // Setting the header is HttpContentHeader's responsibility, but we assert
                // it has happened here because it is UseBufferedOutputStream's responsibility
                // to cause that to happen. HttpControllerHandler relies on this.
                Assert.True(isContentLengthInHeaders);
                Assert.Equal(contentLength.Value, long.Parse(contentLengthStrings[0]));
            }
        }
    }
}
