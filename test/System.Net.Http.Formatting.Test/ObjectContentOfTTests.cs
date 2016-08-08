// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class ObjectContentOfTTests
    {
        private MediaTypeHeaderValue _jsonHeaderValue = new MediaTypeHeaderValue("application/json");

        [Fact]
        public void Constructor_WhenFormatterParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => new ObjectContent<string>("", formatter: null), "formatter");
            Assert.ThrowsArgumentNull(() => new ObjectContent<string>("", formatter: null, mediaType: "foo/bar"), "formatter");
            Assert.ThrowsArgumentNull(() => new ObjectContent<string>("", formatter: null, mediaType: _jsonHeaderValue), "formatter");
        }

        [Fact]
        public void Constructor_SetsFormatterProperty()
        {
            var formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(f => f.CanWriteType(typeof(String))).Returns(true);
            var formatter = formatterMock.Object;

            var content = new ObjectContent<string>(null, formatter, mediaType: (MediaTypeHeaderValue)null);

            Assert.Same(formatter, content.Formatter);
        }

        [Fact]
        public void Constructor_CallsFormattersGetDefaultContentHeadersMethod()
        {
            var formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(f => f.CanWriteType(typeof(String))).Returns(true);

            var content = new ObjectContent(typeof(string), "", formatterMock.Object, _jsonHeaderValue);

            formatterMock.Verify(f => f.SetDefaultContentHeaders(typeof(string), content.Headers, _jsonHeaderValue),
                                 Times.Once());
        }
    }
}
