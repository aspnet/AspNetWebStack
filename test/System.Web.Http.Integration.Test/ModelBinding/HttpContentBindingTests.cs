// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Tests actions that directly use HttpRequestMessage parameters
    /// </summary>
    public class HttpContentBindingTests
    {
        public HttpContentBindingTests()
        {
            SetupHost();
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        [InlineData("application/json")]
        [InlineData("text/json")]
        public async Task Action_Directly_Reads_HttpRequestMessage(string mediaType)
        {
            Order order = new Order() { OrderId = "99", OrderValue = 100.0 };
            var formatter = new MediaTypeFormatterCollection().FindWriter(typeof(Order), new MediaTypeHeaderValue(mediaType));
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new ObjectContent<Order>(order, formatter, mediaType),
                RequestUri = new Uri(baseAddress + "/HttpContentBinding/HandleMessage"),
                Method = HttpMethod.Post
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            Order receivedOrder = await response.Content.ReadAsAsync<Order>();
            Assert.Equal(order.OrderId, receivedOrder.OrderId);
            Assert.Equal(order.OrderValue, receivedOrder.OrderValue);
        }

        private HttpServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        private void SetupHost()
        {
            baseAddress = "http://localhost/";

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.HostNameComparisonMode = HostNameComparisonMode.Exact;
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "HttpContentBinding", action = "HandleMessage" });
            config.MessageHandlers.Add(new ConvertToStreamMessageHandler());

            server = new HttpServer(config);
            httpClient = new HttpClient(server);
        }
    }

    public class Order
    {
        public string OrderId { get; set; }
        public double OrderValue { get; set; }
    }

    public class HttpContentBindingController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> HandleMessage()
        {
            Order order = await Request.Content.ReadAsAsync<Order>();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<Order>(order, new JsonMediaTypeFormatter())
            };
        }
    }
}