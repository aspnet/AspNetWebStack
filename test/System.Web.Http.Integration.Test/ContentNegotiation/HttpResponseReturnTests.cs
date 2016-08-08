// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web.Http.SelfHost;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ContentNegotiation
{
    public class HttpResponseReturnTests
    {
        private HttpServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        public HttpResponseReturnTests()
        {
            this.SetupHost();
        }

        [Theory]
        [InlineData("ReturnHttpResponseMessage")]
        [InlineData("ReturnHttpResponseMessageAsObject")]
        [InlineData("ReturnString")]
        public void ActionReturnsHttpResponseMessage(string action)
        {
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">Hello</string>";

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Get;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<string>(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("ReturnHttpResponseMessageAsXml")]
        public void ActionReturnsHttpResponseMessageWithExplicitMediaType(string action)
        {
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">Hello</string>";

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Method = HttpMethod.Get;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<string>(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("ReturnMultipleSetCookieHeaders")]
        public void ReturnMultipleSetCookieHeadersShouldWork(string action)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseAddress + String.Format("HttpResponseReturn/{0}", action));
            request.Method = HttpMethod.Get;
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            IEnumerable<string> list;
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out list));
            Assert.Equal(new[] { "cookie1", "cookie2" }, list);
        }

        public void SetupHost()
        {
            baseAddress = "http://localhost/";

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "HttpResponseReturn" });
            config.MessageHandlers.Add(new ConvertToStreamMessageHandler());

            server = new HttpServer(config);
            httpClient = new HttpClient(server);
        }
    }

    public class HttpResponseReturnController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage ReturnHttpResponseMessage()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello");
        }

        [HttpGet]
        public object ReturnHttpResponseMessageAsObject()
        {
            return ReturnHttpResponseMessage();
        }

        [HttpGet]
        public HttpResponseMessage ReturnHttpResponseMessageAsXml()
        {
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new ObjectContent<string>("Hello", new XmlMediaTypeFormatter())
            };
            return response;
        }

        [HttpGet]
        public string ReturnString()
        {
            return "Hello";
        }

        [HttpGet]
        public HttpResponseMessage ReturnMultipleSetCookieHeaders()
        {
            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Headers.Add("Set-Cookie", "cookie1");
            resp.Headers.Add("Set-Cookie", "cookie2");
            return resp;
        }
    }
}