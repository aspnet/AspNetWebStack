// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;

namespace System.Web.Mvc
{
    public class HttpUnauthorizedResult : HttpStatusCodeResult
    {
        public HttpUnauthorizedResult()
            : this(null)
        {
        }

        // Unauthorized is equivalent to HTTP status 401, the status code for unauthorized
        // access. Other code might intercept this and perform some special logic. For
        // example, the FormsAuthenticationModule looks for 401 responses and instead
        // redirects the user to the login page.
        public HttpUnauthorizedResult(string statusDescription)
            : base(HttpStatusCode.Unauthorized, statusDescription)
        {
        }
    }
}
