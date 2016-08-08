// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web.Helpers.Resources;
using System.Web.WebPages;

namespace System.Web.Helpers
{
    internal static class VirtualPathUtil
    {
        /// <summary>
        /// Resolves and maps a path (physical or virtual) to a physical path on the server. 
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContextBase"/>.</param>
        /// <param name="path">Either a physical rooted path or a virtual path to be mapped.
        /// Physical paths are returned without modifications. Virtual paths are resolved relative to the current executing page.
        /// </param>
        /// <remarks>Result of this call should not be shown to the user (e.g. in an exception message) since
        /// it could be security sensitive. But we need to pass this result to the file APIs like File.WriteAllBytes
        /// which will show it if exceptions are raised from them. Unfortunately VirtualPathProvider doesn't have
        /// APIs for writing so we can't use that.</remarks>
        public static string MapPath(HttpContextBase httpContext, string path)
        {
            Debug.Assert(!String.IsNullOrEmpty(path));

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            // There is no TryMapPath API so we have to catch HttpException if we want to
            // throw ArgumentException instead.  
            try
            {
                return httpContext.Request.MapPath(ResolvePath(TemplateStack.GetCurrentTemplate(httpContext), httpContext, path));
            }
            catch (HttpException)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.InvariantCulture, HelpersResources.PathUtils_IncorrectPath, path), "path");
            }
        }

        /// <summary>
        /// Resolves path relative to the current executing page
        /// </summary>
        public static string ResolvePath(string virtualPath)
        {
            if (String.IsNullOrEmpty(virtualPath))
            {
                return virtualPath;
            }

            if (HttpContext.Current == null)
            {
                return virtualPath;
            }
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            return ResolvePath(TemplateStack.GetCurrentTemplate(httpContext), httpContext, virtualPath);
        }

        internal static string ResolvePath(ITemplateFile templateFile, HttpContextBase httpContext, string virtualPath)
        {
            Debug.Assert(!String.IsNullOrEmpty(virtualPath));
            string basePath;
            if (templateFile != null)
            {
                // If a page is available resolve paths relative to it.
                basePath = templateFile.TemplateInfo.VirtualPath;
            }
            else
            {
                basePath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            }
            return VirtualPathUtility.Combine(basePath, virtualPath);
        }
    }
}
