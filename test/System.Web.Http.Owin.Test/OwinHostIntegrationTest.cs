// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Owin.Hosting;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Owin;

namespace System.Web.Http.Owin
{
    public class OwinHostIntegrationTest
    {
        [Fact]
        public void SimpleGet_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, "HelloWorld")).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void SimplePost_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();
                var content = new StringContent("\"Echo this\"", Encoding.UTF8, "application/json");

                var response = client.PostAsync(CreateUrl(port, "Echo"), content).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Echo this\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void GetThatThrowsDuringSerializations_RespondsWith500()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync(CreateUrl(port, "Error")).Result;

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                JObject json = Assert.IsType<JObject>(JToken.Parse(response.Content.ReadAsStringAsync().Result));
                JToken exceptionMessage;
                Assert.True(json.TryGetValue("ExceptionMessage", out exceptionMessage));
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        private static string CreateBaseUrl(PortReserver port)
        {
            return port.BaseUri + "vroot";
        }

        private static string CreateUrl(PortReserver port, string localPath)
        {
            return CreateBaseUrl(port) + "/" + localPath;
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}");
            appBuilder.UseWebApi(config);
        }
    }

    public class HelloWorldController : ApiController
    {
        public string Get()
        {
            return "Hello from OWIN";
        }
    }

    public class EchoController : ApiController
    {
        public string Post([FromBody] string s)
        {
            return s;
        }
    }

    public class ErrorController : ApiController
    {
        public ExceptionThrower Get()
        {
            return new ExceptionThrower();
        }

        public class ExceptionThrower
        {
            public string Throws
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
