// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpVirtualPathData : IHttpVirtualPathData
    {
        private readonly VirtualPathData _virtualPath;

        public HostedHttpVirtualPathData(VirtualPathData virtualPath, IHttpRoute httpRoute)
        {
            if (virtualPath == null)
            {
                throw Error.ArgumentNull("route");
            }

            _virtualPath = virtualPath;
            Route = httpRoute;
        }

        public IHttpRoute Route { get; private set; }

        public string VirtualPath
        {
            get { return _virtualPath.VirtualPath; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _virtualPath.VirtualPath = value;
            }
        }
    }
}
