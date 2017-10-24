// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class ParameterSourceTest
    {
        [Fact]
        public void FromUriParameterSource_ShowUpCorrectlyOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ParameterSourceController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetCompleTypeFromUri");
            Assert.NotNull(description);
            Assert.True(description.ParameterDescriptions.All(param => param.Source == ApiParameterSource.FromUri), "All parameters should come from URI.");

            description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetCustomFromUriAttribute");
            Assert.NotNull(description);
            Assert.True(description.ParameterDescriptions.Any(param => param.Source == ApiParameterSource.FromUri && param.Name == "value"), "The 'value' parameter should come from URI.");
            Assert.True(description.ParameterDescriptions.Any(param => param.Source == ApiParameterSource.FromBody && param.Name == "bodyValue"), "The 'bodyValue' parameter should come from body.");
        }

        [Fact]
        public void FromBodyParameterSource_ShowUpCorrectlyOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ParameterSourceController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostSimpleTypeFromBody");
            Assert.NotNull(description);
            Assert.True(description.ParameterDescriptions.All(param => param.Source == ApiParameterSource.FromBody), "The parameter should come from Body.");
        }

        [Fact]
        public void UnknownParameterSource_ShowUpCorrectlyOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ParameterSourceController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetFromHeaderAttribute");
            Assert.NotNull(description);
            Assert.True(description.ParameterDescriptions.All(param => param.Source == ApiParameterSource.Unknown), "The parameter source should be Unknown.");
        }

        [Fact]
        public void EnumParameters_ShowUpCorrectlyOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}");
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(EnumParameterOverloadsController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetWithEnumParameter");
            Assert.NotNull(description);
            ApiParameterDescription parameterDescription = Assert.Single(description.ParameterDescriptions);
            Assert.Equal(ApiParameterSource.FromUri, parameterDescription.Source);
            Assert.Equal("EnumParameterOverloads?scope={scope}", description.RelativePath);

            description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetWithTwoEnumParameters");
            Assert.NotNull(description);
            Assert.Equal(2, description.ParameterDescriptions.Count);
            Assert.Equal(ApiParameterSource.FromUri, description.ParameterDescriptions[0].Source);
            Assert.Equal(ApiParameterSource.FromUri, description.ParameterDescriptions[1].Source);
            Assert.Equal("EnumParameterOverloads?level={level}&kind={kind}", description.RelativePath);

            description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "GetWithNullableEnumParameter");
            Assert.NotNull(description);
            parameterDescription = Assert.Single(description.ParameterDescriptions);
            Assert.Equal(ApiParameterSource.FromUri, parameterDescription.Source);
            Assert.Equal("EnumParameterOverloads?level={level}", description.RelativePath);
        }

        [Theory]
        [InlineData("api/values/{id}", "", "Get")]
        [InlineData("api/values", "?value={value}", "GetFromUri")]
        [InlineData("api/values", "?X={X}&Y={Y}", "GetPoint")]
        [InlineData("api/values", "?origin.X={origin.X}&origin.Y={origin.Y}&end.X={end.X}&end.Y={end.Y}", "GetDistance")]
        [InlineData("api/values", "?Latitude={Latitude}&Longitude={Longitude}", "GetLocation")]
        [InlineData("api/values", "?value={value}", "GetConvertible")]
        [InlineData("api/values", "", "GetNoDescribable")]
        [InlineData("api/values", "?X={X}", "GetParticle")]
        public void RelativePath_IsCorrectForTypesFromUri(string routeTemplate, string expectedQueryString, string methodName)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", routeTemplate, new { controller = "ApiExplorerActionsWithParameters", action = methodName });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ApiExplorerActionsWithParametersController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            // Act
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == methodName);

            // Assert
            Assert.NotNull(description);
            Assert.Equal(routeTemplate + expectedQueryString, description.RelativePath);
        }

        public class ApiExplorerActionsWithParametersController : ApiController
        {
            public void Get(int id) { }
            public void GetFromUri([FromUri]string value) { }
            public void GetPoint([FromUri] Point complexParameterObject) { }
            public void GetDistance([FromUri] Point origin, [FromUri] Point end) { }
            public void GetLocation(Location location) { }
            public void GetConvertible([FromUri] ConvertibleFromString value) { }
            public void GetNoDescribable([FromUri] NonDescribable nonDescribable) { }
            public void GetParticle([FromUri] Particle particle) { }
        }

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        [FromUri]
        public class Location
        {
            public int Latitude { get; set; }
            public int Longitude { get; set; }
            public static Location Greenwich { get; set; }
        }

        [TypeConverter(typeof(ConvertibleFromStringConverter))]
        public class ConvertibleFromString
        {
            public class ConvertibleFromStringConverter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return sourceType == typeof(string);
                }
            }
            public int Id { get; set; }
            public string Value { get; set; }
        }

        // We only support complex types whose all of their individual
        // properties can be converted from string.
        public class NonDescribable
        {
            public Point Point { get; set; }
        }

        // Get only, Set only, and private properties are ignored.
        public class Particle
        {
            public int X { get; set; }
            public int Y { get; private set; }
            public int Z
            {
                get
                {
                    return 0;
                }
            }
            private int _mass;
            public int Mass
            {
                set
                {
                    _mass = value;
                }
            }
            private string Color { get; set; }
        }
    }
}