// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class DataContractJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        public DataContractJsonMediaTypeFormatter()
        {
            UseDataContractJsonSerializer = true;
        }
    }

    public class DataContractJsonMediaTypeFormatterTests : MediaTypeFormatterTestBase<DataContractJsonMediaTypeFormatter>
    {
        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return HttpTestData.StandardJsonMediaTypes; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("{\"Number\":42}"); }
        }

        [Fact]
        public void DefaultMediaType_ReturnsApplicationJson()
        {
            MediaTypeHeaderValue mediaType = DataContractJsonMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/json", mediaType.MediaType);
        }

        [Fact]
        public void Indent_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlMediaTypeFormatter(),
                c => c.Indent,
                expectedDefaultValue: false);
        }

        [Fact]
        public void MaxDepth_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new DataContractJsonMediaTypeFormatter(),
                c => c.MaxDepth,
                expectedDefaultValue: 256,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 256);
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        public void CanReadType_ReturnsExpectedValues(Type variationType, object testData)
        {
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

            bool isSerializable = IsTypeSerializableWithJsonSerializer(variationType, testData);
            bool canSupport = formatter.CanReadTypeProxy(variationType);

            // If we don't agree, we assert only if the DCJ serializer says it cannot support something we think it should
            Assert.False(isSerializable != canSupport && isSerializable, String.Format("CanReadType returned wrong value for '{0}'.", variationType));

            // Ask a 2nd time to probe whether the cached result is treated the same
            canSupport = formatter.CanReadTypeProxy(variationType);
            Assert.False(isSerializable != canSupport && isSerializable, String.Format("2nd CanReadType returned wrong value for '{0}'.", variationType));
        }

        [Fact]
        public void CanReadType_ReturnsFalse_ForInvalidDataContracts()
        {
            JsonMediaTypeFormatter formatter = new DataContractJsonMediaTypeFormatter();
            Assert.False(formatter.CanReadType(typeof(InvalidDataContract)));
        }

        [Fact]
        public void CanWriteType_ReturnsFalse_ForInvalidDataContracts()
        {
            JsonMediaTypeFormatter formatter = new DataContractJsonMediaTypeFormatter();
            Assert.False(formatter.CanWriteType(typeof(InvalidDataContract)));
        }

        public class InvalidDataContract
        {
            // removing the default ctor makes this invalid
            public InvalidDataContract(string s)
            {
            }
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        public async Task UseJsonFormatterWithNull(Type type)
        {
            JsonMediaTypeFormatter xmlFormatter = new DataContractJsonMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await Assert.Task.SucceedsAsync(xmlFormatter.WriteToStreamAsync(type, null, memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("null"), "Using Json formatter to serialize null should emit 'null'.");
        }

        [Theory]
        [TestDataSet(typeof(JsonMediaTypeFormatterTests), "ValueAndRefTypeTestDataCollectionExceptULong", RoundTripDataVariations)]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsync(Type variationType, object testData)
        {
            // Guard
            bool canSerialize = IsTypeSerializableWithJsonSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                // Arrange
                TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();

                // Arrange & Act & Assert
                object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [TestDataSet(typeof(XmlMediaTypeFormatterTests), "BunchOfTypedObjectsTestDataCollection", RoundTripDataVariations)]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsync_KnownTypes(Type variationType, object testData)
        {
            // Guard
            bool canSerialize = IsTypeSerializableWithJsonSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                // Arrange
                TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter { AddDBNullKnownType = true, };

                // Arrange & Act & Assert
                object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
                Assert.Equal(testData, readObj);
            }
        }

        // Test alternate null value
        [Fact]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsync_DBNull()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter();
            Type variationType = typeof(DBNull);
            object testData = DBNull.Value;

            // Arrange & Act & Assert
            object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);

            // DBNull.Value round-trips as either Object or DBNull because serialization includes its type
            Assert.Equal(testData, readObj);
        }

        [Fact]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsync_DBNullAsEmptyString()
        {
            // Arrange
            TestJsonMediaTypeFormatter formatter = new TestJsonMediaTypeFormatter { AddDBNullKnownType = true, };
            Type variationType = typeof(string);
            object testData = DBNull.Value;

            // Arrange & Act & Assert
            object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);

            // Lower levels convert DBNull.Value to empty string on read
            Assert.Equal(String.Empty, readObj);
        }

        [Fact]
        public async Task UseDataContractJsonSerializer_Default()
        {
            DataContractJsonMediaTypeFormatter jsonFormatter = new DataContractJsonMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await Assert.Task.SucceedsAsync(jsonFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.False(serializedString.Contains("\r\n"), "Using DCJS should emit data without indentation by default.");
        }

        [Fact]
        public void UseDataContractJsonSerializer_True_Indent_Throws()
        {
            DataContractJsonMediaTypeFormatter jsonFormatter = new DataContractJsonMediaTypeFormatter { Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            Assert.Throws<NotSupportedException>(
                () => jsonFormatter.WriteToStreamAsync(typeof(SampleType),
                    new SampleType(),
                    memoryStream, content, transportContext: null));
        }

        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            DataContractJsonMediaTypeFormatter formatter = new DataContractJsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return ReadContentUsingCorrectCharacterEncodingHelperAsync(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // DataContractJsonSerializer does not honor the value of byteOrderMark in the UnicodeEncoding ctor.
            // It doesn't include the BOM when byteOrderMark is set to true.
            if (!isDefaultEncoding || encoding != "utf-8")
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            DataContractJsonMediaTypeFormatter formatter = new DataContractJsonMediaTypeFormatter();
            string formattedContent = "\"" + content + "\"";
            string mediaType = string.Format("application/json; charset={0}", encoding);

            // Act & assert
            return WriteContentUsingCorrectCharacterEncodingHelperAsync(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        public class TestJsonMediaTypeFormatter : DataContractJsonMediaTypeFormatter
        {
            public bool AddDBNullKnownType { get; set; }

            public bool CanReadTypeProxy(Type type)
            {
                return CanReadType(type);
            }

            public bool CanWriteTypeProxy(Type type)
            {
                return CanWriteType(type);
            }

            public override DataContractJsonSerializer CreateDataContractSerializer(Type type)
            {
                if (AddDBNullKnownType)
                {
                    return new DataContractJsonSerializer(type, new Type[] { typeof(DBNull), });
                }
                else
                {
                    return base.CreateDataContractSerializer(type);
                }
            }
        }

        private bool IsTypeSerializableWithJsonSerializer(Type type, object obj)
        {
            try
            {
                new DataContractJsonSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new DataContractJsonSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return !Assert.Http.IsKnownUnserializable(type, obj, (t) => typeof(INotJsonSerializable).IsAssignableFrom(t));
        }
    }
}
