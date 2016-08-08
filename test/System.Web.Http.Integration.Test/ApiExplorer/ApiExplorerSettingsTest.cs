// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class ApiExplorerSettingsTest
    {
        public static IEnumerable<object[]> HiddenController_DoesNotShowUpOnDescription_PropertyData
        {
            get
            {
                object controllerType = typeof(HiddenController);
                object expectedApiDescriptions = new List<object>();
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("HiddenController_DoesNotShowUpOnDescription_PropertyData")]
        public void HiddenController_DoesNotShowUpOnDescription(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }

        public static IEnumerable<object[]> HiddenAction_DoesNotShowUpOnDescription_PropertyData
        {
            get
            {
                object controllerType = typeof(HiddenActionController);
                object expectedApiDescriptions = new List<object>
                {
                    new { HttpMethod = HttpMethod.Get, RelativePath = "HiddenAction/{id}", HasRequestFormatters = false, HasResponseFormatters = true, NumberOfParameters = 1}
                };
                yield return new[] { controllerType, expectedApiDescriptions };
            }
        }

        [Theory]
        [PropertyData("HiddenAction_DoesNotShowUpOnDescription_PropertyData")]
        public void HiddenAction_DoesNotShowUpOnDescription(Type controllerType, List<object> expectedResults)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerType);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiExplorerHelper.VerifyApiDescriptions(explorer.ApiDescriptions, expectedResults);
        }
    }
}
