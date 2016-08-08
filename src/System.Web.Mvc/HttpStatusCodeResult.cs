// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;

namespace System.Web.Mvc
{
    public class HttpStatusCodeResult : ActionResult
    {
        public HttpStatusCodeResult(int statusCode)
            : this(statusCode, null)
        {
        }

        public HttpStatusCodeResult(HttpStatusCode statusCode)
            : this(statusCode, null)
        {
        }

        public HttpStatusCodeResult(HttpStatusCode statusCode, string statusDescription)
            : this((int)statusCode, statusDescription)
        {
        }

        public HttpStatusCodeResult(int statusCode, string statusDescription)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        public int StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.HttpContext.Response.StatusCode = StatusCode;
            if (StatusDescription != null)
            {
                context.HttpContext.Response.StatusDescription = StatusDescription;
            }
        }
    }
}
