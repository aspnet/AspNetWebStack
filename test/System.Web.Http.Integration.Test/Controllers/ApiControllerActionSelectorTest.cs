// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    [CLSCompliant(false)]
    public class ApiControllerActionSelectorTest
    {
        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/2", "GetUser")]
        [InlineData("GET", "Test/3?name=mario", "GetUserByNameAndId")]
        [InlineData("GET", "Test/3?name=mario&ssn=123456", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456&age=3", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/5?random=9", "GetUser")]
        [InlineData("Post", "Test", "PostUser")]
        [InlineData("Post", "Test?name=mario&age=10", "PostUserByNameAndAge")]

        // Note: Normally the following would not match DeleteUserByIdAndOptName because it has 'id' and 'age' as parameters while the DeleteUserByIdAndOptName action has 'id' and 'name'.
        // However, because the default value is provided on action parameter 'name', having the 'id' in the request was enough to match the action.
        [InlineData("Delete", "Test/6?age=10", "DeleteUserByIdAndOptName")]
        [InlineData("Delete", "Test", "DeleteUserByOptName")]
        [InlineData("Delete", "Test?name=user", "DeleteUserByOptName")]
        [InlineData("Delete", "Test/6?email=user@test.com", "DeleteUserById_Email_OptName_OptPhone")]
        [InlineData("Delete", "Test/6?email=user@test.com&name=user", "DeleteUserById_Email_OptName_OptPhone")]
        [InlineData("Delete", "Test/6?email=user@test.com&name=user&phone=123456789", "DeleteUserById_Email_OptName_OptPhone")]
        [InlineData("Delete", "Test/6?email=user@test.com&height=1.8", "DeleteUserById_Email_Height_OptName_OptPhone")]
        [InlineData("Delete", "Test/6?email=user@test.com&height=1.8&name=user", "DeleteUserById_Email_Height_OptName_OptPhone")]
        [InlineData("Delete", "Test/6?email=user@test.com&height=1.8&name=user&phone=12345678", "DeleteUserById_Email_Height_OptName_OptPhone")]
        [InlineData("Head", "Test/6", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test/6?size=2", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test/6?index=2", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test/6?index=2&size=10", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test/6?index=2&otherParameter=10", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test/6?otherQueryParameter=1234", "Head_Id_OptSize_OptIndex")]
        [InlineData("Head", "Test", "Head")]
        [InlineData("Head", "Test?otherParam=2", "Head")]
        [InlineData("Head", "Test?index=2&size=10", "Head")]
        public void Route_Parameters_Default(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/2", "GetUsersByName")]
        [InlineData("GET", "Test/luigi?ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test/luigi?ssn=123456&id=2&ssn=12345", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?age=10&ssn=123456", "GetUsers")]
        [InlineData("GET", "Test?id=3&ssn=123456&name=luigi", "GetUserByNameIdAndSsn")]
        [InlineData("POST", "Test/luigi?age=20", "PostUserByNameAndAge")]
        public void Route_Parameters_Non_Id(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{name}";
            object routeDefault = new { name = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/3?NAME=mario", "GetUserByNameAndId")]
        [InlineData("GET", "Test/3?name=mario&SSN=123456", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?nAmE=mario&ssn=123456&AgE=3", "GetUserByNameAgeAndSsn")]
        [InlineData("Delete", "Test/6?AGe=10", "DeleteUserByIdAndOptName")]
        public void Route_Parameters_Casing(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{ID}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/GetUsers", "GetUsers")]
        [InlineData("GET", "Test/GetUser/7", "GetUser")]
        [InlineData("GET", "Test/GetUser?id=3", "GetUser")]
        [InlineData("GET", "Test/GetUser/4?id=3", "GetUser")]
        [InlineData("GET", "Test/GetUserByNameAgeAndSsn?name=user&age=90&ssn=123456789", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/GetUserByNameAndSsn?name=user&ssn=123456789", "GetUserByNameAndSsn")]
        [InlineData("POST", "Test/PostUserByNameAndAddress?name=user", "PostUserByNameAndAddress")]
        public void Route_Action(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/getusers", "GetUsers")]
        [InlineData("GET", "Test/getuseR/1", "GetUser")]
        [InlineData("GET", "Test/Getuser?iD=3", "GetUser")]
        [InlineData("GET", "Test/GetUser/4?Id=3", "GetUser")]
        [InlineData("GET", "Test/GetUserByNameAgeandSsn?name=user&age=90&ssn=123456789", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/getUserByNameAndSsn?name=user&ssn=123456789", "GetUserByNameAndSsn")]
        [InlineData("POST", "Test/PostUserByNameAndAddress?name=user", "PostUserByNameAndAddress")]
        public void Route_Action_Name_Casing(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/?name=peach", "GetUsersByName")]
        [InlineData("GET", "Test?name=peach", "GetUsersByName")]
        [InlineData("GET", "Test?name=peach&ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test?name=peach&ssn=123456&age=3", "GetUserByNameAgeAndSsn")]
        public void Route_No_Action(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}";

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "ParameterAttribute/2", "GetUser")]
        [InlineData("GET", "ParameterAttribute?id=2", "GetUser")]
        [InlineData("GET", "ParameterAttribute?myId=2", "GetUserByMyId")]
        [InlineData("POST", "ParameterAttribute/3?name=user", "PostUserNameFromUri")]
        [InlineData("POST", "ParameterAttribute/3", "PostUserNameFromBody")]
        [InlineData("DELETE", "ParameterAttribute/3?name=user", "DeleteUserWithNullableIdAndName")]
        [InlineData("DELETE", "ParameterAttribute?address=userStreet", "DeleteUser")]
        public void ModelBindingParameterAttribute_AreAppliedWhenSelectingActions(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "ParameterAttribute", typeof(ParameterAttributeController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "notActionParameterValue1/Test", "GetUsers")]
        [InlineData("GET", "notActionParameterValue2/Test/2", "GetUser")]
        [InlineData("GET", "notActionParameterValue1/Test?randomQueryVariable=val1", "GetUsers")]
        [InlineData("GET", "notActionParameterValue2/Test/2?randomQueryVariable=val2", "GetUser")]
        public void ActionsThatHaveSubsetOfRouteParameters_AreConsideredForSelection(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{notActionParameter}/{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Fact]
        public void RequestToAmbiguousAction_OnDefaultRoute()
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string httpMethod = "Post";
            string requestUrl = "Test?name=mario";

            // This would result in ambiguous match because complex parameter is not considered for matching.
            // Therefore, PostUserByNameAndAddress(string name, Address address) would conflicts with PostUserByName(string name)
            Assert.Throws<InvalidOperationException>(() =>
                {
                    HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                    context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);
                });
        }

        [Fact]
        public void RequestToActionWithNotSupportedHttpMethod_OnRouteWithAction()
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string requestUrl = "Test/GetUsers";
            string httpMethod = "POST";

            var exception = Assert.Throws<HttpResponseException>(() =>
            {
                HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);
            });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            AssertAllowedHeaders(exception.Response, HttpMethod.Get);
            Assert.Equal("The requested resource does not support http method 'POST'.", ((HttpError)content.Value).Message);
        }

        [Fact]
        public void RequestToActionWith_HttpMethodDefinedByAttributeAndActionName()
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string requestUrl = "Test";
            string httpMethod = "PATCH";

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal("PutUser", descriptor.ActionName);

            // When you have the HttpMethod attribute, the convention should not be applied.
            httpMethod = "PUT";
            var exception = Assert.Throws<HttpResponseException>(() =>
            {
                context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                ApiControllerHelper.SelectAction(context);
            });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The requested resource does not support http method 'PUT'.", ((HttpError)content.Value).Message);
            AssertAllowedHeaders(exception.Response, HttpMethod.Get, new HttpMethod("PATCH"), HttpMethod.Post, HttpMethod.Delete, HttpMethod.Head);
        }

        // Verify response has all the methods in its Allow header. values are unsorted.
        private void AssertAllowedHeaders(HttpResponseMessage response, params HttpMethod[] allowedMethods)
        {
            foreach (var method in allowedMethods)
            {
                Assert.Contains(method.ToString(), response.Content.Headers.Allow);
            }
            Assert.Equal(allowedMethods.Length, response.Content.Headers.Allow.Count);
        }

        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/2", "GetUser")]
        [InlineData("GET", "Test/3?name=mario", "GetUserByNameAndId")]
        [InlineData("GET", "Test/3?name=mario&ssn=123456", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456&age=3", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/5?random=9", "GetUser")]
        public void SelectionBasedOnParameter_IsNotAffectedBy_AddingGlobalValueProvider(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.Configuration.Services.Add(typeof(ValueProviderFactory), new HeaderValueProviderFactory());
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test", "Get")]
        [InlineData("GET", "Test?scope=global", "GetWithEnumParameter")]
        [InlineData("GET", "Test?level=off&kind=trace", "GetWithTwoEnumParameters")]
        [InlineData("GET", "Test?level=", "GetWithNullableEnumParameter")]
        public void SelectAction_ReturnsActionDescriptor_ForEnumParameterOverloads(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}";

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "EnumParameterOverloadsController", typeof(EnumParameterOverloadsController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }
    }
}