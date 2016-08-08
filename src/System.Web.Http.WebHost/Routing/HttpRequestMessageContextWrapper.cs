// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Net.Http;

namespace System.Web.Http.WebHost.Routing
{
    internal class HttpRequestMessageContextWrapper : HttpContextBase
    {
        private HttpRequestMessageWrapper _httpWrapper;

        // Using Hashtable to be consistent with HttpContext.Items
        private Hashtable _items;

        public HttpRequestMessageContextWrapper(string virtualPathRoot, HttpRequestMessage httpRequest)
        {
            _httpWrapper = new HttpRequestMessageWrapper(virtualPathRoot, httpRequest);
            _items = new Hashtable();
        }

        public override HttpRequestBase Request
        {
            get { return _httpWrapper; }
        }

        public override IDictionary Items
        {
            get { return _items; }
        }
    }
}