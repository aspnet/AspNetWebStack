// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.WebHost.Routing;

namespace System.Web.Http.WebHost
{
    internal class HttpBatchContextWrapper : HttpContextBase
    {
        private HttpRequestMessageWrapper _httpRequestWrapper;
        private HttpContextBase _httpContextBase;
        private Hashtable _items;

        public HttpBatchContextWrapper(HttpContextBase httpContext, HttpRequestMessage httpRequest)
        {
            _httpContextBase = httpContext;
            _items = new Hashtable();
            _httpRequestWrapper = new HttpRequestMessageWrapper(httpContext.Request.ApplicationPath, httpRequest);
        }

        public override HttpRequestBase Request
        {
            get { return _httpRequestWrapper; }
        }

        public override HttpResponseBase Response
        {
            get { return _httpContextBase.Response; }
        }

        public override IDictionary Items
        {
            get { return _items; }
        }

        public override IPrincipal User
        {
            get
            {
                return _httpContextBase.User;
            }
            set
            {
                _httpContextBase.User = value;
            }
        }
    }
}