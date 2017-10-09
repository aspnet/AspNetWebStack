// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterCollectionTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatterCollection), TypeAssert.TypeProperties.IsPublicVisibleClass, typeof(Collection<MediaTypeFormatter>));
        }

        [Fact]
        public void Constructor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();

            Assert.Equal(3, collection.Count);
            Assert.NotNull(collection.XmlFormatter);
            Assert.NotNull(collection.JsonFormatter);
            Assert.NotNull(collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void Constructor1_AcceptsEmptyList()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Empty(collection);
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "AllFormatterCollections")]
        public void Constructor1_SetsProperties(IEnumerable<MediaTypeFormatter> formatterCollection)
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatterCollection);
            if (collection.OfType<XmlMediaTypeFormatter>().Any())
            {
                Assert.NotNull(collection.XmlFormatter);
            }
            else
            {
                Assert.Null(collection.XmlFormatter);
            }

            if (collection.OfType<JsonMediaTypeFormatter>().Any())
            {
                Assert.NotNull(collection.JsonFormatter);
            }
            else
            {
                Assert.Null(collection.JsonFormatter);
            }
        }

        [Fact]
        public void Constructor1_SetsDerivedFormatters()
        {
            // force to array to get stable instances
            MediaTypeFormatter[] derivedFormatters = HttpTestData.DerivedFormatters.ToArray();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(derivedFormatters);
            Assert.True(derivedFormatters.SequenceEqual(collection));
        }

        [Fact]
        public void Constructor1_ThrowsWithNullFormatters()
        {
            Assert.ThrowsArgumentNull(() => new MediaTypeFormatterCollection(null), "formatters");
        }

        [Fact]
        public void Constructor1_ThrowsWithNullFormatterInCollection()
        {
            Assert.ThrowsArgument(
                () => new MediaTypeFormatterCollection(new MediaTypeFormatter[] { null }), "formatters",
                Error.Format(Properties.Resources.CannotHaveNullInList,
                typeof(MediaTypeFormatter).Name));
        }

        [Fact]
        public void Constructor1_AcceptsDuplicateFormatterTypes()
        {
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[]
            {
                new XmlMediaTypeFormatter(),
                new JsonMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter(),

                new XmlMediaTypeFormatter(),
                new JsonMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter(),
            };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);
            Assert.True(formatters.SequenceEqual(collection));
        }

        [Fact]
        public void MediaTypeFormatterCollection_Changing_FiresOnClear()
        {
            TestChanging((collection) => collection.Clear(), 1);
        }

        [Fact]
        public void MediaTypeFormatterCollection_Changing_FiresOnInsert()
        {
            TestChanging((collection) => collection.Insert(0, new XmlMediaTypeFormatter()), 1);
        }

        [Fact]
        public void MediaTypeFormatterCollection_Changing_FiresOnRemove()
        {
            TestChanging((collection) => collection.RemoveAt(0), 1);
        }

        [Fact]
        public void MediaTypeFormatterCollection_Changing_FiresOnSet()
        {
            TestChanging((collection) => collection[0] = new XmlMediaTypeFormatter(), 1);
        }

        private static void TestChanging(Action<MediaTypeFormatterCollection> mutation, int expectedCount)
        {
            // Arrange
            MediaTypeFormatter formatter1 = new XmlMediaTypeFormatter();
            MediaTypeFormatter formatter2 = new JsonMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter1, formatter2 });
            int changeCount = 0;
            collection.Changing += (source, args) => { changeCount++; };

            // Act
            mutation(collection);

            //Assert
            Assert.Equal(expectedCount, changeCount);
        }

        [Fact]
        public void XmlFormatter_SetByCtor()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.XmlFormatter);
        }

        [Fact]
        public void XmlFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.XmlFormatter);
        }

        [Fact]
        public void JsonFormatter_SetByCtor()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.JsonFormatter);
        }

        [Fact]
        public void JsonFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.JsonFormatter);
        }

        [Fact]
        public void FormUrlEncodedFormatter_SetByCtor()
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void FormUrlEncodedFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void Remove_SetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.XmlFormatter);
            Assert.Null(collection.XmlFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void Remove_SetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.JsonFormatter);
            Assert.Null(collection.JsonFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void Insert_SetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.XmlFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

        [Fact]
        public void Insert_SetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.JsonFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

        [Fact]
        public void FindReader_ThrowsOnNullType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/test");
            Assert.ThrowsArgumentNull(() => collection.FindReader(type: null, mediaType: mediaType), "type");
        }

        [Fact]
        public void FindReader_ThrowsOnNullMediaType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.ThrowsArgumentNull(() => collection.FindReader(type: typeof(object), mediaType: null), "mediaType");
        }

        [Fact]
        public void FindReader_ReturnsNullOnNoMatch()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("text/test");

            // Act
            MediaTypeFormatter actualFormatter = collection.FindReader(typeof(object), contentType);

            // Assert
            Assert.Null(actualFormatter);
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void FindReader_ReturnsFormatterOnMatch(Type variationType, object testData, string mediaType)
        {
            // Arrange
            GC.KeepAlive(testData); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string legalMediaType in HttpTestData.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(legalMediaType));
            }

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaType);

            // Act
            MediaTypeFormatter actualFormatter = collection.FindReader(variationType, contentType);

            // Assert
            Assert.Same(formatter, actualFormatter);
        }

        [Fact]
        public void FindWriter_ThrowsOnNullType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/test");
            Assert.ThrowsArgumentNull(() => collection.FindWriter(type: null, mediaType: mediaType), "type");
        }

        [Fact]
        public void FindWriter_ThrowsOnNullMediaType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.ThrowsArgumentNull(() => collection.FindWriter(type: typeof(object), mediaType: null), "mediaType");
        }

        [Fact]
        public void FindWriter_ReturnsNullOnNoMatch()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("text/test");

            // Act
            MediaTypeFormatter actualFormatter = collection.FindWriter(typeof(object), contentType);

            // Assert
            Assert.Null(actualFormatter);
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpTestData), "LegalMediaTypeStrings")]
        public void FindWriter_ReturnsFormatterOnMatch(Type variationType, object testData, string mediaType)
        {
            // Arrange
            GC.KeepAlive(testData); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string legalMediaType in HttpTestData.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaType);

            // Act
            MediaTypeFormatter actualFormatter = collection.FindWriter(variationType, contentType);

            // Assert
            Assert.Same(formatter, actualFormatter);
        }

        [Theory]
        [InlineData(typeof(JObject))]
        [InlineData(typeof(XAttribute))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(byte[]))]
