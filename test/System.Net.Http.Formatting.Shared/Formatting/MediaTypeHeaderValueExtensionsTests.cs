// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class MediaTypeHeaderValueExtensionsTests
    {
        public static TheoryDataSet<string, string, int> EqualValues
        {
            get
            {
                // These values are all equal
                return new TheoryDataSet<string, string, int>
                {
                    { "text/xml", "text/xml", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "TEXT/XML", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "text/xml; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; parameter=value", "text/xml; parameter=value", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8; parameter=value", "text/xml; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.None },

                    { "text/*", "text/*", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*", "TEXT/*", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*; charset=utf-8", "text/*; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*; parameter=value", "text/*; parameter=value", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*; charset=utf-8; parameter=value", "text/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },

                    { "*/*", "*/*", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "*/*; charset=utf-8", "*/*; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "*/*; parameter=value", "*/*; parameter=value", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "*/*; charset=utf-8; parameter=value", "*/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                };
            }
        }

        public static TheoryDataSet<string, string, int> NonEqualValues
        {
            get
            {
                return new TheoryDataSet<string, string, int>
                {
                    // These values are all subsets. If compared in reverse they are all non-subsets.
                    { "text/xml", "text/xml; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "TEXT/XML; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "text/xml; parameter=value", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "text/xml; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.None },

                    { "text/*", "text/*; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*", "TEXT/*; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*", "text/*; parameter=value", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/*; charset=utf-8", "text/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },

                    { "text/xml", "text/*", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/xml", "text/*; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/xml", "TEXT/*; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/xml", "text/*; parameter=value", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },
                    { "text/xml; charset=utf-8", "text/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.SubtypeMediaRange },

                    { "*/*", "*/*; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "*/*", "*/*; parameter=value", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "*/*; charset=utf-8", "*/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },

                    { "text/*", "*/*", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/*", "*/*; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/*", "*/*; parameter=value", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/*; charset=utf-8", "*/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },

                    { "text/xml", "*/*", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/xml", "*/*; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/xml", "*/*; parameter=value", (int)MediaTypeHeaderValueRange.AllMediaRange },
                    { "text/xml; charset=utf-8", "*/*; parameter=value; charset=utf-8", (int)MediaTypeHeaderValueRange.AllMediaRange },
                };
            }
        }

        public static TheoryDataSet<string, string, int> NonOverlappingValues
        {
            get
            {
                return new TheoryDataSet<string, string, int>
                {
                    // These values are all value1 < value2 regardless of which value is first and second
                    // We do this asymmetric sorting algorithm to ensure that subsets are always <=0.
                    { "text/xml", "application/xml", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "APPLICATION/XML", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "application/xml; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "application/xml; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "application/xml; parameter=value", (int)MediaTypeHeaderValueRange.None },

                    { "text/xml", "text/plain", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "TEXT/PLAIN", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml", "text/plain; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "text/plain; charset=utf-8", (int)MediaTypeHeaderValueRange.None },
                    { "text/xml; charset=utf-8", "text/plain; parameter=value", (int)MediaTypeHeaderValueRange.None },
                };
            }
        }

        [Theory]
        [PropertyData("EqualValues")]
        public void IsSubsetOf_ReturnsTrueForEqualValues(string mediaType1, string mediaType2, int range)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            MediaTypeHeaderValueRange actualRange;
            Assert.True(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2, out actualRange));
            Assert.Equal(range, (int)actualRange);

            Assert.True(mediaTypeHeaderValue2.IsSubsetOf(mediaTypeHeaderValue1, out actualRange));
        }

        [Theory]
        [PropertyData("NonEqualValues")]
        public void IsSubsetOf_ReturnsTrueForNonEqualValues(string mediaType1, string mediaType2, int range)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            MediaTypeHeaderValueRange actualRange;
            Assert.True(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2, out actualRange));
            Assert.Equal(range, (int)actualRange);

            Assert.False(mediaTypeHeaderValue2.IsSubsetOf(mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("NonOverlappingValues")]
        public void IsSubsetOf_ReturnsFalseForNonOverlappingValues(string mediaType1, string mediaType2, int range)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            MediaTypeHeaderValueRange actualRange;
            Assert.False(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2, out actualRange));
            Assert.Equal(range, (int)actualRange);
        }
    }
}
