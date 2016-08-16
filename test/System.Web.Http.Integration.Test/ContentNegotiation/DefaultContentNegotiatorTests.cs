// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ContentNegotiation
{
    public class DefaultContentNegotiatorTests : ContentNegotiationTestBase
    {
        [Fact]
        public async Task Custom_ContentNegotiator_Used_In_Response()
        {
            // Arrange
            Configuration.Formatters.Clear();
            MediaTypeWithQualityHeaderValue requestContentType = new MediaTypeWithQualityHeaderValue("application/xml");
            MediaTypeHeaderValue responseContentType = null;

            Mock<IContentNegotiator> selector = new Mock<IContentNegotiator>();
            selector.Setup(s => s.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(), It.IsAny<IEnumerable<MediaTypeFormatter>>()))
                .Returns(new ContentNegotiationResult(new XmlMediaTypeFormatter(), null));

            Configuration.Services.Replace(typeof(IContentNegotiator), selector.Object);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress);
            request.Headers.Accept.Add(requestContentType);
            HttpResponseMessage response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            responseContentType = response.Content.Headers.ContentType;

            // Assert
            selector.Verify(s => s.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(), It.IsAny<IEnumerable<MediaTypeFormatter>>()), Times.AtLeastOnce());
        }
    }
}
