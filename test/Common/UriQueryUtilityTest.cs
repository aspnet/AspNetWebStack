// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class UriQueryUtilityTest
    {
        public static TheoryDataSet<string, string, string> UriQueryData
        {
            get
            {
                return UriQueryTestData.UriQueryData;
            }
        }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(UriQueryUtility), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        public void UrlEncode_ReturnsNull()
        {
            Assert.Null(UriQueryUtility.UrlEncode(null));
        }

        [Fact]
        public void UrlDecode_ReturnsNull()
        {
            Assert.Null(UriQueryUtility.UrlDecode(null));
        }

        [Fact]
        public void UrlDecode_ParsesEmptySegmentsCorrectly()
        {
            int iterations = 16;
            List<string> segments = new List<string>();

            for (int index = 1; index < iterations; index++)
            {
                segments.Add("&");
                string query = string.Join("", segments);
                NameValueCollection result = ParseQueryString(query);
                Assert.NotNull(result);

                // Because this is a NameValueCollection, the same name appears only once
                Assert.Single(result);

                // Values should be a comma separated list of empty strings
                string[] values = result[""].Split(new char[] { ',' });

                // We expect length+1 segment as the final '&' counts as a segment
                Assert.Equal(index + 1, values.Length);
                foreach (var value in values)
                {
                    Assert.Equal("", value);
                }
            }
        }

        [Theory]
        [InlineData("N", "N", "")]
        [InlineData("%26", "&", "")]
        [InlineData("foo=%u0026", "foo", "%u0026")]
        [PropertyData("UriQueryData")]
        public void UrlDecode_ParsesCorrectly(string segment, string resultName, string resultValue)
        {
            int iterations = 16;
            List<string> segments = new List<string>();

            for (int index = 1; index < iterations; index++)
            {
                segments.Add(segment);
                string query = CreateQuery(segments.ToArray());
                NameValueCollection result = ParseQueryString(query);
                Assert.NotNull(result);

                // Because this is a NameValueCollection, the same name appears only once
                Assert.Single(result);

                // Values should be a comma separated list of resultValue
                string[] values = result[resultName].Split(new char[] { ',' });
                Assert.Equal(index, values.Length);
                foreach (var value in values)
                {
                    Assert.Equal(resultValue, value);
                }
            }
        }

        private static string CreateQuery(params string[] segments)
        {
            StringBuilder buffer = new StringBuilder();
            bool first = true;
            foreach (string segment in segments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    buffer.Append('&');
                }

                buffer.Append(segment);
            }

            return buffer.ToString();
        }

        private static NameValueCollection ParseQueryString(string query)
        {
            return new FormDataCollection(query).ReadAsNameValueCollection();
        }
    }
}