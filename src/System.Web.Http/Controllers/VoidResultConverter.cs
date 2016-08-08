// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// A converter for creating a response from actions that do not return a value.
    /// </summary>
    public class VoidResultConverter : IActionResultConverter
    {
        public HttpResponseMessage Convert(HttpControllerContext controllerContext, object actionResult)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            Contract.Assert(actionResult == null);
            return controllerContext.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
