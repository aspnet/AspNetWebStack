// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class RoutesTest
    {
        [Fact]
        public void VerifyDescription_OnEmptyRoute()
        {
            HttpConfiguration config = new HttpConfiguration();
            IApiExplorer explorer = config.Services.GetApiExplorer();

            Assert.NotNull(explorer);
            Assert.Empty(explorer.ApiDescriptions);
        }

        [Fact]
        public void VerifyDescription_OnInvalidRoute()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Invalid", "badRouteWithNoControler");
            IApiExplorer explorer = config.Services.GetApiExplorer();

            Assert.NotNull(explorer);
            Assert.Empty(explorer.ApiDescriptions);
        }

        public static IEnumerable<object[]> VerifyDescription_OnDefaultRoute_PropertyData
        {
            get
            {
                object controllerType = typeof(ItemController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Item?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Item/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(OverloadsController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads?name={name}&age={age}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/{id}?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Overloads", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Overloads?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Overloads/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Overloads/{id}?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 3}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_OnDefaultRoute_PropertyData")]
        public void VerifyDescription_OnDefaultRoute(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> VerifyDescription_OnRouteWithControllerOnDefaults_PropertyData
        {
            get
            {
                object controllerType = typeof(ItemController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "myitem?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "myitem", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "myitem", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "myitem/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_OnRouteWithControllerOnDefaults_PropertyData")]
        public void VerifyDescription_OnRouteWithControllerOnDefaults(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "myitem/{id}", new { controller = "Item", id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> VerifyDescription_OnRouteWithActionVariable_PropertyData
        {
            get
            {
                object controllerType = typeof(ItemController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Item/GetItem?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Item/PostItem", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "Item/PostItem", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Item/RemoveItem/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(OverloadsController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/Get/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/Get", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/Get?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndId/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndAge?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAgeAndSsn?name={name}&age={age}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameIdAndSsn/{id}?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndSsn?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Overloads/Post", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Overloads/ActionDefaultedToPost?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Overloads/Delete/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Overloads/Delete/{id}?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 3}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_OnRouteWithActionVariable_PropertyData")]
        public void VerifyDescription_OnRouteWithActionVariable(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> VerifyDescription_On_RouteWithActionOnDefaults_PropertyData
        {
            get
            {
                object controllerType = typeof(ItemController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Item/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_On_RouteWithActionOnDefaults_PropertyData")]
        public void VerifyDescription_On_RouteWithActionOnDefaults(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { action = "RemoveItem", id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        [Fact]
        public void InvalidActionNameOnRoute_DoesNotThrow()
        {
            Type controllerType = typeof(OverloadsController);
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { action = "ActionThatDoesNotExist", id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Assert.Empty(explorer.ApiDescriptions);
        }

        [Fact]
        public void InvalidControllerNameOnRoute_DoesNotThrow()
        {
            Type controllerType = typeof(OverloadsController);
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "mycontroller/{id}", new { controller = "ControllerThatDoesNotExist", id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Assert.Empty(explorer.ApiDescriptions);
        }

        [Fact]
        public void VerifyOnlyOneSetOfDescriptionIsGenerated_OnTwoMatchingRoutes()
        {
            Type controllerType = typeof(ItemController);
            HttpConfiguration config = new HttpConfiguration();
            IHttpRoute matchingRoute = config.Routes.MapHttpRoute("Item", "Item/{id}", new { id = RouteParameter.Optional, controller = "Item" });
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Collection<ApiDescription> descriptions = explorer.ApiDescriptions;

            foreach (ApiDescription description in descriptions)
            {
                Assert.Same(matchingRoute, description.Route);
            }
            List<object> expectedResults = new List<object>
            {
                new { HttpMethod = HttpMethod.Get, RelativePath = "Item?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                new { HttpMethod = HttpMethod.Post, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                new { HttpMethod = HttpMethod.Put, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                new { HttpMethod = HttpMethod.Delete, RelativePath = "Item/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
            };
            ApiExplorerHelper.VerifyApiDescriptions(descriptions, expectedResults);
        }

        [Fact]
        public void VerifyDescriptionIsNotGeneratedForAmbiguousAction_OnDefaultRoutes()
        {
            Type controllerType = typeof(AmbiguousActionController);
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Collection<ApiDescription> descriptions = explorer.ApiDescriptions;

            List<object> expectedResults = new List<object>
            {
                new { HttpMethod = HttpMethod.Post, RelativePath = "AmbiguousAction/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1}
            };
            ApiExplorerHelper.VerifyApiDescriptions(descriptions, expectedResults);
        }

        [Fact]
        public void VerifyDescriptionIsGenerated_WhenRouteParameterIsNotInAction()
        {
            Type controllerType = typeof(ItemController);
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Custom", "{majorVersion}/{minorVersion}/custom", new { controller = "Item" });
            config.Routes.MapHttpRoute("Default", "{version}/{controller}/{id}", new { id = RouteParameter.Optional });
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Collection<ApiDescription> descriptions = explorer.ApiDescriptions;

            List<object> expectedResults = new List<object>
            {
                new { HttpMethod = HttpMethod.Get, RelativePath = "{majorVersion}/{minorVersion}/custom?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 4},
                new { HttpMethod = HttpMethod.Post, RelativePath = "{majorVersion}/{minorVersion}/custom", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 3},
                new { HttpMethod = HttpMethod.Put, RelativePath = "{majorVersion}/{minorVersion}/custom", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 3},
                new { HttpMethod = HttpMethod.Delete, RelativePath = "{majorVersion}/{minorVersion}/custom?id={id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 3},
                new { HttpMethod = HttpMethod.Get, RelativePath = "{version}/Item?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                new { HttpMethod = HttpMethod.Post, RelativePath = "{version}/Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 2},
                new { HttpMethod = HttpMethod.Put, RelativePath = "{version}/Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 2},
                new { HttpMethod = HttpMethod.Delete, RelativePath = "{version}/Item/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 2}
            };
            ApiExplorerHelper.VerifyApiDescriptions(descriptions, expectedResults);
        }
    }
}