// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Owin;

namespace System.Web.Http.Owin
{
    public class OwinHostIntegrationTest
    {
        [Fact]
        public async Task SimpleGet_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = await client.GetAsync(CreateUrl(port, "HelloWorld"));

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", await response.Content.ReadAsStringAsync());
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public async Task SimplePost_Works()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();
                var content = new StringContent("\"Echo this\"", Encoding.UTF8, "application/json");

                var response = await client.PostAsync(CreateUrl(port, "Echo"), content);

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Echo this\"", await response.Content.ReadAsStringAsync());
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public async Task GetThatThrowsDuringSerializations_RespondsWith500()
        {
            using (var port = new PortReserver())
            using (WebApp.Start<OwinHostIntegrationTest>(url: CreateBaseUrl(port)))
            {
                HttpClient client = new HttpClient();

                var response = await client.GetAsync(CreateUrl(port, "Error"));

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                JObject json = Assert.IsType<JObject>(JToken.Parse(await response.Content.ReadAsStringAsync()));
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

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Configuration(IAppBuilder appBuilder)
#pragma warning restore xUnit1013 // Public method should be marked as test
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
