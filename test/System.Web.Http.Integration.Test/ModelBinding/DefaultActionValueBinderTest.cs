// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.ModelBinding
{
    public class DefaultActionValueBinderTest
    {
        [Fact]
        public async Task BindValuesAsync_Uses_DefaultValues()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Get") });
            CancellationToken cancellationToken = new CancellationToken();
            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 0;
            expectedResult["firstName"] = "DefaultFirstName";
            expectedResult["lastName"] = "DefaultLastName";
            Assert.Equal(expectedResult, context.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_WithObjectContentInRequest_Works()
        {
            // Arrange
            ActionValueItem cust = new ActionValueItem() { FirstName = "FirstName", LastName = "LastName", Id = 1 };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });
            context.ControllerContext.Request = new HttpRequestMessage
            {
                Content = new ObjectContent<ActionValueItem>(cust, new JsonMediaTypeFormatter())
            };
            CancellationToken cancellationToken = new CancellationToken();
            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            var result = context.ActionArguments;
            Assert.Single(result);
            var item = Assert.IsType<ActionValueItem>(result["item"]);
            Assert.Equal(cust.FirstName, item.FirstName);
            Assert.Equal(cust.LastName, item.LastName);
            Assert.Equal(cust.Id, item.Id);
        }

        #region Query Strings

        [Fact]
        public async Task BindValuesAsync_ConvertEmptyString()
        {
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?A1=&A2=&A3=&A4=")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetTestEmptyString") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, CancellationToken.None);

            // Assert
            ConvertEmptyStringContainer arg = (ConvertEmptyStringContainer)actionContext.ActionArguments["x"];

            Assert.NotNull(arg);
            Assert.Equal(String.Empty, arg.A1);
            Assert.Null(arg.A2);
            Assert.Null(arg.A3);
            Assert.Null(arg.A4);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Simple_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=5&firstName=queryFirstName&lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Get") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 5;
            expectedResult["firstName"] = "queryFirstName";
            expectedResult["lastName"] = "queryLastName";
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Simple_Types_With_FromUriAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=5&firstName=queryFirstName&lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetFromUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 5;
            expectedResult["firstName"] = "queryFirstName";
            expectedResult["lastName"] = "queryLastName";
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Complex_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=5&firstName=queryFirstName&lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItem") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Assert.True(actionContext.ModelState.IsValid);
            KeyValuePair<string, object> actionArgument = Assert.Single(actionContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(actionArgument.Value);
            Assert.Equal(5, deserializedActionValueItem.Id);
            Assert.Equal("queryFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("queryLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Post_Complex_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=5&firstName=queryFirstName&lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexTypeUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Assert.True(actionContext.ModelState.IsValid);
            KeyValuePair<string, object> actionArgument = Assert.Single(actionContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(actionArgument.Value);
            Assert.Equal(5, deserializedActionValueItem.Id);
            Assert.Equal("queryFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("queryLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Post_Enumerable_Complex_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?items[0].id=5&items[0].firstName=queryFirstName&items[0].lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostEnumerableUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Assert.True(actionContext.ModelState.IsValid);
            KeyValuePair<string, object> keyValuePair = Assert.Single(actionContext.ActionArguments);
            IEnumerable<ActionValueItem> items = Assert.IsAssignableFrom<IEnumerable<ActionValueItem>>(keyValuePair.Value);
            ActionValueItem deserializedActionValueItem = items.First();
            Assert.Equal(5, deserializedActionValueItem.Id);
            Assert.Equal("queryFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("queryLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Post_Enumerable_Complex_Types_No_Index()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=5&firstName=queryFirstName&items.lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostEnumerableUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Assert.True(actionContext.ModelState.IsValid);
            KeyValuePair<string, object> keyValuePair = Assert.Single(actionContext.ActionArguments);
            IEnumerable<ActionValueItem> items = Assert.IsAssignableFrom<IEnumerable<ActionValueItem>>(keyValuePair.Value);
            Assert.Empty(items);     // expect unsuccessful bind but proves we don't loop infinitely
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_ComplexType_Using_Prefixes()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?item.id=5&item.firstName=queryFirstName&item.lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItem") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(actionContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(5, deserializedActionValueItem.Id);
            Assert.Equal("queryFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("queryLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_ComplexType_Using_FromUriAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?item.id=5&item.firstName=queryFirstName&item.lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItemFromUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(actionContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(5, deserializedActionValueItem.Id);
            Assert.Equal("queryFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("queryLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_Using_Custom_ValueProviderAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetFromCustom") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 99;
            expectedResult["firstName"] = "99";
            expectedResult["lastName"] = "99";
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_Using_Prefix_To_Rename()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?custid=5&first=renamedFirstName&last=renamedLastName")
                    // notice the query string names match the prefixes in GetFromNamed() and not the actual parameter names
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetFromNamed") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 5;
            expectedResult["firstName"] = "renamedFirstName";
            expectedResult["lastName"] = "renamedLastName";
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_Query_String_Values_To_Complex_Types_With_Validation_Error()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost?id=100&firstName=queryFirstName&lastName=queryLastName")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItem") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Assert.False(actionContext.ModelState.IsValid);
        }

        #endregion Query Strings

        #region RouteData

        [Fact]
        public async Task BindValuesAsync_RouteData_Values_To_Simple_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("id", 6);
            route.Values.Add("firstName", "routeFirstName");
            route.Values.Add("lastName", "routeLastName");

            HttpActionContext controllerContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(route, new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Get") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(controllerContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 6;
            expectedResult["firstName"] = "routeFirstName";
            expectedResult["lastName"] = "routeLastName";
            Assert.Equal(expectedResult, controllerContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_RouteData_Values_To_Simple_Types_Using_FromUriAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("id", 6);
            route.Values.Add("firstName", "routeFirstName");
            route.Values.Add("lastName", "routeLastName");

            HttpActionContext controllerContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(route, new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Get") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(controllerContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["id"] = 6;
            expectedResult["firstName"] = "routeFirstName";
            expectedResult["lastName"] = "routeLastName";
            Assert.Equal(expectedResult, controllerContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_RouteData_Values_To_Complex_Types()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("id", 6);
            route.Values.Add("firstName", "routeFirstName");
            route.Values.Add("lastName", "routeLastName");

            HttpActionContext controllerContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(route, new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItem") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(controllerContext, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(controllerContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(6, deserializedActionValueItem.Id);
            Assert.Equal("routeFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("routeLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_RouteData_Values_To_Complex_Types_Using_FromUriAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("id", 6);
            route.Values.Add("firstName", "routeFirstName");
            route.Values.Add("lastName", "routeLastName");

            HttpActionContext controllerContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(route, new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://localhost")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetItemFromUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(controllerContext, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(controllerContext.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsType<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(6, deserializedActionValueItem.Id);
            Assert.Equal("routeFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("routeLastName", deserializedActionValueItem.LastName);
        }

        #endregion RouteData

        #region ControllerContext
        [Fact]
        public async Task BindValuesAsync_ControllerContext_CancellationToken()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetFromCancellationToken") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(actionContext.ActionArguments);
            Assert.Equal(cancellationToken, keyValuePair.Value);
        }
        #endregion ControllerContext

        #region Body

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Json()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string jsonString = "{\"Id\":\"7\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Json_With_Validation_Error()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string jsonString = "{\"Id\":\"100\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            Assert.False(context.ModelState.IsValid);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_FormUrlEncoded()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string formUrlEncodedString = "Id=7&FirstName=testFirstName&LastName=testLastName";
            StringContent stringContent = new StringContent(formUrlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_FormUrlEncoded_With_Validation_Error()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string formUrlEncodedString = "Id=101&FirstName=testFirstName&LastName=testLastName";
            StringContent stringContent = new StringContent(formUrlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            Assert.False(context.ModelState.IsValid);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Xml()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");
            ActionValueItem item = new ActionValueItem() { Id = 7, FirstName = "testFirstName", LastName = "testLastName" };
            ObjectContent<ActionValueItem> tempContent = new ObjectContent<ActionValueItem>(item, new XmlMediaTypeFormatter());
            StringContent stringContent = new StringContent(await tempContent.ReadAsStringAsync());
            stringContent.Headers.ContentType = mediaType;
            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(item.Id, deserializedActionValueItem.Id);
            Assert.Equal(item.FirstName, deserializedActionValueItem.FirstName);
            Assert.Equal(item.LastName, deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Xml_Structural()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");

            // Test sending from a non .NET type (raw xml).
            // The default XML serializer requires that the xml root name matches the C# class name.
            string xmlSource =
                @"<ActionValueItem xmlns='http://schemas.datacontract.org/2004/07/System.Web.Http.ModelBinding' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                      <FirstName>testFirstName</FirstName>
                      <Id>7</Id>
                      <LastName>testLastName</LastName>
                  </ActionValueItem>".Replace('\'', '"');

            StringContent stringContent = new StringContent(xmlSource);
            stringContent.Headers.ContentType = mediaType;
            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Xml_With_Validation_Error()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");
            ActionValueItem item = new ActionValueItem() { Id = 101, FirstName = "testFirstName", LastName = "testLastName" };
            var tempContent = new ObjectContent<ActionValueItem>(item, new XmlMediaTypeFormatter());
            StringContent stringContent = new StringContent(await tempContent.ReadAsStringAsync());
            stringContent.Headers.ContentType = mediaType;
            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            Assert.False(context.ModelState.IsValid);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_NullContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage() { Content = null };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, CancellationToken.None);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            object deserializedActionValueItem = keyValuePair.Value;
            Assert.Null(deserializedActionValueItem);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_And_Uri_To_Simple()
        {
            // Arrange
            string jsonString = "{\"Id\":\"7\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/ActionValueController/PostFromBody?id=123"),
                Content = stringContent
            };

            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostFromBodyAndUri") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(2, context.ActionArguments.Count);
            Assert.Equal(123, context.ActionArguments["id"]);

            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(context.ActionArguments["item"]);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Using_FromBodyAttribute()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string jsonString = "{\"Id\":\"7\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };

            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostFromBody") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_Complex_Type_Using_Formatter_To_Deserialize()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string jsonString = "{\"Id\":\"7\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostComplexType") });
            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem deserializedActionValueItem = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }


        [Fact]
        public async Task BindValuesAsync_Body_To_IEnumerable_Complex_Type_Json()
        {
            // ModelBinding will bind T to IEnumerable<T>, but JSON.Net won't. So enclose JSON in [].
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            string jsonString = "[{\"Id\":\"7\",\"FirstName\":\"testFirstName\",\"LastName\":\"testLastName\"}]";
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostEnumerable") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            IEnumerable<ActionValueItem> items = Assert.IsAssignableFrom<IEnumerable<ActionValueItem>>(keyValuePair.Value);
            ActionValueItem deserializedActionValueItem = items.First();
            Assert.Equal(7, deserializedActionValueItem.Id);
            Assert.Equal("testFirstName", deserializedActionValueItem.FirstName);
            Assert.Equal("testLastName", deserializedActionValueItem.LastName);
        }

        [Fact]
        public async Task BindValuesAsync_Body_To_JToken()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/json");
            ActionValueItem item = new ActionValueItem() { Id = 7, FirstName = "testFirstName", LastName = "testLastName" };
            string json = "{\"a\":123,\"b\":[false,null,12.34]}";
            JToken jt = JToken.Parse(json);
            var tempContent = new ObjectContent<JToken>(jt, new JsonMediaTypeFormatter());
            StringContent stringContent = new StringContent(await tempContent.ReadAsStringAsync());
            stringContent.Headers.ContentType = mediaType;
            HttpRequestMessage request = new HttpRequestMessage() { Content = stringContent };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostJsonValue") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, cancellationToken);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            JToken deserializedJsonValue = Assert.IsAssignableFrom<JToken>(keyValuePair.Value);
            string deserializedJsonAsString = deserializedJsonValue.ToString(Formatting.None);
            Assert.Equal(json, deserializedJsonAsString);
        }

        #endregion Body

        [Fact]
        public async Task BindValuesAsync_FromUriAttribute_DecoratedOn_Type()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = new HttpMethod("Patch"),
                    RequestUri = new Uri("http://localhost?x=123&y=456&data.description=mypoint")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Patch") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["point"] = new Point { X = 123, Y = 456, Data = new Data { Description = "mypoint" } };
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_ModelBinderAttribute_WithEmptyName()
        {
            // Arrange
            CancellationToken cancellationToken = new CancellationToken();
            HttpActionContext actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(new HttpRequestMessage()
                {
                    Method = new HttpMethod("Options"),
                    RequestUri = new Uri("http://localhost?x=123&y=456&data.description=mypoint")
                }),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("Options") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, cancellationToken);

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["data"] = new Point { X = 123, Y = 456, Data = new Data { Description = "mypoint" } };
            Assert.Equal(expectedResult, actionContext.ActionArguments, new DictionaryEqualityComparer());
        }

        [Fact]
        public async Task BindValuesAsync_Config_BindParameter()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.BindParameter(typeof(Data), new CustomModelBinder());

            HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("http://localhost")
                };

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(config, request);
            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext,
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("GetData") });

            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(actionContext, CancellationToken.None);

            // Assert
            Data argData = (Data)actionContext.ActionArguments["data"];
            Assert.NotNull(argData);
            Assert.Equal("testing", argData.Description);
        }

        [Fact]
        public async Task BindValuesAsync_Config_BindParameter_ShouldNotBeAppliedToUnrelatedTypes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.BindParameter(typeof(Data), new CustomModelBinder());

            ActionValueItem item = new ActionValueItem() { Id = 7, FirstName = "testFirstName", LastName = "testLastName" };
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new ObjectContent(typeof(ActionValueItem), item, config.Formatters.JsonFormatter)
            };
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(config, request),
                new ReflectedHttpActionDescriptor() { MethodInfo = typeof(ActionValueController).GetMethod("PostItem") });
            DefaultActionValueBinder provider = new DefaultActionValueBinder();

            // Act
            await provider.BindValuesAsync(context, CancellationToken.None);

            // Assert
            KeyValuePair<string, object> keyValuePair = Assert.Single(context.ActionArguments);
            ActionValueItem result = Assert.IsAssignableFrom<ActionValueItem>(keyValuePair.Value);
            Assert.Equal(7, result.Id);
            Assert.Equal("testFirstName", result.FirstName);
            Assert.Equal("testLastName", result.LastName);
        }
    }

    [FromUri]
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Data Data { get; set; }

        public override bool Equals(object obj)
        {
            Point other = obj as Point;
            if (other != null)
            {
                return other.X == X && other.Y == Y && other.Data.Description == other.Data.Description;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    // Test model binder to bind typeof(Data)
    public class CustomModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            Assert.Equal(typeof(Data), bindingContext.ModelType);
            bindingContext.Model = new Data { Description = "testing" };
            return true;
        }
    }

    public class Data
    {
        public string Description { get; set; }
    }

    public class ActionValueController : ApiController
    {
        public void GetData(Data data)
        {
        }

        public void PostItem(ActionValueItem item)
        {
        }

        // Demonstrates the use of ModelBinderAttribute with empty name
        public void Options([FromUri(Name = "")]Point data) { }

        // Demonstrates complex parameter that has FromUri declared on the type
        public void Patch(Point point) { }

        // Demonstrates parameter that can come from route, query string, or defaults
        public ActionValueItem Get(int id = 0, string firstName = "DefaultFirstName", string lastName = "DefaultLastName")
        {
            return new ActionValueItem() { Id = id, FirstName = firstName, LastName = lastName };
        }

        // Demonstrates an explicit override to obtain parameters from URL
        public ActionValueItem GetFromUri([FromUri] int id = 0,
                                   [FromUri] string firstName = "DefaultFirstName",
                                   [FromUri] string lastName = "DefaultLastName")
        {
            return new ActionValueItem() { Id = id, FirstName = firstName, LastName = lastName };
        }


        // Complex objects default to body. But we can bind from URI with an attribute.
        public ActionValueItem GetItem([FromUri] ActionValueItem item)
        {
            return item;
        }

        // Demonstrates ModelBinding a Item object explicitly from Uri
        public ActionValueItem GetItemFromUri([FromUri] ActionValueItem item)
        {
            return item;
        }

        // Demonstrates use of renaming parameters via name
        public ActionValueItem GetFromNamed([FromUri(Name = "custID")] int id,
                                     [FromUri(Name = "first")] string firstName,
                                     [FromUri(Name = "last")] string lastName)
        {
            return new ActionValueItem() { Id = id, FirstName = firstName, LastName = lastName };
        }


        public void GetTestEmptyString([FromUri] ConvertEmptyStringContainer x)
        {
        }

        // Demonstrates use of custom ValueProvider via attribute
        public ActionValueItem GetFromCustom([ValueProvider(typeof(ActionValueControllerValueProviderFactory), Name = "id")] int id,
                                      [ValueProvider(typeof(ActionValueControllerValueProviderFactory), Name = "customFirstName")] string firstName,
                                      [ValueProvider(typeof(ActionValueControllerValueProviderFactory), Name = "customLastName")] string lastName)
        {
            return new ActionValueItem() { Id = id, FirstName = firstName, LastName = lastName };
        }

        // Demonstrates ModelBinding to the CancellationToken of the current request
        public string GetFromCancellationToken(CancellationToken cancellationToken)
        {
            return cancellationToken.ToString();
        }

        // Demonstrates ModelBinding to the ModelState of the current request
        public string GetFromModelState(ModelState modelState)
        {
            return modelState.ToString();
        }

        // Demonstrates binding to complex type from body
        public ActionValueItem PostComplexType(ActionValueItem item)
        {
            return item;
        }

        // Demonstrates binding to complex type from uri
        public ActionValueItem PostComplexTypeUri([FromUri] ActionValueItem item)
        {
            return item;
        }

        // Demonstrates binding to IEnumerable of complex type from body or Uri
        public ActionValueItem PostEnumerable(IEnumerable<ActionValueItem> items)
        {
            return items.FirstOrDefault();
        }

        // Demonstrates binding to IEnumerable of complex type from body or Uri
        public ActionValueItem PostEnumerableUri([FromUri] IEnumerable<ActionValueItem> items)
        {
            return items.FirstOrDefault();
        }

        // Demonstrates binding to JsonValue from body
        public JToken PostJsonValue(JToken jsonValue)
        {
            return jsonValue;
        }

        // Demonstrate what we expect to be the common default scenario. No attributes are required.
        // A complex object comes from the body, and simple objects come from the URI.
        public ActionValueItem PostFromBodyAndUri(int id, ActionValueItem item)
        {
            return item;
        }

        // Demonstrates binding to complex type explicitly marked as coming from body
        public ActionValueItem PostFromBody([FromBody] ActionValueItem item)
        {
            return item;
        }

        // Demonstrates how body can be shredded to name/value pairs to bind to simple types
        public ActionValueItem PostToSimpleTypes(int id, string firstName, string lastName)
        {
            return new ActionValueItem() { Id = id, FirstName = firstName, LastName = lastName };
        }

        // Demonstrates binding to ObjectContent<T> from request body
        public Task<ActionValueItem> PostObjectContentOfItemAsync(ObjectContent<ActionValueItem> item)
        {
            return item.ReadAsAsync<ActionValueItem>();
        }

        public class ActionValueControllerValueProviderFactory : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                return new ActionValueControllerValueProvider();
            }
        }

        public class ActionValueControllerValueProvider : IValueProvider
        {
            public bool ContainsPrefix(string prefix)
            {
                return true;
            }

            public ValueProviderResult GetValue(string key)
            {
                return new ValueProviderResult("99", "99", CultureInfo.CurrentCulture);
            }
        }
    }

    static class DefaultActionValueBinderExtensions
    {
        public static Task BindValuesAsync(this DefaultActionValueBinder binder, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpActionBinding binding = binder.GetBinding(actionContext.ActionDescriptor);
            return binding.ExecuteBindingAsync(actionContext, cancellationToken);
        }
    }

    public class ActionValueItem
    {
        [Range(0, 99)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    // Test variants of converting empty string to null.
    // Pass each property the empty string.
    public class ConvertEmptyStringContainer
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string A1 { get; set; } // ""

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public string A2 { get; set; } // Null

        [DisplayFormat]
        public string A3 { get; set; } // Null

        public string A4 { get; set; } // Null
    }
}
