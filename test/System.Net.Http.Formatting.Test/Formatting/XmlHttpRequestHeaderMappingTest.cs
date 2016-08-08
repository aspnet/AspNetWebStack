// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class XmlHttpRequestHeaderMappingTest
    {
        // HttpRequestMessage request, double expectedMatch
        public static TheoryDataSet<HttpRequestMessage, double> TryMatchMediaTypeData
        {
            get
            {
                return new TheoryDataSet<HttpRequestMessage, double>()
                {
                    { CreateXhrRequest(), 1.0 },
                    { CreateXhrRequest("*/*"), 1.0 },
                    { CreateXhrRequest("*/*; q=0.5"), 1.0 },

                    { CreateXhrRequest("text/*"), 0.0 },
                    { CreateXhrRequest("text/*; q=0.5"), 0.0 },
                    { CreateXhrRequest("application/xml"), 0.0 },
                    { CreateXhrRequest("application/xml; q=0.5"), 0.0 },
                    { CreateXhrRequest("text/test", "*/*; q=0.5"), 0.0 },
                };
            }
        }

        private static HttpRequestMessage CreateXhrRequest(params string[] acceptHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("X-Requested-With", "XmlHttpRequest");
            foreach (string accept in acceptHeaders)
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(accept));
            }
            return request;
        }

        [Fact]
        public void Constructor_Initializes()
        {
            XmlHttpRequestHeaderMapping mapping = new XmlHttpRequestHeaderMapping();
            Assert.Equal("x-requested-with", mapping.HeaderName);
            Assert.Equal("XMLHttpRequest", mapping.HeaderValue);
            Assert.Equal(StringComparison.OrdinalIgnoreCase, mapping.HeaderValueComparison);
            Assert.True(mapping.IsValueSubstring);
            Assert.Equal(MediaTypeConstants.ApplicationJsonMediaType, mapping.MediaType);
        }

        [Fact]
        public void TryMatchMediaType_ThrowsOnNull()
        {
            XmlHttpRequestHeaderMapping mapping = new XmlHttpRequestHeaderMapping();
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(null), "request");
        }

        [Theory]
        [PropertyData("TryMatchMediaTypeData")]
        public void TryMatchMediaType_Matches(HttpRequestMessage request, double expectedMatch)
        {
            // Arrange
            XmlHttpRequestHeaderMapping mapping = new XmlHttpRequestHeaderMapping();

            // Act
            double actualMatch = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(expectedMatch, actualMatch);
        }
    }
}
