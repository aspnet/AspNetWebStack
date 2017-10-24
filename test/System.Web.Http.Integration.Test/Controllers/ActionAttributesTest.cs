// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    [CLSCompliant(false)]
    public class ActionAttributesTest
    {
        [Theory]
        [InlineData("GET", "ActionAttributeTest/RetriveUsers", "RetriveUsers")]
        [InlineData("POST", "ActionAttributeTest/AddUsers/1", "AddUsers")]
        [InlineData("PUT", "ActionAttributeTest/UpdateUsers", "UpdateUsers")]
        [InlineData("DELETE", "ActionAttributeTest/DeleteUsers", "DeleteUsers")]
        [InlineData("PATCH", "ActionAttributeTest/Users?key=2", "Users")]
        [InlineData("HEAD", "ActionAttributeTest/Users?key=3", "Users")]
        public void SelectAction_OnRouteWithActionParameter(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ActionAttributeTestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("POST", "ActionAttributeTest/RetriveUsers")]
        [InlineData("DELETE", "ActionAttributeTest/RetriveUsers")]
        [InlineData("WHATEVER", "ActionAttributeTest/RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/UpdateUsers")]
        [InlineData("WHATEVER", "ActionAttributeTest/UpdateUsers")]
        [InlineData("POST", "ActionAttributeTest/DeleteUsers")]
        [InlineData("DELETEME", "ActionAttributeTest/DeleteUsers")]
        public void SelectAction_ThrowsMethodNotSupported_OnRouteWithActionParameter(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);

            var exception = Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The requested resource does not support http method '" + httpMethod + "'.", ((HttpError)content.Value).Message);
        }

        [Theory]
        [InlineData("POST", "ActionAttributeTest/NonAction")]
        [InlineData("ACTION", "ActionAttributeTest/NonActionWitHttpMethod")]
        [InlineData("GET", "ActionAttributeTest/AddUsers")] // id is required on this action, so url is invalid. 404
        [InlineData("PUT", "ActionAttributeTest/AddUsers")] // id is required on this action, so url is invalid. 404
        [InlineData("WHATEVER", "ActionAttributeTest/AddUsers")] // id is required on this action, so url is invalid. 404
        [InlineData("GET", "ActionAttributeTest/Users")] // key param is required, bad url. 404
        [InlineData("POST", "ActionAttributeTest/Users")] // key param is required, bad url. 404
        [InlineData("PATCHING", "ActionAttributeTest/Users")] // key param is required, bad url. 404
        [InlineData("NonAction", "ActionAttributeTest/NonAction")] // NonAction, 404
        [InlineData("GET", "ActionAttributeTest/NonAction")] // NonAction, 404
        public void SelectAction_ThrowsNotFound_OnRouteWithActionParameter(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            controllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);

            var exception = Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                });

            Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);

            // Error message might be ApiControllerActionSelector_ActionNameNotFound or ApiControllerActionSelector_ActionNotFound
            string actualMessage = (string)((HttpError)content.Value)["MessageDetail"];
            Assert.StartsWith("No action was found on the controller 'ActionAttributeTestController' that matches", actualMessage);
        }

        [Theory]
        [InlineData("GET", "ActionAttributeTest/", "RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/3", "RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/4?ssn=12345", "RetriveUsers")]
        [InlineData("POST", "ActionAttributeTest/1", "AddUsers")]
        [InlineData("PUT", "ActionAttributeTest", "UpdateUsers")]
        [InlineData("PUT", "ActionAttributeTest/4", "UpdateUsers")]
        [InlineData("PUT", "ActionAttributeTest/4?extra=thing", "UpdateUsers")]
        [InlineData("DELETE", "ActionAttributeTest", "DeleteUsers")]
        [InlineData("PATCH", "ActionAttributeTest", "PatchUsers")]
        [InlineData("HEAD", "ActionAttributeTest/", "Head")]
        [InlineData("PATCH", "ActionAttributeTest?key=2", "Users")]
        [InlineData("HEAD", "ActionAttributeTest?key=2", "Users")]
        [InlineData("OPTIONS", "ActionAttributeTest", "Options")]
        [InlineData("PATCH", "ActionAttributeTest/2", "Update")]
        [InlineData("HEAD", "ActionAttributeTest/2", "Ping")]
        [InlineData("OPTIONS", "ActionAttributeTest/2", "Help")]
        public void SelectAction_OnDefaultRoute(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ActionAttributeTestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("CONNECT", "ActionAttributeTest")]
        [InlineData("TRACE", "ActionAttributeTest")]
        [InlineData("NonAction", "ActionAttributeTest/")]
        [InlineData("DENY", "ActionAttributeTest")]
        [InlineData("APP", "ActionAttributeTest")]
        public void SelectAction_ThrowsMethodNotSupported_OnDefaultRoute(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);

            var exception = Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The requested resource does not support http method '" + httpMethod + "'.", ((HttpError)content.Value).Message);
        }
    }
}
