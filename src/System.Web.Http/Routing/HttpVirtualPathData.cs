// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    public class HttpVirtualPathData : IHttpVirtualPathData
    {
        private string _virtualPath;

        public HttpVirtualPathData(IHttpRoute route, string virtualPath)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (virtualPath == null)
            {
                throw Error.ArgumentNull("virtualPath");
            }

            Route = route;
            VirtualPath = virtualPath;
        }

        public IHttpRoute Route { get; private set; }

        public string VirtualPath
        {
            get { return _virtualPath; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _virtualPath = value;
            }
        }
    }
}
