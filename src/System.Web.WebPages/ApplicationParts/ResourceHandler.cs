// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.WebPages.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.ApplicationParts
{
    // Used to serve static resource files (e.g. .jpg, .css, .js) that live inside appliaction modules
    internal class ResourceHandler : IHttpHandler
    {
        private readonly string _path;
        private readonly ApplicationPart _applicationPart;

        public ResourceHandler(ApplicationPart applicationPart, string path)
        {
            if (applicationPart == null)
            {
                throw new ArgumentNullException("applicationPart");
            }

            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }

            _applicationPart = applicationPart;
            _path = path;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpResponseWrapper(context.Response));
        }

        internal void ProcessRequest(HttpResponseBase response)
        {
            string virtualPath = _path;

            // Make sure it starts with ~/
            if (!virtualPath.StartsWith("~/", StringComparison.Ordinal))
            {
                virtualPath = "~/" + virtualPath;
            }

            // Get the resource stream for this virtual path
            using (var stream = _applicationPart.GetResourceStream(virtualPath))
            {
                if (stream == null)
                {
                    throw new HttpException(404, String.Format(
                        CultureInfo.CurrentCulture,
                        WebPageResources.ApplicationPart_ResourceNotFound, _path));
                }

                // Set the mime type based on the file extension
                response.ContentType = MimeMapping.GetMimeMapping(virtualPath);

                // Copy it to the response
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}
