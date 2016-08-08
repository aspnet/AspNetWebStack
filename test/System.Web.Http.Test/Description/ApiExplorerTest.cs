// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Description
{
    public class ApiExplorerTest
    {
        [Fact]
        public void Descriptions_RecognizesDirectRoutes()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "ApiExplorerValues", typeof(ApiExplorerValuesController));
            var action = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Get"));
            var actions = new ReflectedHttpActionDescriptor[] { action };
            config.Routes.Add("Route", CreateDirectRoute(routeTemplate, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
            Assert.Equal(action, description.ActionDescriptor);
        }

        [Fact]
        public void Descriptions_RecognizesIgnoreApiForDirectRoutes_Action()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "ApiExplorerValues", typeof(ApiExplorerValuesController));
            var actions = new ReflectedHttpActionDescriptor[] 
            {
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Get")),
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Post")),
            };
            config.Routes.Add("Route", CreateDirectRoute(routeTemplate, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
        }

        [Fact]
        public void Descriptions_RecognizesIgnoreApiForDirectRoutes_Controller()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "IgnoreApiValues", typeof(IgnoreApiValuesController));
            var actions = new ReflectedHttpActionDescriptor[] 
            {
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(IgnoreApiValuesController).GetMethod("Get")),
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(IgnoreApiValuesController).GetMethod("Post")),
            };
            config.Routes.Add("Route", CreateDirectRoute(routeTemplate, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            Assert.Empty(descriptions);
        }

        public class ApiExplorerValuesController : ApiController
        {
            public void Get() { }

            [ApiExplorerSettings(IgnoreApi = true)]
            public void Post() { }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public class IgnoreApiValuesController : ApiController
        {
            public void Get() { }
            public void Post() { }
        }

        [Fact]
        public void Descriptions_RecognizesCompositeRoutes()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "AttributeApiExplorerValues", typeof(AttributeApiExplorerValuesController));
            var action = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(AttributeApiExplorerValuesController).GetMethod("Action"));
            var actions = new ReflectedHttpActionDescriptor[] { action };

            var routeCollection = new List<IHttpRoute>();
            routeCollection.Add(CreateDirectRoute(routeTemplate, actions));

            RouteCollectionRoute route = new RouteCollectionRoute();
            route.EnsureInitialized(() => routeCollection);

            config.Routes.Add("Route", route);

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
            Assert.Equal(action, description.ActionDescriptor);
        }

        [Fact]
        public void TryExpandUriParameters_EnsureNoKeyConflicts()
        {
            // This test ensures that keys adding to parameterValuesForRoute are case-insensitive
            // and would not cause any exeception if it already has the key. So set up two
            // ApiParameterDescription instances, one with "id" and another with "Id". Act the
            // method and assert that no exception occurs and the output is correct.
            // Arrange
            string expectedExpandedRouteTemplate = "?id={id}";
            string expandedRouteTemplate;
            Mock<HttpParameterDescriptor> parameterDescriptorMock = new Mock<HttpParameterDescriptor>();
            parameterDescriptorMock.SetupGet(p => p.ParameterType).Returns(typeof(ClassWithId));
            List<ApiParameterDescription> descriptions = new List<ApiParameterDescription>()
            {
                new ApiParameterDescription()
                {
                    Source = ApiParameterSource.FromUri,
                    Name = "id"
                },
                new ApiParameterDescription()
                {
                    Source = ApiParameterSource.FromUri,
                    ParameterDescriptor = parameterDescriptorMock.Object
                },
            };

            // Act
            bool isExpanded = ApiExplorer.TryExpandUriParameters(new HttpRoute(),
                                                     new HttpParsedRoute(new List<PathSegment>()),
                                                     descriptions,
                                                     out expandedRouteTemplate);

            // Assert
            Assert.True(isExpanded);
            Assert.Equal(expectedExpandedRouteTemplate, expandedRouteTemplate);
        }


        [Theory]
        // Simple type
        [InlineData("?id={id}", typeof(int), "id")]
        // Simple Array and Collection
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(int[]), "id")]
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(string[]), "id")]
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(IList<string>), "id")]
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(List<string>), "id")]
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(IEnumerable<string>), "id")]
        [InlineData("?id[0]={id[0]}&id[1]={id[1]}", typeof(ICollection<int>), "id")]
        // Complex Array and Collection
        [InlineData("?users[0].Name={users[0].Name}&users[0].Age={users[0].Age}" +
                        "&users[1].Name={users[1].Name}&users[1].Age={users[1].Age}",
                    typeof(IEnumerable<User>),
                    "users")]
        [InlineData("?users[0].Name={users[0].Name}&users[0].Age={users[0].Age}" +
                        "&users[1].Name={users[1].Name}&users[1].Age={users[1].Age}",
                    typeof(User[]),
                    "users")]
        // MutableObject
        [InlineData("?Foo={Foo}&Bar={Bar}", typeof(MutableObject), "mutable")]
        // KeyValuePair
        [InlineData("?key={key}&value={value}", typeof(KeyValuePair<string, string>), "pair")]
        // Dictionary
        [InlineData("?dict[0].key={dict[0].key}&dict[0].value={dict[0].value}&dict[1].key={dict[1].key}" +
                        "&dict[1].value={dict[1].value}",
                    typeof(Dictionary<string, string>),
                    "dict")]
        // MutableObject extending IList<> does not generate query string samples like ?id[0]={id[0]}&id[1]={id[1]}.
        // Note that the "Item" query string in the following is not valid,
        // which is a bug but will happen in rare cases.
        [InlineData("?Foo={Foo}&Bar={Bar}&Capacity={Capacity}&Item={Item}",
                    typeof(GenericMutableObject<string>),
                    "genericMutable")]
        public void TryExpandUriParameters_FromUri_Succeeds(string expectedPath,
                                                                      Type parameterType,
                                                                      string parameterName)
        {
            // Arrange
            string finalPath;
            List<ApiParameterDescription> descriptions = new List<ApiParameterDescription>()
            {
                CreateApiParameterDescription(parameterType, parameterName),
            };

            // Act
            bool isExpanded = ApiExplorer.TryExpandUriParameters(new HttpRoute(),
                                                     new HttpParsedRoute(new List<PathSegment>()),
                                                     descriptions,
                                                     out finalPath);

            // Assert
            Assert.True(isExpanded);
            Assert.Equal(expectedPath, finalPath);
        }

        [Fact]
        public void TryExpandUriParameters_CompositeParametersFromUri_Succeeds()
        {
            // Arrange
            string finalPath;
            const string expectedPath = 
                "?id[0]={id[0]}&id[1]={id[1]}&property[0]={property[0]}&property[1]={property[1]}&name={name}";
            List<ApiParameterDescription> descriptions = new List<ApiParameterDescription>()
            {
                CreateApiParameterDescription(typeof(int[]), "id"),
                CreateApiParameterDescription(typeof(ICollection<string>), "property"),
                CreateApiParameterDescription(typeof(string), "name"),
            };

            // Act
            bool isExpanded = ApiExplorer.TryExpandUriParameters(new HttpRoute(),
                                                     new HttpParsedRoute(new List<PathSegment>()),
                                                     descriptions,
                                                     out finalPath);

            // Assert
            Assert.True(isExpanded);
            Assert.Equal(expectedPath, finalPath);
        }

        [Fact]
        public void Descriptions_RecognizesMixedCaseParameters()
        {
            // Ensure that two "Id"s, one from "api/values/{id}" and another "Id" from ClassWithId,
            // would not cause any exception and only one of them is added.
            var config = new HttpConfiguration();
            var routeTemplate = "api/values/{id}";
            var controllerDescriptor = new HttpControllerDescriptor(config, "ApiExplorerValues", typeof(DuplicatedIdController));
            var action = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(DuplicatedIdController).GetMethod("Get"));
            var actions = new ReflectedHttpActionDescriptor[] { action };
            config.Routes.Add("Route", CreateDirectRoute(routeTemplate, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(action, description.ActionDescriptor);
        }

        private class ClassWithId
        {
            public int Id { get; set; }
        }

        private class DuplicatedIdController : ApiController
        {
            public void Get([FromUri] ClassWithId objectWithId) { }
        }

        public class AttributeApiExplorerValuesController : ApiController
        {
            [Route("")]
            [HttpGet]
            public void Action() { }
        }

        private static IHttpRoute CreateDirectRoute(string template,
            IReadOnlyCollection<ReflectedHttpActionDescriptor> actions)
        {
            DirectRouteBuilder builder = new DirectRouteBuilder(actions, targetIsAction: true);
            builder.Template = template;
            return builder.Build().Route;
        }

        private ApiParameterDescription CreateApiParameterDescription(Type type, string name)
        {
            Mock<HttpParameterDescriptor> parameterDescriptorMock = new Mock<HttpParameterDescriptor>();
            parameterDescriptorMock.SetupGet(p => p.ParameterName).Returns(name);
            parameterDescriptorMock.SetupGet(p => p.ParameterType).Returns(type);
            return new ApiParameterDescription()
            {
                Source = ApiParameterSource.FromUri,
                ParameterDescriptor = parameterDescriptorMock.Object,
                Name = name
            };
        }

        private class MutableObject
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        private class GenericMutableObject<T> : List<T>
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        private class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
