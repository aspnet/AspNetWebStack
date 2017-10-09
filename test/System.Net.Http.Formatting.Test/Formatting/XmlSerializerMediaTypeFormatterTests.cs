// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class XmlSerializerMediaTypeFormatter : XmlMediaTypeFormatter
    {
        public XmlSerializerMediaTypeFormatter()
        {
            UseXmlSerializer = true;
        }
    }

    public class XmlSerializerMediaTypeFormatterTests : MediaTypeFormatterTestBase<XmlSerializerMediaTypeFormatter>
    {
        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get { return HttpTestData.StandardXmlMediaTypes; }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get { return HttpTestData.StandardEncodings; }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get { return ExpectedSupportedEncodings.ElementAt(0).GetBytes("<SampleType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Number>42</Number></SampleType>"); }
        }

        [Fact]
        public void DefaultMediaType_ReturnsApplicationXml()
        {
            MediaTypeHeaderValue mediaType = XmlSerializerMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/xml", mediaType.MediaType);
        }

        [Fact]
        public void Indent_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlSerializerMediaTypeFormatter(),
                c => c.Indent,
                expectedDefaultValue: false);
        }

        [Fact]
        public async Task ReadDeeplyNestedObjectWorks()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter()
            {
#if !NETFX_CORE // We don't support MaxDepth in the portable library
                MaxDepth = 5001
#endif
            };

            StringContent content = new StringContent(GetDeeplyNestedObject(5000));

            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            Assert.IsType<Nest>(await formatter.ReadFromStreamAsync(typeof(Nest), await content.ReadAsStreamAsync(), content, null));
        }

        [Fact]
        public void UseXmlSerializer_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new XmlSerializerMediaTypeFormatter(),
                c => c.UseXmlSerializer,
                expectedDefaultValue: true);
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        public async Task UseXmlFormatterWithNull(Type type)
        {
            XmlMediaTypeFormatter xmlFormatter = new XmlSerializerMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await Assert.Task.SucceedsAsync(xmlFormatter.WriteToStreamAsync(type, null, memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("nil=\"true\""),
                "Null value should be serialized as nil.");
            Assert.True(serializedString.ToLower().Contains("arrayofstring"),
                "It should be serialized out as an array of string.");
        }

        [Fact]
        public async Task UseXmlSerializer_True()
        {
            XmlSerializerMediaTypeFormatter xmlFormatter = new XmlSerializerMediaTypeFormatter();
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await Assert.Task.SucceedsAsync(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.False(serializedString.Contains("DataContractSampleType"),
                "SampleType should not be serialized with data contract name DataContractSampleType because UseXmlSerializer is set to true.");
            Assert.False(serializedString.Contains("version=\"1.0\" encoding=\"utf-8\""),
              "Using XmlSerializer should not emit the xml declaration by default.");
            Assert.False(serializedString.Contains("\r\n"), "Using default XmlSerializer should emit data without indentation.");
        }

        [Fact]
        public async Task UseXmlSerializer_True_Indent()
        {
            XmlSerializerMediaTypeFormatter xmlFormatter = new XmlSerializerMediaTypeFormatter { Indent = true };
            MemoryStream memoryStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            await Assert.Task.SucceedsAsync(xmlFormatter.WriteToStreamAsync(typeof(SampleType), new SampleType(), memoryStream, content, transportContext: null));
            memoryStream.Position = 0;
            string serializedString = new StreamReader(memoryStream).ReadToEnd();
            Assert.True(serializedString.Contains("\r\n"), "Using default XmlSerializer with Indent set to true should emit data with indentation.");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        public void CanReadType_ReturnsSameResultAsXmlSerializerConstructor(Type variationType, object testData)
        {
            TestXmlSerializerMediaTypeFormatter formatter = new TestXmlSerializerMediaTypeFormatter();

            bool isSerializable = IsSerializableWithXmlSerializer(variationType, testData);
            bool canSupport = formatter.CanReadTypeCaller(variationType);
            if (isSerializable != canSupport)
            {
                Assert.Equal(isSerializable, canSupport);
            }

            // Ask a 2nd time to probe whether the cached result is treated the same
            canSupport = formatter.CanReadTypeCaller(variationType);
            Assert.Equal(isSerializable, canSupport);

        }

        [Fact]
        public void SetSerializer_ThrowsWithNullType()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlSerializer); }, "type");
        }

        [Fact]
        public void SetSerializer_ThrowsWithNullSerializer()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer1_ThrowsWithNullSerializer()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer2_ThrowsWithNullType()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            XmlObjectSerializer xmlObjectSerializer = new DataContractSerializer(typeof(string));
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(null, xmlObjectSerializer); }, "type");
        }

        [Fact]
        public void SetSerializer2_ThrowsWithNullSerializer()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer(typeof(string), (XmlObjectSerializer)null); }, "serializer");
        }

        [Fact]
        public void SetSerializer3_ThrowsWithNullSerializer()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.SetSerializer<string>((XmlSerializer)null); }, "serializer");
        }

        [Fact]
        public void RemoveSerializer_ThrowsWithNullType()
        {
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.RemoveSerializer(null); }, "type");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection", RoundTripDataVariations)]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsyncUsingXmlSerializer(Type variationType, object testData)
        {
            // Guard
            bool canSerialize = IsSerializableWithXmlSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                // Arrange
                TestXmlSerializerMediaTypeFormatter formatter = new TestXmlSerializerMediaTypeFormatter();
                formatter.SetSerializer(variationType, new XmlSerializer(variationType));

                // Arrange & Act & Assert
                object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
                Assert.Equal(testData, readObj);
            }
        }

        [Theory]
        [TestDataSet(typeof(XmlMediaTypeFormatterTests), "BunchOfTypedObjectsTestDataCollection", RoundTripDataVariations)]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsyncUsingXmlSerializer_ExtraTypes(Type variationType, object testData)
        {
            // Guard
            bool canSerialize = IsSerializableWithXmlSerializer(variationType, testData) && Assert.Http.CanRoundTrip(variationType);
            if (canSerialize)
            {
                // Arrange
                TestXmlSerializerMediaTypeFormatter formatter = new TestXmlSerializerMediaTypeFormatter();
                formatter.SetSerializer(variationType, new XmlSerializer(variationType, new Type[] { typeof(DBNull), }));

                // Arrange & Act & Assert
                object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
                Assert.Equal(testData, readObj);
            }
        }

        // Test alternate null value; this serializer attempts to cast DBNull to variationType so typeof(string) variation fails
        [Fact]
        public async Task ReadFromStreamAsync_RoundTripsWriteToStreamAsyncUsingXmlSerializer_DBNull()
        {
            // Arrange
            TestXmlSerializerMediaTypeFormatter formatter = new TestXmlSerializerMediaTypeFormatter();
            Type variationType = typeof(DBNull);
            formatter.SetSerializer(variationType, new XmlSerializer(variationType));
            object testData = DBNull.Value;

            // Arrange & Act & Assert
            object readObj = await ReadFromStreamAsync_RoundTripsWriteToStreamAsync_Helper(formatter, variationType, testData);
            Assert.Equal(testData, readObj);
        }

        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            string formattedContent = "<string>" + content + "</string>";
