// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.ApiExplorer;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace System.Web.Http
{
    [ControllerConfigAttribute]
    public class SpecialConfigController : ApiController
    {
        public int GetFormattersCount_ControllerConfig()
        {
            return Configuration.Formatters.Count;
        }

        public int GetParameterRulesCount_ControllerConfig()
        {
            return Configuration.ParameterBindingRules.Count;
        }

        public int GetServicesCount_ControllerConfig()
        {
            return Configuration.Services.GetService(typeof(IDocumentationProvider)) == null ? 0 : 1;
        }

        public int GetFormattersCount_RequestConfig()
        {
            return Request.GetConfiguration().Formatters.Count;
        }

        public int GetParameterRulesCount_RequestConfig()
        {
            return Request.GetConfiguration().ParameterBindingRules.Count;
        }

        public int GetServicesCount_RequestConfig()
        {
            return Request.GetConfiguration().Services.GetService(typeof(IDocumentationProvider)) == null ? 0 : 1;
        }

        private class ControllerConfigAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Formatters.Clear();
                controllerSettings.Formatters.Add(new XmlMediaTypeFormatter());

                controllerSettings.ParameterBindingRules.Clear();

                controllerSettings.Services.Replace(typeof(IDocumentationProvider), new AttributeDocumentationProvider());
            }
        }
    }
}
