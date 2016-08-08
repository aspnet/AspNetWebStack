// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.Cors.Test.Controllers
{
    public class ExternalActionSelectorAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ExternalActionSelector());
        }

        private class ExternalActionSelector : IHttpActionSelector
        {
            public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
            {
                HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor
                {
                    ControllerName = "Sample",
                    ControllerType = typeof(SampleController)
                };
                Action action = new SampleController().Head;
                return new ReflectedHttpActionDescriptor(controllerDescriptor, action.Method);
            }

            public ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
            {
                List<HttpActionDescriptor> descriptors = new List<HttpActionDescriptor>();
                return descriptors.ToLookup(d => d.ActionName);
            }
        }
    }
}