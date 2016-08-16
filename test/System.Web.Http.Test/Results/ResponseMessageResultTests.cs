// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Results
{
    public class ResponseMessageResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenResponseIsNull()
        {
            // Arrange, Act & Assert
            Assert.ThrowsArgumentNull(() => { new ResponseMessageResult(null); }, "response");
        }

        [Fact]
        public void Response_Returns_InstanceProvided()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                ResponseMessageResult result = new ResponseMessageResult(expectedResponse);

                // Act
                HttpResponseMessage response = result.Response;

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public async Task ExecuteAsync_Returns_InstanceProvided()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                IHttpActionResult result = new ResponseMessageResult(expectedResponse);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);

                using (HttpResponseMessage response = await task)
                {
                    Assert.Same(expectedResponse, response);
                }
            }
        }

        [Fact]
        public void ApiControllerResponseMessage_CreatesCorrectResponseMessageResult()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                ApiController controller = CreateController();

                // Act
                ResponseMessageResult result = controller.ResponseMessage(expectedResponse);

                // Assert
                Assert.NotNull(result);
                Assert.Same(expectedResponse, result.Response);
            }
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private class FakeController : ApiController
        {
        }
    }
}