#if !NETFX_CORE
        [InlineData(typeof(XmlElement))]
#endif
        [InlineData(typeof(FormDataCollection))]
        public void IsTypeExcludedFromValidation_ReturnsTrueForExcludedTypes(Type type)
        {
            Assert.True(MediaTypeFormatterCollection.IsTypeExcludedFromValidation(type));
        }

        [Fact]
        public void WritingFormatters_FiltersOutCanWriteAnyTypesFalse()
        {
            // Arrange
            MockMediaTypeFormatter writableFormatter = new MockMediaTypeFormatter();
            MockMediaTypeFormatter readOnlyFormatter = new MockMediaTypeFormatter() { CanWriteAnyTypesReturn = false };
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>() { writableFormatter, readOnlyFormatter };
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);

            // Act
            MediaTypeFormatter[] writableFormatters = collection.WritingFormatters;

            // Assert
            MediaTypeFormatter[] expectedFormatters = new MediaTypeFormatter[] { writableFormatter };
            Assert.Equal(expectedFormatters, writableFormatters);
        }

        [Fact]
        public void WritingFormatters_FiltersOutNull()
        {
            // Arrange
            MockMediaTypeFormatter writableFormatter = new MockMediaTypeFormatter();
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>() { writableFormatter };
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);
            collection.Add(null);

            // Act
            MediaTypeFormatter[] writableFormatters = collection.WritingFormatters;

            // Assert
            MediaTypeFormatter[] expectedFormatters = new MediaTypeFormatter[] { writableFormatter };
            Assert.Equal(expectedFormatters, writableFormatters);
        }

        [Fact]
        public void WritingFormatters_Caches()
        {
            // Arrange
            MockMediaTypeFormatter formatter1 = new MockMediaTypeFormatter();
            MockMediaTypeFormatter formatter2 = new MockMediaTypeFormatter();
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>() { formatter1, formatter2 };
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);

            // Act
            MediaTypeFormatter[] writableFormatters1 = collection.WritingFormatters;
            MediaTypeFormatter[] writableFormatters2 = collection.WritingFormatters;

            // Assert
            MediaTypeFormatter[] expectedFormatters = formatters.ToArray();
            Assert.Equal(expectedFormatters, writableFormatters1);
            Assert.Same(writableFormatters1, writableFormatters2);
        }

        [Fact]
        public void WritingFormatters_Insert_ResetsCache()
        {
            TestWritingFormattersCacheReset((collection) => collection.Insert(0, new MockMediaTypeFormatter()));
        }

        [Fact]
        public void WritingFormatters_RemoveAt_ResetsCache()
        {
            TestWritingFormattersCacheReset((collection) => collection.RemoveAt(0));
        }

        private static void TestWritingFormattersCacheReset(Action<MediaTypeFormatterCollection> mutation)
        {
            // Arrange
            MockMediaTypeFormatter formatter1 = new MockMediaTypeFormatter();
            MockMediaTypeFormatter formatter2 = new MockMediaTypeFormatter();
            List<MediaTypeFormatter> formatters = new List<MediaTypeFormatter>() { formatter1, formatter2 };
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);

            // Act
            mutation(collection);
            MediaTypeFormatter[] expectedFormatters = collection.ToArray();
            MediaTypeFormatter[] writableFormatters = collection.WritingFormatters;

            // Assert
            Assert.Equal(expectedFormatters, writableFormatters);
        }
    }
}
