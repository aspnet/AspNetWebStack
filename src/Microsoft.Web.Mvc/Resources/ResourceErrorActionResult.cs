// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Action result for returning HTTP errors that result from performing operations on resources, including
    /// an optional details in the HTTP body
    /// </summary>
    public class ResourceErrorActionResult : ActionResult
    {
        private object details;
        private ContentType responseFormat;
        private HttpStatusCode statusCode;

        /// <summary>
        /// Sends back a response using the status code in the HttpException.
        /// The response body contains a details serialized in the responseFormat.
        /// If the HttpException.Data has a key named "details", its value is used as the response body.
        /// If there is no such key, HttpException.ToString() is used as the response body.
        /// </summary>
        /// <param name="httpException"></param>
        /// <param name="responseFormat"></param>
        public ResourceErrorActionResult(HttpException httpException, ContentType responseFormat)
        {
            this.statusCode = (HttpStatusCode)httpException.GetHttpCode();
            this.details = httpException.Data.Contains("details") ? httpException.Data["details"] : httpException.ToString();
            this.responseFormat = responseFormat;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (this.details != null)
            {
                MultiFormatActionResult rar = new MultiFormatActionResult(this.details, this.responseFormat, this.statusCode);
                if (rar.TryExecuteResult(context, this.details, this.responseFormat))
                {
                    return;
                }
            }
            context.HttpContext.Response.ClearContent();
            context.HttpContext.Response.StatusCode = (int)this.statusCode;
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
