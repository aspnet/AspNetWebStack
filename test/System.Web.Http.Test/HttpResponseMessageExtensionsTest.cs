// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class HttpResponseMessageExtensionsTest
    {
        private readonly HttpResponseMessage _response = new HttpResponseMessage();
        private readonly Mock<MediaTypeFormatter> _formatterMock = new Mock<MediaTypeFormatter>();

        public HttpResponseMessageExtensionsTest()
        {
            _formatterMock.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
        }

        [Fact]
        public void TryGetContentValue_WhenResponseParameterIsNull_Throws()
        {
            object value;
            Assert.ThrowsArgumentNull(() => HttpResponseMessageExtensions.TryGetContentValue<object>(null, out value), "response");
        }

        [Theory]
        [InlineData(default(bool))]
        [InlineData(default(int))]
        [InlineData(default(object))]
        public void TryGetContentValue_WhenResponseHasNoContent_ReturnsFalse<T>(T expectedResult)
        {
            T value;

            Assert.False(_response.TryGetContentValue<T>(out value));

            Assert.Equal(expectedResult, value);
        }


        [Theory]
        [InlineData(default(bool))]
        [InlineData(default(int))]
        [InlineData(default(object))]
        public void TryGetContentValue_WhenResponseHasNonObjectContent_ReturnsFalse<T>(T expectedResult)
        {
            _response.Content = new StringContent("43");
            T value;

            Assert.False(_response.TryGetContentValue<T>(out value));

            Assert.Equal(expectedResult, value);
        }

        [Theory]
        [InlineData(default(bool))]
        [InlineData(default(int))]
        [InlineData(default(object))]
        public void TryGetContentValue_WhenResponseHasObjectContentWithNullValue_ReturnsFalse<T>(T expectedResult)
        {
            _response.Content = new ObjectContent(typeof(object), null, _formatterMock.Object);
            T value;

            Assert.False(_response.TryGetContentValue<T>(out value));

            Assert.Equal(expectedResult, value);
        }

        [Theory]
        [InlineData(default(bool))]
        [InlineData(default(int))]
        public void TryGetContentValue_WhenResponseHasObjectContentWithIncompatibleValue_ReturnsFalse<T>(T expectedResult)
        {
            _response.Content = new ObjectContent<string>("42", _formatterMock.Object);
            T value;

            Assert.False(_response.TryGetContentValue<T>(out value));

            Assert.Equal(expectedResult, value);
        }

        [Fact]
        public void TryGetContentValue_WhenResponseHasObjectContentWithCompatibleValue_ReturnsTrue()
        {
            List<string> value = new List<string>();
            _response.Content = new ObjectContent<List<string>>(value, _formatterMock.Object);
            IList<string> result;

            Assert.True(_response.TryGetContentValue(out result));

            Assert.Same(value, result);
        }

        [Fact]
        public void TryGetContentValue_WhenResponseHasObjectContentValueTypeValue_RetrievingAsObjectReturnsTrue()
        {
            _response.Content = new ObjectContent<int>(32, _formatterMock.Object);
            object result;

            Assert.True(_response.TryGetContentValue(out result));

            Assert.Equal(32, result);
        }
    }
}
