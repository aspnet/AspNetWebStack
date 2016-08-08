// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Routing;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages.ApplicationParts
{
    internal class ResourceRouteHandler : IRouteHandler
    {
        private ApplicationPartRegistry _partRegistry;

        public ResourceRouteHandler(ApplicationPartRegistry partRegistry)
        {
            _partRegistry = partRegistry;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // Get the package name and static resource path from the route
            string partName = (string)requestContext.RouteData.GetRequiredString("module");

            // Try to find an application module by this name
            ApplicationPart module = _partRegistry[partName];

            // Throw an exception if we can't find the module by name
            if (module == null)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  WebPageResources.ApplicationPart_ModuleCannotBeFound, partName));
            }

            // Get the resource path
            string path = (string)requestContext.RouteData.GetRequiredString("path");

            // Return the resource handler for this module and path
            return new ResourceHandler(module, path);
        }
    }
}
