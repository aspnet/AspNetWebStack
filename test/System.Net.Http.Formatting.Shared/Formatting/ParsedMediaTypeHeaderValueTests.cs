// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class ParsedMediaTypeHeadeValueTests
    {
        public static TheoryDataSet<string> FullMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "*/*",
                    "*/*; charset=utf-8",
                    "*/*; charset=utf-8; q=1.0",
                };
            }
        }

        public static TheoryDataSet<string> SubTypeMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "text/*",
                    "TEXT/*",
                    "application/*; charset=utf-8",
                    "APPLICATION/*; charset=utf-8",
                    "APPLICATION/*; charset=utf-8; q=1.0",
                };
            }
        }

        public static TheoryDataSet<string> InvalidMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "*/text",
                    "*/XML",
                };
            }
        }
        public static TheoryDataSet<string> NonMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "text/plain",
                    "TEXT/XML",
                    "application/xml; charset=utf-8",
                    "APPLICATION/xml; charset=utf-8",
                };
            }
        }

        public static TheoryDataSet<string> InvalidNonMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    " ",
                    "\n",
                    "\t",
                    "text",
                    "text/",
                    "text\\", 
                    "\\", "//", 
                    "text/[", 
                    "text/ ", 
                    " text/", 
                    " text/ ", 
                    "text\\ ", 
                    " text\\", 
                    " text\\ ", 
                    "text\\xml", 
                    "text//xml" 
                };
            }
        }

        [Theory]
        [PropertyData("InvalidNonMediaRanges")]
        public void MediaTypeHeaderValue_EnsuresValidMediaType(string invalidMediaType)
        {
            Assert.Throws<Exception>(() => new MediaTypeHeaderValue(invalidMediaType), exceptionMessage: null, allowDerivedExceptions: true);
        }

        [Fact]
        public void TypesEqual_SameType_True()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/json"));
            Assert.True(parsedMediaType1.TypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void TypesEqual_SameTypeDifferentCase_True()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("TEXT/xml"));
            Assert.True(parsedMediaType1.TypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void TypesEqual_DifferentType_False()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("application/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            Assert.False(parsedMediaType1.TypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void TypesEqual_DifferentTypeSameLength_False()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("texx/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            Assert.False(parsedMediaType1.TypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void SubTypesEqual_SameType_True()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("application/xml"));
            Assert.True(parsedMediaType1.SubTypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void SubTypesEqual_SameTypeDifferentCase_True()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/XML"));
            Assert.True(parsedMediaType1.SubTypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void SubTypesEqual_DifferentType_False()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/json"));
            Assert.False(parsedMediaType1.SubTypesEqual(ref parsedMediaType2));
        }

        [Fact]
        public void SubTypesEqual_DifferentTypeSameLength_False()
        {
            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/xml"));
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(MediaTypeHeaderValue.Parse("text/yml"));
            Assert.False(parsedMediaType1.SubTypesEqual(ref parsedMediaType2));
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        [PropertyData("SubTypeMediaRanges")]
        public void IsSubTypeMediaRange_ReturnsTrueForSubTypeMediaRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.True(parsedMediaType.IsSubtypeMediaRange);
        }

        [Theory]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void IsSubTypeMediaRange_ReturnsFalseForNonSubTypeMediaRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.False(parsedMediaType.IsSubtypeMediaRange);
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        public void IsAllMediaRange_ReturnsTrueForFullMediaTypeRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.True(parsedMediaType.IsAllMediaRange);
        }

        [Theory]
        [PropertyData("SubTypeMediaRanges")]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void IsAllMediaRange_ReturnsFalseForNonFullMediaTypeRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.False(parsedMediaType.IsAllMediaRange);
        }
    }
}
