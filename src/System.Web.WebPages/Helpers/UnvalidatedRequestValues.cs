// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Helpers
{
    // Provides access to Request.* collections, except that these have not gone through request validation.
    [Obsolete("Use System.Web.HttpRequest.Unvalidated instead.")]
    public sealed class UnvalidatedRequestValues
    {
        private readonly HttpRequestBase _request;

        internal UnvalidatedRequestValues(HttpRequestBase request)
        {
            _request = request;
        }

        public NameValueCollection Form
        {
            get { return _request.Unvalidated.Form; }
        }

        public NameValueCollection QueryString
        {
            get { return _request.Unvalidated.QueryString; }
        }

        public string this[string key]
        {
            get
            {
                return _request.Unvalidated[key];
            }
        }
    }
}