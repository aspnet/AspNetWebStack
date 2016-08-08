// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Razor;
using System.Web.WebPages.Razor;

namespace System.Web.Mvc
{
    public class MvcWebRazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            WebPageRazorHost host = base.CreateHost(virtualPath, physicalPath);

            if (!host.IsSpecialPage)
            {
                return new MvcWebPageRazorHost(virtualPath, physicalPath);
            }

            return host;
        }
    }
}
