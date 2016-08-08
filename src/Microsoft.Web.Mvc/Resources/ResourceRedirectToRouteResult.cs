// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Augments the RedirectToRouteResult behavior by sending Created HTTP status code in responses to POST, OK HTTP status code otherwise
    /// </summary>
    internal class ResourceRedirectToRouteResult : ActionResult
    {
        private RedirectToRouteResult inner;

        public ResourceRedirectToRouteResult(RedirectToRouteResult inner)
        {
            this.inner = inner;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            // call the base which we expect to be setting the Location header
            this.inner.ExecuteResult(context);

            if (!context.RequestContext.IsBrowserRequest())
            {
                // on POST we return Created, otherwise (EG: DELETE) we return OK
                context.HttpContext.Response.ClearContent();
                context.HttpContext.Response.StatusCode = context.HttpContext.Request.IsHttpMethod(HttpVerbs.Post, true) ? (int)HttpStatusCode.Created : (int)HttpStatusCode.OK;
            }
        }
    }
}
