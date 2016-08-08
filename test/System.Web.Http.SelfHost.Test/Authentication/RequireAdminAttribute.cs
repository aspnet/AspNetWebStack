// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http
{
    public class RequireAdminAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext context)
        {
            // do authorization based on the principle.
            IPrincipal principal = Thread.CurrentPrincipal;
            if (principal == null || !principal.IsInRole("Administrators"))
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }
    }
}