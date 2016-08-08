// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;

namespace System.Web.Mvc
{
    public class HttpNotFoundResult : HttpStatusCodeResult
    {
        public HttpNotFoundResult()
            : this(null)
        {
        }

        // NotFound is equivalent to HTTP status 404.
        public HttpNotFoundResult(string statusDescription)
            : base(HttpStatusCode.NotFound, statusDescription)
        {
        }
    }
}
