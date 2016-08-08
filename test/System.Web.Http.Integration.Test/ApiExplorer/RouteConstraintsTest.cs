// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class RouteConstraintsTest
    {
        public static IEnumerable<object[]> HttpMethodConstraints_LimitsTheDescriptions_PropertyData
        {
            get
            {
                object controllerType = typeof(ItemController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Item?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1}
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
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("HttpMethodConstraints_LimitsTheDescriptions_PropertyData")]
        public void HttpMethodConstraints_LimitsTheDescriptions(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional }, new { routeConstraint = new HttpMethodConstraint(HttpMethod.Get, HttpMethod.Put) });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> RegexConstraint_LimitsTheController_PropertyData
        {
            get
            {
                object[] controllerTypes = new Type[] { typeof(OverloadsController), typeof(ItemController) };
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Item?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Post, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Put, RelativePath = "Item", HasRequestFormatters = true, HasResponseFormatters = true, NumberOfParameters = 1},
                    new { HttpMethod = HttpMethod.Delete, RelativePath = "Item/{id}", HasRequestFormatters = false, HasResponseFormatters = false, NumberOfParameters = 1}
                };
                yield return new[] { controllerTypes, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("RegexConstraint_LimitsTheController_PropertyData")]
        public void RegexConstraint_LimitsTheController(Type[] controllerTypes, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional }, new { controller = "It.*" }); // controllers that start with "It"

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerTypes);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> RegexConstraint_LimitsTheAction_PropertyData
        {
            get
            {
                object[] controllerTypes = new Type[] { typeof(OverloadsController), typeof(ItemController) };
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Item/GetItem?name={name}&series={series}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndId/{id}?name={name}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndAge?name={name}&age={age}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAgeAndSsn?name={name}&age={age}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameIdAndSsn/{id}?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 3},
                    new { HttpMethod = HttpMethod.Get, RelativePath = "Overloads/GetPersonByNameAndSsn?name={name}&ssn={ssn}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 2}
                };
                yield return new[] { controllerTypes, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("RegexConstraint_LimitsTheAction_PropertyData")]
        public void RegexConstraint_LimitsTheAction(Type[] controllerTypes, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { id = RouteParameter.Optional }, new { action = "Get.+" }); // actions that start with "Get" and at least one extra character

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerTypes);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }
    }
}
