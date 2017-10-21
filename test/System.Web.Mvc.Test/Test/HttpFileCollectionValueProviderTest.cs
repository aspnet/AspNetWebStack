// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpFileCollectionValueProviderTest
    {
        private static readonly KeyValuePair<string, HttpPostedFileBase>[] _allFiles = new KeyValuePair<string, HttpPostedFileBase>[]
        {
            new KeyValuePair<string, HttpPostedFileBase>("foo", new MockHttpPostedFile(42, "fooFile1")),
            new KeyValuePair<string, HttpPostedFileBase>("foo", null),
            new KeyValuePair<string, HttpPostedFileBase>("foo", new MockHttpPostedFile(0, "") /* empty */),
            new KeyValuePair<string, HttpPostedFileBase>("foo", new MockHttpPostedFile(100, "fooFile2")),
            new KeyValuePair<string, HttpPostedFileBase>("bar.baz", new MockHttpPostedFile(200, "barBazFile"))
        };

        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetValueProvider();

            // Act
            bool result = valueProvider.ContainsPrefix("bar");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_DoesNotContainEmptyPrefixIfBackingStoreIsEmpty()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetEmptyValueProvider();

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_ThrowsIfPrefixIsNull()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetValueProvider();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.ContainsPrefix(null); }, "prefix");
        }

        [Fact]
        public void GetValue()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetValueProvider();

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);

            HttpPostedFileBase[] expectedRawValues = (from el in _allFiles
                                                      where el.Key == "foo"
                                                      let file = el.Value
                                                      let hasContent = (file != null && file.ContentLength > 0 && !String.IsNullOrEmpty(file.FileName))
                                                      select (hasContent) ? file : null).ToArray();
            Assert.Equal(expectedRawValues, (HttpPostedFileBase[])vpResult.RawValue);
            Assert.Equal("System.Web.HttpPostedFileBase[]", vpResult.AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetValueProvider();

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_ThrowsIfKeyIsNull()
        {
            // Arrange
            HttpFileCollectionValueProvider valueProvider = GetValueProvider();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.GetValue(null); }, "key");
        }

        private static HttpFileCollectionValueProvider GetEmptyValueProvider()
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Files.Count).Returns(0);
            return new HttpFileCollectionValueProvider(mockControllerContext.Object);
        }

        private static HttpFileCollectionValueProvider GetValueProvider()
        {
            Mock<HttpFileCollectionBase> mockFileCollection = new Mock<HttpFileCollectionBase>();
            mockFileCollection.SetupGet(c => c.Count).Returns(_allFiles.Length);
            mockFileCollection.SetupGet(c => c.AllKeys).Returns(_allFiles.Select(f => f.Key).ToArray());
            for (int i = 0; i < _allFiles.Length; i++)
            {
                int j = i;
                mockFileCollection.SetupGet(c => c[j]).Returns(_allFiles[j].Value);
            }

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(o => o.HttpContext.Request.Files).Returns(mockFileCollection.Object);

            return new HttpFileCollectionValueProvider(mockControllerContext.Object);
        }

        private sealed class MockHttpPostedFile : HttpPostedFileBase
        {
            private readonly int _contentLength;
            private readonly string _fileName;

            public MockHttpPostedFile(int contentLength, string fileName)
            {
                _contentLength = contentLength;
                _fileName = fileName;
            }

            public override int ContentLength
            {
                get { return _contentLength; }
            }

            public override string FileName
            {
                get { return _fileName; }
            }
        }
    }
}
