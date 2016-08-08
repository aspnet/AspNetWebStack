// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpRequestMessageExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(HttpRequestMessageExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void CreateResponseThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageExtensions.CreateResponse(null), "request");
        }

        [Fact]
        public void CreateResponseWithStatusThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageExtensions.CreateResponse(null, HttpStatusCode.OK), "request");
        }

        [Fact]
        public void CreateResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            HttpResponseMessage response = request.CreateResponse();

            // Assert
            Assert.Same(request, response.RequestMessage);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void CreateResponseWithStatus()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.NotImplemented);

            // Assert
            Assert.Same(request, response.RequestMessage);
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }
    }
}
