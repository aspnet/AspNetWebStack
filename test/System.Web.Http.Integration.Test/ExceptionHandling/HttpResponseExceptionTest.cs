// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class HttpResponseExceptionTest
    {
        [Theory]
        [InlineData("DoNotThrow")]
        [InlineData("ActionMethod")]
        // TODO : 332683 - HttpResponseExceptions in message handlers
        //[InlineData("RequestMessageHandler")]
        //[InlineData("ResponseMessageHandler")]
        [InlineData("RequestAuthorization")]
        [InlineData("AuthenticationAuthenticate")]
        [InlineData("AuthenticationChallenge")]
        [InlineData("BeforeActionExecuted")]
        [InlineData("AfterActionExecuted")]
        [InlineData("ContentNegotiatorNegotiate")]
        [InlineData("ActionMethodAndExceptionFilter")]
        [InlineData("MediaTypeFormatterReadFromStreamAsync")]
        public async Task HttpResponseExceptionWithExplicitStatusCode(string throwAt)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(ScenarioHelper.BaseAddress + "/ExceptionTests/ReturnString");
            request.Method = HttpMethod.Post;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent("\"" + throwAt + "\"", Encoding.UTF8, "application/json");

            await ScenarioHelper.RunTestAsync(
                "ExceptionTests",
                "/{action}",
                request,
                async response =>
                {
                    Assert.NotNull(response.Content);
                    Assert.NotNull(response.Content.Headers.ContentType);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    if (throwAt == "DoNotThrow")
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Hello World!",
                            await response.Content.ReadAsAsync<string>(new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter() }));
                    }
                    else
                    {
                        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                        Assert.Equal(String.Format("Error at {0}", throwAt),
                            await response.Content.ReadAsAsync<string>(new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter() }));
                    }
                },
                config =>
                {
                    config.Services.Replace(typeof(IContentNegotiator), new CustomContentNegotiator(throwAt));

                    config.MessageHandlers.Add(new CustomMessageHandler(throwAt));
                    config.Filters.Add(new CustomActionFilterAttribute(throwAt));
                    config.Filters.Add(new CustomAuthorizationFilterAttribute(throwAt));
                    config.Filters.Add(new CustomAuthenticationFilter(throwAt));
                    config.Filters.Add(new CustomExceptionFilterAttribute(throwAt));
                    config.Formatters.Clear();
                    config.Formatters.Add(new CustomJsonMediaTypeFormatter(throwAt));
                }
            );
        }
    }

    public class CustomMessageHandler : DelegatingHandler
    {
        private string _throwAt;

        public CustomMessageHandler(string throwAt)
        {
            _throwAt = throwAt;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "RequestMessageHandler");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            ExceptionTestsUtility.CheckForThrow(_throwAt, "ResponseMessageHandler");

            return response;
        }
    }

    public class ExceptionTestsController : ApiController
    {
        [HttpPost]
        public string ReturnString([FromBody] string throwAt)
        {
            string message = "Hello World!";

            // check if the test wants to throw from here
            ExceptionTestsUtility.CheckForThrow(throwAt, "ActionMethod");

            // NOTE: this indicates that we want to throw from here & after this gets intercepted
            // by the ExceptionFilter, we want to throw from there too
            ExceptionTestsUtility.CheckForThrow(throwAt, "ActionMethodAndExceptionFilter");

            return message;
        }
    }

    public class CustomAuthenticationFilter : IAuthenticationFilter
    {
        private string _throwAt;

        public CustomAuthenticationFilter(string throwAt)
        {
            _throwAt = throwAt;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "AuthenticationAuthenticate");
            return Task.FromResult<object>(null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "AuthenticationChallenge");
            return Task.FromResult<object>(null);
        }

        public bool AllowMultiple
        {
            get { return false; }
        }
    }

    public class CustomAuthorizationFilterAttribute : AuthorizationFilterAttribute
    {
        private string _throwAt;

        public CustomAuthorizationFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnAuthorization(HttpActionContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "RequestAuthorization");
        }
    }

    public class CustomActionFilterAttribute : ActionFilterAttribute
    {
        private string _throwAt;

        public CustomActionFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "BeforeActionExecuted");
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "AfterActionExecuted");
        }
    }

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private string _throwAt;

        public CustomExceptionFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "ActionMethodAndExceptionFilter");
        }
    }

    public class CustomContentNegotiator : System.Net.Http.Formatting.DefaultContentNegotiator
    {
        private string _throwAt;

        public CustomContentNegotiator(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "ContentNegotiatorNegotiate");

            return base.Negotiate(type, request, formatters);
        }
    }

    public class CustomJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private string _throwAt;

        public CustomJsonMediaTypeFormatter(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "MediaTypeFormatterReadFromStreamAsync");

            return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "MediaTypeFormatterWriteToStreamAsync");

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }
    }

    public static class ExceptionTestsUtility
    {
        public static void CheckForThrow(string throwAt, string stage)
        {
            if (throwAt == stage)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<string>(String.Format("Error at {0}", stage), new JsonMediaTypeFormatter())
                };

                throw new HttpResponseException(response);
            }
        }
    }
}
