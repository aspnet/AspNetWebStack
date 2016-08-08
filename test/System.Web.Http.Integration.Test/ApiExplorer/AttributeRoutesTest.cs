// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class AttributeRoutesTest
    {
        public static IEnumerable<object[]> VerifyDescription_OnAttributeRoutes_PropertyData
        {
            get
            {
                object controllerType;
                object expectedApiDescriptions;

                controllerType = typeof(MixedController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Post, RelativePath = "attribute/mixed", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(AttributedController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "controller/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "controller/{name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "controller/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "controller/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "optional/{opt1}/{opt2}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "optionalwconstraint/{opt}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "default/{default1}/{default2}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "wildcard/{wildcard}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "multiverb", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "multiverb", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "multi1", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "multi2", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(PrefixedController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "prefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "prefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "prefix/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(DefaultRouteController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "prefix2/defaultroute/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "prefix2", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "prefix2/defaultrouteoverride/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(RpcController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/default2/getallcustomers1", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/default2/getallcustomers2", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/resource/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(PartlyResourcePartlyRpcController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "partial/doop1", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "partial/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(OptionalController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "apioptional", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "apioptional/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(OverloadController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "apioverload/{name}?age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "apioverload/{id}?score={score}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(BaseClassController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "baseclass?id={id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(BaseClassPrefixController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "baseclassprefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "baseclassprefix/base/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(SubClassRouteController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "subclassroute", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "subclassroute?id={id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "subclassroute?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_OnAttributeRoutes_PropertyData")]
        public void VerifyDescription_OnAttributeRoutes(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            config.EnsureInitialized();
            
            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> VerifyDescription_OnMixedRoutes_PropertyData
        {
            get
            {
                object controllerType;
                object expectedApiDescriptions;

                controllerType = typeof(MixedController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/Mixed?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "attribute/mixed", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "api/Mixed/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(PrefixedController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "prefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "prefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "prefix/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "api/prefixed", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 0},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(SubClassController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "subclass?id={id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "subclass?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(SubClassNoRouteController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/subclassnoroute/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "api/subclassnoroute?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(SubClassNoPrefixController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/subclassnoprefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/subclassnoprefix/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "api/subclassnoprefix?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };

                controllerType = typeof(SubClassPrefixController);
                expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/subclassprefix", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 0},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "api/subclassprefix/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "subclassprefix?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1},
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("VerifyDescription_OnMixedRoutes_PropertyData")]
        public void VerifyDescription_OnMixedRoutes(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            config.EnsureInitialized();

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        [Fact]
        public void NoDescription_OnAttributeRoutedAction_UsingStandardControllerRoute()
        {
            HttpConfiguration config = new HttpConfiguration();
            var route = config.Routes.MapHttpRoute("Default", "api/someController", new { controller = "DefaultRoute" });
            config.MapHttpAttributeRoutes();

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(DefaultRouteController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            config.EnsureInitialized();

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Assert.True(explorer.ApiDescriptions.All(d => d.Route != route));
        }

        [Fact]
        public void NoDescription_OnAttributeRoutedAction_UsingStandardRoute()
        {
            HttpConfiguration config = new HttpConfiguration();
            var route = config.Routes.MapHttpRoute("Default", "api/someAction/{id}", new { controller = "Attributed", action = "Get" });
            config.MapHttpAttributeRoutes();

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(AttributedController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            config.EnsureInitialized();

            IApiExplorer explorer = config.Services.GetApiExplorer();
            Assert.True(explorer.ApiDescriptions.All(d => d.Route != route));
        }
    }
}