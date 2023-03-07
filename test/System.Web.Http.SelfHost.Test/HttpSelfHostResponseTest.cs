// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.SelfHost
{
    [Xunit.Collection("PortReserver Collection")] // Avoid conflicts between different PortReserver consumers.
    public class HttpSelfHostResponseTest
    {
        [Fact]
        public async Task Get_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = true;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/GetNormalResponse"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Get;

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal(0, response.Content.Headers.ContentLength);
            }
        }

        [Fact]
        public async Task Post_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = true;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/PostNormalResponse"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Post;
                request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal(0, response.Content.Headers.ContentLength);
            }
        }

        [Fact]
        public async Task Get_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = false;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/GetNullResponseFromAction"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Get;

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains("\"Message\":\"An error has occurred.\"", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task Post_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = false;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/PostNullResponseFromAction"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Post;
                request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains("\"Message\":\"An error has occurred.\"", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task Get_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = false;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/GetNullTaskFromAction"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Get;

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains("\"Message\":\"An error has occurred.\"", await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task Post_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            using (var selfHostTester = new SelfHostTester())
            {
                // Arrange
                selfHostTester.MessageHandler.ReturnNull = false;
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Path.Combine(selfHostTester.BaseAddress, "NullResponse/PostNullTaskFromAction"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Method = HttpMethod.Post;
                request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

                // Action
                HttpResponseMessage response = await selfHostTester.HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains("\"Message\":\"An error has occurred.\"", await response.Content.ReadAsStringAsync());
            }
        }

        private class SelfHostTester : IDisposable
        {
            private HttpSelfHostServer _server;
            private PortReserver _testPort = new PortReserver();

            public string BaseAddress { get; private set; }
            public HttpClient HttpClient { get; private set; }
            public NullResponseMessageHandler MessageHandler { get; private set; }

            public SelfHostTester()
            {
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(_testPort.BaseUri);
                BaseAddress = _testPort.BaseUri;

                config.HostNameComparisonMode = HostNameComparisonMode.Exact;
                config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "NullResponse" });

                MessageHandler = new NullResponseMessageHandler();
                config.MessageHandlers.Add(MessageHandler);

                _server = new HttpSelfHostServer(config);
                _server.OpenAsync().Wait();

                HttpClient = new HttpClient();
            }

            public void Dispose()
            {
                _testPort.Dispose();
                HttpClient.Dispose();
                _server.CloseAsync().Wait();
            }
        }
    }

    public class NullResponseController : ApiController
    {
        [HttpGet]
        public NullResponseCustomer GetNormalResponse()
        {
            return new NullResponseCustomer() { Name = "Fred", Age = 39 };
        }

        [HttpPost]
        public NullResponseCustomer PostNormalResponse(NullResponseCustomer customer)
        {
            return customer;
        }

        [HttpGet]
        public HttpResponseMessage GetNullResponseFromAction()
        {
            return null;
        }

        [HttpPost]
        public HttpResponseMessage PostNullResponseFromAction(NullResponseCustomer customer)
        {
            return null;
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetNullTaskFromAction()
        {
            return null;
        }

        [HttpPost]
        public Task<HttpResponseMessage> PostNullTaskFromAction(NullResponseCustomer customer)
        {
            return null;
        }
    }

    public class NullResponseCustomer
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class NullResponseMessageHandler : DelegatingHandler
    {
        public bool ReturnNull { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Threading.CancellationToken cancellationToken)
        {
            Task<HttpResponseMessage> t = base.SendAsync(request, cancellationToken);

            if (!ReturnNull)
            {
                return t;
            }

            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
