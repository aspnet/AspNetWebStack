// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Facebook-related extension methods for <see cref="HtmlHelper"/>.
    /// </summary>
    public static class FacebookHtmlHelperExtensions
    {
        /// <summary>
        /// Returns the signed_request in a hidden input element.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="HtmlHelper"/>.</param>
        /// <returns>The signed_request in a hidden input element.</returns>
        public static IHtmlString FacebookSignedRequest(this HtmlHelper htmlHelper)
        {
            string signedRequest = htmlHelper.ViewContext.HttpContext.Request.Params["signed_request"];
            if (!String.IsNullOrEmpty(signedRequest))
            {
                return htmlHelper.Hidden("signed_request", signedRequest);
            }
            return new HtmlString(String.Empty);
        }
    }
}