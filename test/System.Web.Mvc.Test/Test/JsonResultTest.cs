// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class JsonResultTest
    {
        private static readonly object _jsonData = new object[] { 1, 2, "three", "four" };
        private static readonly string _jsonSerializedData = "[1,2,\"three\",\"four\"]";

        [Fact]
        public void PropertyDefaults()
        {
            // Act
            JsonResult result = new JsonResult();

            // Assert
            Assert.Null(result.Data);
            Assert.Null(result.ContentEncoding);
            Assert.Null(result.ContentType);
            Assert.Null(result.MaxJsonLength);
            Assert.Null(result.RecursionLimit);
            Assert.Equal(JsonRequestBehavior.DenyGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void EmptyContentTypeRendersDefault()
        {
            // Arrange
            object data = _jsonData;
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/json").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(_jsonSerializedData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                ContentType = String.Empty,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            object data = _jsonData;
            string contentType = "Some content type.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(_jsonSerializedData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultWithNullContextThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new JsonResult().ExecuteResult(null /* context */); }, "context");
        }

        [Fact]
        public void NullContentIsNotOutput()
        {
            // Arrange
            string contentType = "Some content type.";
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();

            // Though most other tests run fine with SetupSet(c => c.HttpContext.Response.ContentType = ...), this
            // one does not. Explicitly ensure Response property is not null.
            var response = new Mock<HttpResponseBase>(MockBehavior.Strict);
            response.SetupSet(r => r.ContentType = contentType).Verifiable();
            response.SetupSet(r => r.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.SetupGet(c => c.HttpContext.Response).Returns(response.Object);

            JsonResult result = new JsonResult
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
            response.Verify();
        }

        [Fact]
        public void NullContentEncodingIsNotOutput()
        {
            // Arrange
            object data = _jsonData;
            string contentType = "Some content type.";

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = contentType).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(_jsonSerializedData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                ContentType = contentType,
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void NullContentTypeRendersDefault()
        {
            // Arrange
            object data = _jsonData;
            Encoding contentEncoding = Encoding.UTF8;

            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/json").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentEncoding = contentEncoding).Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(_jsonSerializedData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                ContentEncoding = contentEncoding
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void NullMaxJsonLengthDefaultIsUsed()
        {
            // Arrange
            string data = new String('1', 2100000);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST");

            // Though most other tests run fine with SetupSet(c => c.HttpContext.Response.ContentType = ...), this
            // one does not. Explicitly ensure Response property is not null.
            var response = new Mock<HttpResponseBase>(MockBehavior.Strict);
            response.SetupSet(r => r.ContentType = "application/json");
            mockControllerContext.SetupGet(c => c.HttpContext.Response).Returns(response.Object);

            JsonResult result = new JsonResult
            {
                Data = data
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => result.ExecuteResult(mockControllerContext.Object),
                "Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property.");
        }

        [Fact]
        public void MaxJsonLengthIsPassedToSerializer()
        {
            // Arrange
            string data = new String('1', 2100000);
            string jsonData = "\"" + data + "\"";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/json").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(jsonData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                MaxJsonLength = 2200000
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void RecursionLimitIsPassedToSerilizer()
        {
            // Arrange
            Tuple<string, Tuple<string, Tuple<string, string>>> data =
                new Tuple<string, Tuple<string, Tuple<string, string>>>("key1",
                                                                        new Tuple<string, Tuple<string, string>>("key2",
                                                                                                                 new Tuple<string, string>("key3", "value")
                                                                            )
                    );
            string jsonData = "{\"Item1\":\"key1\",\"Item2\":{\"Item1\":\"key2\",\"Item2\":{\"Item1\":\"key3\",\"Item2\":\"value\"}}}";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/json").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(jsonData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data,
                RecursionLimit = 2
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => result.ExecuteResult(mockControllerContext.Object),
                "RecursionLimit exceeded.");
        }

        [Fact]
        public void NullRecursionLimitDefaultIsUsed()
        {
            // Arrange
            Tuple<string, Tuple<string, Tuple<string, string>>> data =
                new Tuple<string, Tuple<string, Tuple<string, string>>>("key1",
                                                                        new Tuple<string, Tuple<string, string>>("key2",
                                                                                                                 new Tuple<string, string>("key3", "value")
                                                                            )
                    );
            string jsonData = "{\"Item1\":\"key1\",\"Item2\":{\"Item1\":\"key2\",\"Item2\":{\"Item1\":\"key3\",\"Item2\":\"value\"}}}";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("POST").Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/json").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.Write(jsonData)).Verifiable();

            JsonResult result = new JsonResult
            {
                Data = data
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void GetRequestBlocked()
        {
            // Arrange expectations
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupGet(c => c.HttpContext.Request.HttpMethod).Returns("GET").Verifiable();

            JsonResult result = new JsonResult();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => result.ExecuteResult(mockControllerContext.Object),
                "This request has been blocked because sensitive information could be disclosed to third party web sites when this is used in a GET request. To allow GET requests, set JsonRequestBehavior to AllowGet.");

            mockControllerContext.Verify();
        }
    }
}
