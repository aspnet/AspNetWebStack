// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Routing
{
    public class HttpRouteData : IHttpRouteData
    {
        private IHttpRoute _route;
        private IDictionary<string, object> _values;

        public HttpRouteData(IHttpRoute route)
            : this(route, new HttpRouteValueDictionary())
        {
        }

        public HttpRouteData(IHttpRoute route, HttpRouteValueDictionary values)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            _route = route;
            _values = values;
        }

        public IHttpRoute Route
        {
            get { return _route; }
        }

        public IDictionary<string, object> Values
        {
            get { return _values; }
        }
    }
}
