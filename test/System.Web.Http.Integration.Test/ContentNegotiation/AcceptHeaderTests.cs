// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.ContentNegotiation
{
    public class AcceptHeaderTests : ContentNegotiationTestBase
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("application/xml")]
        public async Task Response_Contains_ContentType(string contentType)
        {
            // Arrange
            MediaTypeWithQualityHeaderValue requestContentType = new MediaTypeWithQualityHeaderValue(contentType);
            MediaTypeHeaderValue responseContentType = null;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress);
            request.Headers.Accept.Add(requestContentType);
            HttpResponseMessage response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            responseContentType = response.Content.Headers.ContentType;

            // Assert
            Assert.Equal(requestContentType.MediaType, responseContentType.MediaType);
        }
    }
}
