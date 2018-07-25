// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class DefaultHttpBatchHandlerTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());

            Assert.Equal(BatchExecutionOrder.Sequential, batchHandler.ExecutionOrder);
            Assert.NotNull(batchHandler.Invoker);
            Assert.Contains("multipart/mixed", batchHandler.SupportedContentTypes);
        }

        [Fact]
        public void Constructor_Throws_IfHttpServerIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new DefaultHttpBatchHandler(null),
                "httpServer");
        }

        [Fact]
        public Task CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            return Assert.ThrowsArgumentNullAsync(
                () => batchHandler.CreateResponseMessageAsync(null, new HttpRequestMessage(), CancellationToken.None),
                "responses");
        }

        [Fact]
        public Task CreateResponseMessageAsync_Throws_IfRequestIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            return Assert.ThrowsArgumentNullAsync(
                () => batchHandler.CreateResponseMessageAsync(new HttpResponseMessage[0], null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_ReturnsMultipartContent()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpResponseMessage[] responses = new HttpResponseMessage[]
            {
                new HttpResponseMessage(HttpStatusCode.OK),
                new HttpResponseMessage(HttpStatusCode.BadRequest)
            };

            HttpResponseMessage response = await batchHandler.CreateResponseMessageAsync(responses, new HttpRequestMessage(), CancellationToken.None);

            MultipartContent content = Assert.IsType<MultipartContent>(response.Content);
            List<HttpResponseMessage> nestedResponses = new List<HttpResponseMessage>();
            foreach (var part in content)
            {
                nestedResponses.Add(await part.ReadAsHttpResponseMessageAsync());
            }

            Assert.Equal(2, nestedResponses.Count);
            Assert.Equal(HttpStatusCode.OK, nestedResponses[0].StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, nestedResponses[1].StatusCode);
        }

        [Fact]
        public Task ProcessBatchAsync_Throws_IfRequestIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            return Assert.ThrowsArgumentNullAsync(
                () => batchHandler.ProcessBatchAsync(null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task ProcessBatchAsync_CallsRegisterForDispose()
        {
            List<IDisposable> expectedResourcesForDisposal = new List<IDisposable>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                var tmpContent = new StringContent(String.Empty);
                request.RegisterForDispose(tmpContent);
                expectedResourcesForDisposal.Add(tmpContent);
                return new HttpResponseMessage { Content = new StringContent(request.RequestUri.AbsoluteUri) };
            });
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage
            {
                Content = new MultipartContent("mixed")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, "http://example.org/"))
                }
            };

            var response = await batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None);
            var resourcesForDisposal = batchRequest.GetResourcesForDisposal();

            foreach (var expectedResource in expectedResourcesForDisposal)
            {
                Assert.Contains(expectedResource, resourcesForDisposal);
            }
        }

        [Fact]
        public void ExecutionOrder_ThrowsWhenTheValueIsInvalid()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());

            Assert.ThrowsInvalidEnumArgument(
                () => batchHandler.ExecutionOrder = (BatchExecutionOrder)20,
                "value",
                20,
                typeof(BatchExecutionOrder));
        }

        [Fact]
        public Task ExecuteRequestMessagesAsync_Throws_IfRequestsAreNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            return Assert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteRequestMessagesAsync(null, CancellationToken.None),
                "requests");
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_CallsInvokerForEachRequest()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                return new HttpResponseMessage { Content = new StringContent(request.RequestUri.AbsoluteUri) };
            });
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(server);
            HttpRequestMessage[] requests = new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com/"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.org/")
            };

            var responses = await batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None);

            Assert.Equal(2, responses.Count);
            Assert.Equal("http://example.com/", await responses[0].Content.ReadAsStringAsync());
            Assert.Equal("http://example.org/", await responses[1].Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_DisposesResponseInCaseOfException()
        {
            List<DisposableResponseMessage> responses = new List<DisposableResponseMessage>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                if (request.Method == HttpMethod.Put)
                {
                    throw new InvalidOperationException();
                }
                var response = new DisposableResponseMessage();
                responses.Add(response);
                return response;
            });
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(server);
            HttpRequestMessage[] requests = new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com/"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.com/"),
                new HttpRequestMessage(HttpMethod.Put, "http://example.com/")
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None));

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_NonSequentialExecution()
        {
            List<HttpRequestMessage> completedRequests = new List<HttpRequestMessage>();

            MockHttpServer server = new MockHttpServer(async request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    await Task.Delay(2000);
                }
                completedRequests.Add(request);
                return new HttpResponseMessage();
            });
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(server)
            {
                ExecutionOrder = BatchExecutionOrder.NonSequential
            };
            HttpRequestMessage[] requests = new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com/"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.com/")
            };

            await batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None);

            Assert.Equal(2, completedRequests.Count);
            Assert.Equal(HttpMethod.Post, completedRequests[0].Method);
            Assert.Equal(HttpMethod.Get, completedRequests[1].Method);
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_SequentialExecutionCallInvokerForEachRequest()
        {
            List<HttpRequestMessage> completedRequests = new List<HttpRequestMessage>();

            MockHttpServer server = new MockHttpServer(async request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    await Task.Delay(2000);
                }
                completedRequests.Add(request);
                return new HttpResponseMessage();
            });
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(server)
            {
                ExecutionOrder = BatchExecutionOrder.Sequential
            };
            HttpRequestMessage[] requests = new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com/"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.com/")
            };

            await batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None);

            Assert.Equal(2, completedRequests.Count);
            Assert.Equal(HttpMethod.Get, completedRequests[0].Method);
            Assert.Equal(HttpMethod.Post, completedRequests[1].Method);
        }

        [Fact]
        public Task ParseBatchRequestsAsync_Throws_IfRequestIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            return Assert.ThrowsArgumentNullAsync(
                () => batchHandler.ParseBatchRequestsAsync(null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Returns_RequestsFromMultipartContent()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new MultipartContent("mixed")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values"))
                }
            };

            IList<HttpRequestMessage> requests = await batchHandler.ParseBatchRequestsAsync(request, CancellationToken.None);

            Assert.Equal(2, requests.Count);
            Assert.Equal(HttpMethod.Get, requests[0].Method);
            Assert.Equal("http://example.com/", requests[0].RequestUri.AbsoluteUri);
            Assert.Equal(HttpMethod.Post, requests[1].Method);
            Assert.Equal("http://example.com/values", requests[1].RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Returns_RequestsFromMultipartContent_WithUriSchemeSet_FromRequest()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new MultipartContent("mixed")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, "https://example.com/values"))
                },
                RequestUri = new Uri("https://example.com/")
            };

            IList<HttpRequestMessage> requests = await batchHandler.ParseBatchRequestsAsync(request, CancellationToken.None);

            Assert.Equal(2, requests.Count);
            Assert.Equal(HttpMethod.Get, requests[0].Method);
            Assert.Equal("https://example.com/", requests[0].RequestUri.AbsoluteUri);
            Assert.Equal(HttpMethod.Post, requests[1].Method);
            Assert.Equal("https://example.com/values", requests[1].RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new MultipartContent("mixed")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values"))
                }
            };
            request.Properties.Add("foo", "bar");
            request.SetRouteData(new HttpRouteData(new HttpRoute()));
            request.RegisterForDispose(new StringContent(String.Empty));

            IList<HttpRequestMessage> requests = await batchHandler.ParseBatchRequestsAsync(request, CancellationToken.None);

            Assert.Equal(2, requests.Count);
            Assert.Equal(HttpMethod.Get, requests[0].Method);
            Assert.Equal("bar", requests[0].Properties["foo"]);
            Assert.Null(requests[0].GetRouteData());
            Assert.Same(requests[0], requests[0].GetUrlHelper().Request);
            Assert.Empty(requests[0].GetResourcesForDisposal());
            Assert.Equal("http://example.com/", requests[0].RequestUri.AbsoluteUri);

            Assert.Equal(HttpMethod.Post, requests[1].Method);
            Assert.Equal("http://example.com/values", requests[1].RequestUri.AbsoluteUri);
            Assert.Equal("bar", requests[1].Properties["foo"]);
            Assert.Null(requests[1].GetRouteData());
            Assert.Same(requests[1], requests[1].GetUrlHelper().Request);
            Assert.Empty(requests[1].GetResourcesForDisposal());
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ValidateRequest(null),
                "request");
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The 'Content' property on the batch request cannot be null.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentTypeIsNull()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = null;

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have a \"Content-Type\" header.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestMediaTypeIsWrong()
        {
            DefaultHttpBatchHandler batchHandler = new DefaultHttpBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/json");

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request of media type 'text/json' is not supported.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        private class MockHttpServer : HttpServer
        {
            private Func<HttpRequestMessage, Task<HttpResponseMessage>> _action;

            public MockHttpServer(Func<HttpRequestMessage, HttpResponseMessage> action)
            {
                _action = request =>
                {
                    return Task.FromResult(action(request));
                };
            }

            public MockHttpServer(Func<HttpRequestMessage, Task<HttpResponseMessage>> action)
            {
                _action = action;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _action(request);
            }
        }

        private class DisposableResponseMessage : HttpResponseMessage
        {
            public bool IsDisposed { get; set; }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
            }
        }
    }
}