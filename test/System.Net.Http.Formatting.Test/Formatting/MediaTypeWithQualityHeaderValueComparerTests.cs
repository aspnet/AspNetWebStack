// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class MediaTypeWithQualityHeaderValueComparerTests
    {
        public static TheoryDataSet<string, string> EqualValues
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "text/xml", "text/xml" },
                    { "text/xml", "text/xml; q=1" },
                    { "text/xml", "text/xml; q=1.0" },
                    { "text/xml", "text/xml; q=1.0000" },
                    { "text/xml", "TEXT/XML" },
                    { "text/plain", "text/xml" },
                    { "text/*", "text/*" },
                    { "text/*", "TEXT/*" },
                    { "*/*", "*/*" },
                    { "text/xml", "text/xml; charset=utf8" },
                    { "text/xml", "text/xml; parameter=value" },
                    { "text/xml; parameter=value", "text/xml; parameter=value" },
                    { "text/xml; parameter1=value1", "text/xml; parameter2=value2" },
                    { "text/xml; q=0.5", "text/xml; q=0.50" },
                    { "application/xml; q=0.5", "text/xml; q=0.5" },
                    { "application/xml; q=0.1", "text/xml; q=0.1" },
                    { "application/xml; parameter=value1; q=0.5", "text/xml; parameter=value2; q=0.5" },
                    { "text/xml", "text/xml;q=1" },
                    { "text/xml", "text/xml;q=1.0" },
                    { "text/xml", "text/xml; q=1.00000" },
                    { "text/xml; q=0.5", "text/xml; q=0.5" },
                    { "text/xml; q=1.0", "text/xml; q=1.0" },
                    { "*/*", "*/*;q=1" },
                    { "*/*", "*/*; q=1" },
                    { "*/*", "*/*;q=1.0" },
                    { "*/*", "*/*; q=1.0" },
                    { "*/*; q=0.5", "*/*; q=0.5" },
                    { "*/*; q=1.0", "*/*; q=1.0" },
                };
            }
        }

        public static TheoryDataSet<string, string> NonEqualValues
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "text/plain; q=0.5", "text/plain" },
                    { "text/plain; q=0.5", "application/xml" },
                    { "text/plain; q=0.5", "text/plain; q=1.0" },
                    { "text/plain; q=0.5", "text/xml; q=1.0" },
                    { "text/*", "text/plain" },
                    { "application/*", "text/plain" },
                    { "*/*", "text/xml" },
                    { "*/*", "text/*" },
                    { "*/*;q=0.5", "*/*;q=0.6" },
                    { "*/*;q=0.5", "text/*;q=0.5" },
                    { "*/*;q=1", "text/plain" },
                    { "*/*; q=1", "text/plain" },
                    { "*/*;q=1.0", "text/plain" },
                    { "*/*; q=1.0", "text/plain" },
                    { "*/*; q=0.5", "text/plain; q=0.5" },
                    { "*/*; q=1.0", "text/plain; q=1.0" },
                    { "*/*; q=0.5", "text/*; q=0.6" },
                };
            }
        }

        public static TheoryDataSet<string[], string[]> BeforeAfterSortedValues
        {
            get
            {
                return new TheoryDataSet<string[], string[]>
                {
                    {
                        new string[]
                        {
                            "application/*",
                            "text/plain",
                            "text/plain;q=1.0",
                            "text/plain",
                            "text/plain;q=0",
                            "*/*;q=0.8",
                            "*/*;q=1",
                            "text/*;q=1",
                            "text/plain;q=0.8",
                            "text/*;q=0.8",
                            "text/*;q=0.6",
                            "text/*;q=1.0",
                            "*/*;q=0.4",
                            "text/plain;q=0.6",
                            "text/xml",
                        },
                        new string[]
                        {
                            "text/plain",
                            "text/plain;q=1.0",
                            "text/plain",
                            "text/xml",
                            "application/*",
                            "text/*;q=1",
                            "text/*;q=1.0",
                            "*/*;q=1",
                            "text/plain;q=0.8",
                            "text/*;q=0.8",
                            "*/*;q=0.8",
                            "text/plain;q=0.6",
                            "text/*;q=0.6",
                            "*/*;q=0.4",
                            "text/plain;q=0",
                        }
                    }
                };
            }
        }

        [Fact]
        public void StaticComparer_Returns_SameInstance()
        {
            MediaTypeWithQualityHeaderValueComparer comparer1 = MediaTypeWithQualityHeaderValueComparer.QualityComparer;
            MediaTypeWithQualityHeaderValueComparer comparer2 = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            Assert.NotNull(comparer1);
            Assert.Same(comparer1, comparer2);
        }

        [Theory]
        [PropertyData("EqualValues")]
        public void ComparerReturnsZeroForEqualValues(string mediaType1, string mediaType2)
        {
            // Arrange
            MediaTypeWithQualityHeaderValueComparer comparer = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            // Act
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue1 = MediaTypeWithQualityHeaderValue.Parse(mediaType1);
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue2 = MediaTypeWithQualityHeaderValue.Parse(mediaType2);

            // Assert
            Assert.Equal(0, comparer.Compare(mediaTypeHeaderValue1, mediaTypeHeaderValue2));
            Assert.Equal(0, comparer.Compare(mediaTypeHeaderValue2, mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("NonEqualValues")]
        public void ComparerReturnsNonZeroForNonEqualValues(string mediaType1, string mediaType2)
        {
            // Arrange
            MediaTypeWithQualityHeaderValueComparer comparer = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            // Act
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue1 = MediaTypeWithQualityHeaderValue.Parse(mediaType1);
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue2 = MediaTypeWithQualityHeaderValue.Parse(mediaType2);

            // Assert
            Assert.Equal(-1, comparer.Compare(mediaTypeHeaderValue1, mediaTypeHeaderValue2));
            Assert.Equal(1, comparer.Compare(mediaTypeHeaderValue2, mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("BeforeAfterSortedValues")]
        public void ComparerSortsListCorrectly(string[] unsorted, string[] expectedSorted)
        {
            // Arrange
            IEnumerable<MediaTypeWithQualityHeaderValue> unsortedValues =
                unsorted.Select(u => MediaTypeWithQualityHeaderValue.Parse(u));

            IEnumerable<MediaTypeWithQualityHeaderValue> expectedSortedValues =
                expectedSorted.Select(u => MediaTypeWithQualityHeaderValue.Parse(u));

            // Act
            IEnumerable<MediaTypeWithQualityHeaderValue> actualSorted = unsortedValues.OrderByDescending(m => m, MediaTypeWithQualityHeaderValueComparer.QualityComparer);

            // Assert
            Assert.True(expectedSortedValues.SequenceEqual(actualSorted));
        }
    }
}
