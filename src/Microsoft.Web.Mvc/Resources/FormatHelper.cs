// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Mime;
using System.Web;
using System.Web.Routing;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Base class for content negotiation support
    /// </summary>
    public abstract class FormatHelper
    {
        /// <summary>
        /// Returns the ContentType of a given request.
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns>The format of the request.</returns>
        /// <exception cref="HttpException">If the format is unrecognized or not supported.</exception>
        public abstract ContentType GetRequestFormat(RequestContext requestContext);

        /// <summary>
        /// Returns a collection of ContentType instances that can be used to render a response to a given request, sorted in priority order.
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns>The formats to use for rendering a response.</returns>
        public abstract IEnumerable<ContentType> GetResponseFormats(RequestContext requestContext);

        /// <summary>
        /// Determines whether the specified HTTP request was sent by a Browser.
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns></returns>
        public abstract bool IsBrowserRequest(RequestContext requestContext);
    }
}
