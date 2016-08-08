// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Properties;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http
{
    public class ExceptionHandlingTest
    {
        [Theory]
        [InlineData("Unavailable")]
        [InlineData("AsyncUnavailable")]
        [InlineData("AsyncUnavailableDelegate")]
        public void ThrowingHttpResponseException_FromAction_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
                }
            );
        }

        [Theory]
        [InlineData("ArgumentNull")]
        [InlineData("AsyncArgumentNull")]
        public void ThrowingArgumentNullException_FromAction_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    HttpError exception = response.Content.ReadAsAsync<HttpError>().Result;
                    Assert.Equal(typeof(ArgumentNullException).FullName, exception["ExceptionType"].ToString());
                }
            );
        }

        [Theory]
        [InlineData("ArgumentNull")]
        [InlineData("AsyncArgumentNull")]
        public void ThrowingArgumentNullException_FromAction_GetsReturnedToClientParsedAsJson(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    dynamic json = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                    string result = json.ExceptionType;
                    Assert.Equal(typeof(ArgumentNullException).FullName, result);
                }
            );
        }

        [Theory]
        [InlineData("AuthenticationFilterAuthenticate")]
        [InlineData("AuthenticationFilterAuthenticateResult")]
        [InlineData("AuthenticationFilterChallenge")]
        [InlineData("AuthenticationFilterChallengeResult")]
        [InlineData("AuthorizationFilter")]
        [InlineData("ActionFilter")]
        [InlineData("ExceptionFilter")]
        public void ThrowingArgumentException_FromFilter_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    HttpError exception = response.Content.ReadAsAsync<HttpError>().Result;
                    Assert.Equal(typeof(ArgumentException).FullName, exception["ExceptionType"].ToString());
                }
            );
        }

        [Theory]
        [InlineData("AuthenticationFilterAuthenticate", HttpStatusCode.Ambiguous)]
        [InlineData("AuthenticationFilterAuthenticateResult", HttpStatusCode.BadGateway)]
        [InlineData("AuthenticationFilterChallenge", HttpStatusCode.BadRequest)]
        [InlineData("AuthenticationFilterChallengeResult", HttpStatusCode.Conflict)]
        [InlineData("AuthorizationFilter", HttpStatusCode.Forbidden)]
        [InlineData("ActionFilter", HttpStatusCode.NotAcceptable)]
        [InlineData("ExceptionFilter", HttpStatusCode.NotImplemented)]
        public void ThrowingHttpResponseException_FromFilter_GetsReturnedToClient(string actionName, HttpStatusCode responseExceptionStatusCode)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Add(ExceptionController.ResponseExceptionHeaderKey, responseExceptionStatusCode.ToString());

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                request,
                (response) =>
                {
                    Assert.Equal(responseExceptionStatusCode, response.StatusCode);
                    Assert.Equal("HttpResponseExceptionMessage", response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        // TODO: add tests that throws from custom model binders

        [Fact]
        public void Service_ReturnsNotFound_WhenControllerNameDoesNotExist()
        {
            string controllerName = "randomControllerThatCannotBeFound";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.DefaultControllerFactory_ControllerNameNotFound, controllerName),
                        response.Content.ReadAsAsync<HttpError>().Result["MessageDetail"]);
                }
            );
        }

        [Fact]
        public void Service_ReturnsNotFound_WhenActionNameDoesNotExist()
        {
            string controllerName = "Exception";
            string actionName = "actionNotFound";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, controllerName, actionName),
                        response.Content.ReadAsAsync<HttpError>().Result["MessageDetail"]);
                }
            );
        }

        [Fact]
        public void Service_ReturnsMethodNotAllowed_WhenActionsDoesNotSupportTheRequestHttpMethod()
        {
            string controllerName = "Exception";
            string actionName = "GetString";
            HttpMethod requestMethod = HttpMethod.Post;
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);
            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(requestMethod, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, requestMethod.Method),
                        response.Content.ReadAsAsync<HttpError>().Result.Message);
                }
            );
        }

        [Fact]
        public void Service_ReturnsInternalServerError_WhenMultipleActionsAreFound()
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Contains(
                        String.Format(SRResources.ApiControllerActionSelector_AmbiguousMatch, String.Empty),
                        response.Content.ReadAsAsync<HttpError>().Result["ExceptionMessage"] as string);
                }
            );
        }

        [Fact]
        public void Service_ReturnsInternalServerError_WhenMultipleControllersAreFound()
        {
            string controllerName = "Duplicate";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Contains(
                        String.Format(SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate, controllerName, "{controller}", String.Empty, Environment.NewLine),
                        response.Content.ReadAsAsync<HttpError>().Result["ExceptionMessage"] as string);
                }
            );
        }

        [Fact]
        public void GenericMethod_Throws_InvalidOperationException()
        {
            HttpConfiguration config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            config.Routes.MapHttpRoute("Default", "Exception/{action}", new { controller = "Exception" });
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            // Ensure that the behavior is repeatable and other action is still callable after the error 
            for (int i = 0; i < 10; i++)
            {
                // Make sure other action can be called
                HttpResponseMessage response = client.GetAsync("http://localhost/Exception/GetString").Result;
                Assert.True(response.IsSuccessStatusCode,
                    String.Format("Successful status code was expected but got '{0}' instead. Error: {1}", response.StatusCode, response.Content.ReadAsStringAsync().Result));

                // Make a request to generic method and verify the exception
                response = client.PostAsync("http://localhost/Exception/GenericAction", null).Result;
                Type controllerType = typeof(ExceptionController);
                HttpError exception = response.Content.ReadAsAsync<HttpError>().Result;
                Assert.Equal(typeof(InvalidOperationException).FullName, exception["ExceptionType"]);
                Assert.Equal(
                    String.Format(
                        SRResources.ReflectedHttpActionDescriptor_CannotCallOpenGenericMethods,
                        controllerType.GetMethod("GenericAction"),
                        controllerType.FullName),
                    exception["ExceptionMessage"]);
            }
        }
    }
}
