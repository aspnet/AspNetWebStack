// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.WebHost
{
    public class BatchingTest
    {
        // Regression test for Codeplex-2103
        //
        // When batching is used in web host, we need to wrap the HttpRequestMessages in an HttpContextBase
        // adapter and pass system to system.web. In 2103 there were bugs in this wrapper, and we weren't 
        // following the contract with respect to %-encoding.
        [Fact]
        public async Task WebHost_Batching_WithSpecialCharactersInUrl()
        {
            // Arrange
            var handler = new SuccessMessageHandler();

            var routeCollection = new HostedHttpRouteCollection(new RouteCollection(), "/");
            routeCollection.Add("default", routeCollection.CreateRoute(
                "values/  space",
                defaults: null,
                constraints: null,
                dataTokens: null,
                handler: handler));

            var configuration = new HttpConfiguration(routeCollection);

            var server = new HttpServer(configuration);

            var batchHandler = new DefaultHttpBatchHandler(server);
            var request = new HttpRequestMessage
            {
                Content = new MultipartContent("mixed")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, "http://contoso.com/values/  space"))
                }
            };

            // Arrange
            var response = await batchHandler.ProcessBatchAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(handler.IsCalled);
        }

        private class SuccessMessageHandler : HttpMessageHandler
        {
            public bool IsCalled { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                IsCalled = true;
                return Task.FromResult(request.CreateResponse(HttpStatusCode.OK));
            }
        }
    }
}