#if NETFX_CORE
            if (String.Equals("utf-16", encoding, StringComparison.OrdinalIgnoreCase))
            {
                // We need to supply the xml declaration when compiled in portable library for non utf-8 content
                formattedContent = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" + formattedContent;
            }
#endif
            string mediaType = string.Format("application/xml; charset={0}", encoding);

            // Act & assert
            return ReadFromStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            if (!isDefaultEncoding)
            {
                // XmlDictionaryReader/Writer only supports utf-8 and 16
                return TaskHelpers.Completed();
            }

            // Arrange
            XmlSerializerMediaTypeFormatter formatter = new XmlSerializerMediaTypeFormatter();
            string formattedContent = "<string>" + content + "</string>";
            string mediaType = string.Format("application/xml; charset={0}", encoding);

            // Act & assert
            return WriteToStreamAsync_UsesCorrectCharacterEncodingHelper(formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        static string GetDeeplyNestedObject(int depth)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Insert(0, "<A>");
                sb.Append("</A>");
            }
            sb.Insert(0, "<Nest xmlns=\"http://example.com\">");
            sb.Append("</Nest>");
            sb.Insert(0, "<?xml version=\"1.0\"?>");

            return sb.ToString();
        }

        public class TestXmlSerializerMediaTypeFormatter : XmlSerializerMediaTypeFormatter
        {
            public bool CanReadTypeCaller(Type type)
            {
                return CanReadType(type);
            }

            public bool CanWriteTypeCaller(Type type)
            {
                return CanWriteType(type);
            }
        }

        [XmlRoot("Nest", Namespace = "http://example.com")]
        public class Nest
        {
            public Nest A { get; set; }
        }

        private bool IsSerializableWithXmlSerializer(Type type, object obj)
        {
            if (Assert.Http.IsKnownUnserializable(type, obj))
            {
                return false;
            }

            try
            {
                new XmlSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new XmlSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool IsSerializableWithDataContractSerializer(Type type, object obj)
        {
            if (Assert.Http.IsKnownUnserializable(type, obj))
            {
                return false;
            }

            try
            {
                new DataContractSerializer(type);
                if (obj != null && obj.GetType() != type)
                {
                    new DataContractSerializer(obj.GetType());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
