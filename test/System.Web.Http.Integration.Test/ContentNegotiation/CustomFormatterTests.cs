// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ContentNegotiation
{
    public class CustomFormatterTests
    {
        private HttpServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;
        private HttpConfiguration config = null;

        public CustomFormatterTests()
        {
            SetupHost();
        }

        [Fact]
        public async Task CustomFormatter_Overrides_SetResponseHeaders_During_Conneg()
        {
            Order reqOrdr = new Order() { OrderId = "100", OrderValue = 100.00 };
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<Order>(reqOrdr, new XmlMediaTypeFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoOrder");
            request.Method = HttpMethod.Post;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plainwithversioninfo"));

            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            IEnumerable<string> versionHdr = null;
            Assert.True(response.Content.Headers.TryGetValues("Version", out versionHdr));
            Assert.Equal("1.3.5.0", versionHdr.First());
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("text/plainwithversioninfo", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task CustomFormatter_Post_Returns_Request_String_Content()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<string>("Hello World!", new PlainTextFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoString");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("Hello World!", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CustomFormatter_Post_Returns_Request_Integer_Content()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<int>(100, new PlainTextFormatter())
            };

            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoInt");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(100, Convert.ToInt32(await response.Content.ReadAsStringAsync()));
        }

        [Fact]
        public async Task CustomFormatter_Post_Returns_Request_ComplexType_Content()
        {
            Order reqOrdr = new Order() { OrderId = "100", OrderValue = 100.00 };
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<Order>(reqOrdr, new PlainTextFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoOrder");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
        }

        private void SetupHost()
        {
            baseAddress = "http://localhost/";
            config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "CustomFormatterTests", action = "EchoOrder" });
            config.MessageHandlers.Add(new ConvertToStreamMessageHandler());
            config.Formatters.Add(new PlainTextFormatterWithVersionInfo());
            config.Formatters.Add(new PlainTextFormatter());

            server = new HttpServer(config);
            httpClient = new HttpClient(server);
        }
    }

    public class PlainTextFormatterWithVersionInfo : MediaTypeFormatter
    {
        public PlainTextFormatterWithVersionInfo()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plainwithversioninfo"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override void SetDefaultContentHeaders(Type objectType, HttpContentHeaders contentHeaders, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(objectType, contentHeaders, mediaType);
            contentHeaders.TryAddWithoutValidation("Version", "1.3.5.0");
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            string stringContent = null;

            using (var reader = new StreamReader(readStream))
            {
                stringContent = reader.ReadToEnd();
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(stringContent);

            return tcs.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var output = value.ToString();
            var writer = new StreamWriter(writeStream);
            writer.Write(output);
            writer.Flush();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);

            return tcs.Task;
        }
    }

    public class PlainTextFormatter : MediaTypeFormatter
    {
        public PlainTextFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            object result = null;

            using (var reader = new StreamReader(readStream))
            {
                result = reader.ReadToEnd();
            }

            if (type == typeof(Int32))
            {
                result = Int32.Parse((string)result);
            }
            else if (type == typeof(Order))
            {
                result = new Order((string)result);
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var output = value == null ? String.Empty : value.ToString();
            var writer = new StreamWriter(writeStream);
            writer.Write(output);
            writer.Flush();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }

    public class CustomFormatterTestsController : ApiController
    {
        [HttpPost]
        public string EchoString([FromBody] string input)
        {
            return input;
        }

        [HttpPost]
        public int EchoInt([FromBody] int input)
        {
            return input;
        }

        [HttpPost]
        public Order EchoOrder(Order order)
        {
            return order;
        }
    }

    public class Order : IEquatable<Order>, ICloneable
    {
        public string OrderId { get; set; }
        public double OrderValue { get; set; }

        public Order() { }

        public Order(string value)
        {
            string[] pieces = value.Split(new[] { '\n' }, 2);

            OrderId = pieces[0];
            OrderValue = Double.Parse(pieces[1]);
        }

        public bool Equals(Order other)
        {
            return (this.OrderId.Equals(other.OrderId) && this.OrderValue.Equals(other.OrderValue));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return String.Format("{0}\n{1}", OrderId, OrderValue);
        }
    }
}
