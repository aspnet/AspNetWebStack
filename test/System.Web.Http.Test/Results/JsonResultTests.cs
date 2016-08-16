// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json;

namespace System.Web.Http.Results
{
    public class JsonResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenSerializerSettingsIsNull()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = null;
            Encoding encoding = CreateDummyEncoding();

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(content, serializerSettings, encoding, request);
                }, "serializerSettings");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenEncodingIsNull()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = null;

            using (HttpRequestMessage request = CreateRequest())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() =>
                {
                    CreateProductUnderTest(content, serializerSettings, encoding, request);
                }, "encoding");
            }
        }

        [Fact]
        public void Constructor_Throws_WhenRequestIsNull()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();
            HttpRequestMessage request = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(content, serializerSettings, encoding, request);
            }, "request");
        }

        [Fact]
        public void Content_Returns_InstanceProvided()
        {
            // Arrange
            object expectedContent = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();

            using (HttpRequestMessage request = CreateRequest())
            {
                JsonResult<object> result = CreateProductUnderTest(expectedContent, serializerSettings, encoding,
                    request);

                // Act
                object content = result.Content;

                // Assert
                Assert.Same(expectedContent, content);
            }
        }

        [Fact]
        public void SerializerSettings_Returns_InstanceProvided()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings expectedSerializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();

            using (HttpRequestMessage request = CreateRequest())
            {
                JsonResult<object> result = CreateProductUnderTest(content, expectedSerializerSettings, encoding,
                    request);

                // Act
                JsonSerializerSettings serializerSettings = result.SerializerSettings;

                // Assert
                Assert.Same(expectedSerializerSettings, serializerSettings);
            }
        }

        [Fact]
        public void Encoding_Returns_InstanceProvided()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding expectedEncoding = CreateDummyEncoding();

            using (HttpRequestMessage request = CreateRequest())
            {
                JsonResult<object> result = CreateProductUnderTest(content, serializerSettings, expectedEncoding,
                    request);

                // Act
                Encoding encoding = result.Encoding;

                // Assert
                Assert.Same(expectedEncoding, encoding);
            }
        }

        [Fact]
        public void Request_Returns_InstanceProvided()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                JsonResult<object> result = CreateProductUnderTest(content, serializerSettings, encoding,
                    expectedRequest);

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Returns_CorrectResponse()
        {
            // Arrange
            string[] content = new string[] { "Content" };
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateEncoding();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult result = CreateProductUnderTest(content, serializerSettings, encoding,
                    expectedRequest);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    Assert.NotNull(response.Content.Headers.ContentType);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                    Assert.Equal(encoding.WebName, response.Content.Headers.ContentType.CharSet);
                    Assert.IsType<ByteArrayContent>(response.Content);
                    StringBuilder expectedBuilder = new StringBuilder();
                    using (TextWriter textWriter = new StringWriter(expectedBuilder, CultureInfo.InvariantCulture))
                    {
                        JsonSerializer serializer = JsonSerializer.Create(serializerSettings);

                        using (JsonWriter jsonWriter = new JsonTextWriter(textWriter))
                        {
                            serializer.Serialize(jsonWriter, content);
                            jsonWriter.Flush();
                        }
                    }
                    byte[] expectedContents = encoding.GetBytes(expectedBuilder.ToString());
                    byte[] contents = await response.Content.ReadAsByteArrayAsync();
                    Assert.Equal(expectedContents, contents);
                    Assert.True(response.Content.Headers.ContentLength.HasValue);
                    Assert.Equal((long)expectedContents.Length, response.Content.Headers.ContentLength.Value);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Constructor_ForApiController_Throws_WhenControllerIsNull()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();
            ApiController controller = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                CreateProductUnderTest(content, serializerSettings, encoding, controller);
            }, "controller");
        }

        [Fact]
        public async Task ExecuteAsync_ForApiController_ReturnsCorrectResponse()
        {
            // Arrange
            string[] content = new string[] { "Content" };
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateEncoding();
            ApiController controller = CreateController();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                IHttpActionResult result = CreateProductUnderTest(content, serializerSettings, encoding, controller);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(response.Content);
                    Assert.NotNull(response.Content.Headers.ContentType);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                    Assert.Equal(encoding.WebName, response.Content.Headers.ContentType.CharSet);
                    Assert.IsType<ByteArrayContent>(response.Content);
                    StringBuilder expectedBuilder = new StringBuilder();
                    using (TextWriter textWriter = new StringWriter(expectedBuilder, CultureInfo.InvariantCulture))
                    {
                        JsonSerializer serializer = JsonSerializer.Create(serializerSettings);

                        using (JsonWriter jsonWriter = new JsonTextWriter(textWriter))
                        {
                            serializer.Serialize(jsonWriter, content);
                            jsonWriter.Flush();
                        }
                    }
                    byte[] expectedContents = encoding.GetBytes(expectedBuilder.ToString());
                    byte[] contents = await response.Content.ReadAsByteArrayAsync();
                    Assert.Equal(expectedContents, contents);
                    Assert.True(response.Content.Headers.ContentLength.HasValue);
                    Assert.Equal((long)expectedContents.Length, response.Content.Headers.ContentLength.Value);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesLazily()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();
            ApiController controller = CreateController();
            JsonResult<object> result = CreateProductUnderTest(content, serializerSettings, encoding, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;

                // Act
                HttpRequestMessage request = result.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void Request_ForApiController_EvaluatesOnce()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();
            ApiController controller = CreateController();
            JsonResult<object> result = CreateProductUnderTest(content, serializerSettings, encoding, controller);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                HttpRequestMessage ignore = result.Request;

                using (HttpRequestMessage otherRequest = CreateRequest())
                {
                    controller.Request = otherRequest;

                    // Act
                    HttpRequestMessage request = result.Request;

                    // Assert
                    Assert.Same(expectedRequest, request);
                }
            }
        }

        [Fact]
        public void Request_ForApiController_Throws_WhenControllerRequestIsNull()
        {
            // Arrange
            object content = CreateContent();
            JsonSerializerSettings serializerSettings = CreateSerializerSettings();
            Encoding encoding = CreateDummyEncoding();
            ApiController controller = CreateController();
            Assert.Null(controller.Request);
            JsonResult<object> result = CreateProductUnderTest(content, serializerSettings, encoding, controller);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                { HttpRequestMessage ignore = result.Request; }, "ApiController.Request must not be null.");
        }

        [Fact]
        public void ApiControllerJson_WithObjectJsonSerializerSettingsAndEncoding_CreatesCorrectResult()
        {
            // Arrange
            object expectedContent = CreateContent();
            JsonSerializerSettings expectedSerializerSettings = CreateSerializerSettings();
            Encoding expectedEncoding = CreateDummyEncoding();
            ApiController controller = CreateController();

            // Act
            JsonResult<object> result = controller.Json(expectedContent, expectedSerializerSettings, expectedEncoding);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedContent, result.Content);
            Assert.Same(expectedSerializerSettings, result.SerializerSettings);
            Assert.Same(expectedEncoding, result.Encoding);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerJson_WithObjectAndJsonSerializerSettings_CreatesCorrectResult()
        {
            // Arrange
            object expectedContent = CreateContent();
            JsonSerializerSettings expectedSerializerSettings = CreateSerializerSettings();
            ApiController controller = CreateController();

            // Act
            JsonResult<object> result = controller.Json(expectedContent, expectedSerializerSettings);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedContent, result.Content);
            Assert.Same(expectedSerializerSettings, result.SerializerSettings);
            Encoding expectedEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);
            Assert.Equal(expectedEncoding, result.Encoding);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        [Fact]
        public void ApiControllerJson_WithObject_CreatesCorrectResult()
        {
            // Arrange
            object expectedContent = CreateContent();
            ApiController controller = CreateController();

            // Act
            JsonResult<object> result = controller.Json(expectedContent);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedContent, result.Content);
            Assert.NotNull(result.SerializerSettings);
            Encoding expectedEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);
            Assert.Equal(expectedEncoding, result.Encoding);

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                controller.Request = expectedRequest;
                Assert.Same(expectedRequest, result.Request);
            }
        }

        private static object CreateContent()
        {
            return new object();
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static Encoding CreateDummyEncoding()
        {
            return new Mock<Encoding>(MockBehavior.Strict).Object;
        }

        private static Encoding CreateEncoding()
        {
            return new ASCIIEncoding();
        }

        private static JsonResult<T> CreateProductUnderTest<T>(T content, JsonSerializerSettings serializerSettings,
            Encoding encoding, HttpRequestMessage request)
        {
            return new JsonResult<T>(content, serializerSettings, encoding, request);
        }

        private static JsonResult<T> CreateProductUnderTest<T>(T content, JsonSerializerSettings serializerSettings,
            Encoding encoding, ApiController controller)
        {
            return new JsonResult<T>(content, serializerSettings, encoding, controller);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static JsonSerializerSettings CreateSerializerSettings()
        {
            return new JsonSerializerSettings();
        }

        private class FakeController : ApiController
        {
        }
    }
}
