// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.SelfHost
{
    [Xunit.Collection("PortReserver Collection")] // Avoid conflicts between different PortReserver consumers.
    public class HttpSelfHostServerTest : IDisposable
    {
        private HttpSelfHostServer server = null;

        public void Dispose()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public async Task SendAsync_Direct_Returns_OK_For_Successful_ObjectContent_Write(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("\"echoString\"", responseString);
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public async Task SendAsync_ServiceModel_Returns_OK_For_Successful_ObjectContent_Write(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange
                server = await CreateServerAsync(port, transferMode);
                bool shouldChunk = transferMode == TransferMode.Streamed;

                // Act
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();
                IEnumerable<string> headerValues = null;
                bool isChunked = response.Headers.TryGetValues("Transfer-Encoding", out headerValues) && headerValues != null &&
                                 headerValues.Any((v) => String.Equals(v, "chunked", StringComparison.OrdinalIgnoreCase));

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("\"echoString\"", responseString);
                Assert.Equal(shouldChunk, isChunked);
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Streamed)]
        public async Task SendAsync_Direct_Returns_OK_For_Successful_Stream_Write(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode);
                HttpResponseMessage response = await new HttpClient(server).GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();
                IEnumerable<string> headerValues = null;
                bool isChunked = response.Headers.TryGetValues("Transfer-Encoding", out headerValues) && headerValues != null &&
                                 headerValues.Any((v) => String.Equals(v, "chunked", StringComparison.OrdinalIgnoreCase));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("echoStream", responseString);
                Assert.False(isChunked);    // stream never chunk, buffered or streamed
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Streamed)]
        public async Task SendAsync_ServiceModel_Returns_OK_For_Successful_Stream_Write(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("echoStream", responseString);
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Streamed)]
        public async Task SendAsync_Direct_Throws_When_ObjectContent_CopyToAsync_Throws(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act & Assert
                server = await CreateServerAsync(port, transferMode);
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => new HttpClient(server).GetAsync(BaseUri(port, transferMode) + uri));
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Streamed)]
        public async Task SendAsync_ServiceModel_Closes_Connection_When_ObjectContent_CopyToAsync_Throws(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange
                server = await CreateServerAsync(port, transferMode);
                Task<HttpResponseMessage> task = new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);

                // Act & Assert
                await Assert.ThrowsAsync<HttpRequestException>(() => task);
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public async Task SendAsync_Direct_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act & Assert
                server = await CreateServerAsync(port, transferMode);
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => new HttpClient(server).GetAsync(BaseUri(port, transferMode) + uri));
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public async Task SendAsync_ServiceModel_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange
                server = await CreateServerAsync(port, transferMode);
                Task task = new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);

                // Act & Assert
                await Assert.ThrowsAsync<HttpRequestException>(() => task);
            }
        }

        [Fact]
        public async Task SendAsync_ServiceModel_AddsSelfHostHttpRequestContext()
        {
            // Arrange
            using (PortReserver port = new PortReserver())
            {
                string baseUri = port.BaseUri;

                HttpRequestContext context = null;
                Uri via = null;

                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = (r, c) =>
                {
                    if (r != null)
                    {
                        context = r.GetRequestContext();
                    }

                    SelfHostHttpRequestContext typedContext = context as SelfHostHttpRequestContext;

                    if (typedContext != null)
                    {
                        via = typedContext.RequestContext.RequestMessage.Properties.Via;
                    }

                    return Task.FromResult(new HttpResponseMessage());
                };

                using (HttpSelfHostConfiguration expectedConfiguration = new HttpSelfHostConfiguration(baseUri))
                {
                    expectedConfiguration.HostNameComparisonMode = HostNameComparisonMode.Exact;

                    using (HttpMessageHandler dispatcher = new LambdaHttpMessageHandler(sendAsync))
                    using (HttpSelfHostServer server = new HttpSelfHostServer(expectedConfiguration, dispatcher))
                    using (HttpClient client = new HttpClient())
                    using (HttpRequestMessage expectedRequest = new HttpRequestMessage(HttpMethod.Get, baseUri))
                    {
                        await server.OpenAsync();

                        // Act
                        using (HttpResponseMessage ignore = await client.SendAsync(expectedRequest))
                        {
                            // Assert
                            SelfHostHttpRequestContext typedContext = (SelfHostHttpRequestContext)context;
                            Assert.Equal(expectedRequest.RequestUri, via);
                            Assert.Same(expectedConfiguration, context.Configuration);
                            Assert.Equal(expectedRequest.RequestUri, typedContext.Request.RequestUri);

                            await server.CloseAsync();
                        }
                    }
                }
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public async Task Get_Returns_Hard404_If_IgnoreRoute(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode, ignoreRoute: true);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.False(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        [Theory]
        [InlineData("/a/b/c/d/e", TransferMode.Buffered)]
        [InlineData("/EchoString?f=12", TransferMode.Streamed)]
        public async Task Get_Returns_Hard404_If_IgnoreRouteDoesNotMatch(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode, ignoreRoute: true);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.False(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        [Theory]
        [InlineData("/constraint/values/10", TransferMode.Buffered)]
        [InlineData("/constraint/values/15", TransferMode.Buffered)]
        [InlineData("/constraint/values/20", TransferMode.Buffered)]
        public async Task Get_Returns_Hard404_If_IgnoreRoute_WithConstraints_ConstraintsMatched(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode, ignoreRoute: true);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.False(response.RequestMessage.Properties.ContainsKey(HttpPropertyKeys.NoRouteMatched));
            }
        }

        [Theory]
        [InlineData("/constraint/values/40", TransferMode.Buffered)]
        [InlineData("/constraint/values/50", TransferMode.Buffered)]
        [InlineData("/constraint/values/65", TransferMode.Buffered)]
        public async Task Get_Returns_Value_If_IgnoreRoute_WithConstraints_ConstraintsNotMatched(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode, ignoreRoute: true);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(String.Concat("/constraint/values/", responseString), uri);
            }
        }

        [Theory]
        [InlineData("/other/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/other/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public async Task Get_Returns_Success_If_OtherRouteMatched(string uri, TransferMode transferMode)
        {
            using (var port = new PortReserver())
            {
                // Arrange & Act
                server = await CreateServerAsync(port, transferMode, ignoreRoute: true);
                HttpResponseMessage response = await new HttpClient().GetAsync(BaseUri(port, transferMode) + uri);
                string responseString = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("\"echoString\"", responseString);
            }
        }

        internal class ThrowsBeforeTaskObjectContent : ObjectContent
        {
            public ThrowsBeforeTaskObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw new InvalidOperationException("ThrowBeforeTask");
            }
        }

        internal class ThrowBeforeWriteObjectContent : ObjectContent
        {
            public ThrowBeforeWriteObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return Task.Factory.StartNew(() =>
                        {
                            throw new InvalidOperationException("ThrowBeforeWrite");
                        });

            }
        }

        internal class ThrowAfterWriteObjectContent : ObjectContent
        {
            public ThrowAfterWriteObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return Task.Factory.StartNew(() =>
                                                 {
                                                     byte[] buffer =
                                                         Encoding.UTF8.GetBytes("ThrowAfterWrite");
                                                     stream.Write(buffer, 0, buffer.Length);
                                                     throw new InvalidOperationException("ThrowAfterWrite");
                                                 });
            }
        }

        internal class ThrowBeforeWriteStream : StreamContent
        {
            public ThrowBeforeWriteStream()
                : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowBeforeWriteStream")))
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw new InvalidOperationException("ThrowBeforeWriteStream");
            }
        }

        internal class ThrowAfterWriteStream : StreamContent
        {
            public ThrowAfterWriteStream()
                : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowAfterWriteStream")))
            {
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                await base.SerializeToStreamAsync(stream, context);
                throw new InvalidOperationException("ThrowAfterWriteStream");
            }
        }

        private static async Task<HttpSelfHostServer> CreateServerAsync(PortReserver port, TransferMode transferMode, bool ignoreRoute = false)
        {
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(BaseUri(port, transferMode));
            config.HostNameComparisonMode = HostNameComparisonMode.Exact;
            if (ignoreRoute)
            {
                config.Routes.IgnoreRoute("Ignore", "{controller}/{action}");
                config.Routes.IgnoreRoute("IgnoreWithConstraints", "constraint/values/{id}", constraints: new { constraint = new CustomConstraint() });
            }
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.Routes.MapHttpRoute("Other", "other/{controller}/{action}");
            config.TransferMode = transferMode;
            config.MapHttpAttributeRoutes();

            HttpSelfHostServer server = new HttpSelfHostServer(config);
            await server.OpenAsync();
            return server;
        }

        public class CustomConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
                IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                long id;
                if (values.ContainsKey("id")
                    && Int64.TryParse(values["id"].ToString(), out id)
                    && (id == 10 || id == 15 || id == 20))
                {
                    return true;
                }

                return false;
            }
        }

        private static string BaseUri(PortReserver port, TransferMode transferMode)
        {
            return transferMode == TransferMode.Streamed
                ? port.BaseUri + "stream"
                : port.BaseUri;
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }
    }

    public class SelfHostServerTestController : ApiController
    {
        [HttpGet]
        public string EchoString()
        {
            return "echoString";
        }

        [HttpGet]
        public HttpResponseMessage EchoStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("echoStream")))
                       };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeTask()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = Request,
                    Content = new HttpSelfHostServerTest.ThrowsBeforeTaskObjectContent()
                };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeWrite()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowBeforeWriteObjectContent()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowAfterWrite()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowAfterWriteObjectContent()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeWriteStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowBeforeWriteStream()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowAfterWriteStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowAfterWriteStream()
            };
        }
    }

    [RoutePrefix("constraint")]
    public class IgnoreRouteWithConstraintsTestController : ApiController
    {
        [Route("values/{id:int}")]
        public int Get(int id)
        {
            return id;
        }
    }
}
