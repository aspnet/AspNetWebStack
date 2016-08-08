// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that redirects to a permission prompt login via JavaScript.
    /// </summary>
    public class ShowPromptResult : JavaScriptRedirectResult
    {
        /// <summary>
        /// Creates a JavaScript based redirect <see cref="ActionResult"/>.
        /// </summary>
        /// <param name="promptUrl">The url that the prompt exists at.</param>
        public ShowPromptResult(Uri promptUrl)
            : base(promptUrl)
        {
        }
    }
}
