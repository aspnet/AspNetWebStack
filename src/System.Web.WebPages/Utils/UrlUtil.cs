// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Routing;

namespace System.Web.WebPages
{
    internal static class UrlUtil
    {
        private static UrlRewriterHelper _urlRewriterHelper = new UrlRewriterHelper();

        // this method can accept an app-relative path or an absolute path for contentPath
        public static string GenerateClientUrl(HttpContextBase httpContext, string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
            {
                return contentPath;
            }

            // many of the methods we call internally can't handle query strings properly, so just strip it out for
            // the time being
            string query;
            contentPath = StripQuery(contentPath, out query);

            // many of the methods we call internally can't handle query strings properly, so tack it on after processing
            // the virtual app path and url rewrites
            if (String.IsNullOrEmpty(query))
            {
                return GenerateClientUrlInternal(httpContext, contentPath);
            }
            else
            {
                return GenerateClientUrlInternal(httpContext, contentPath) + query;
            }
        }

        public static string GenerateClientUrl(HttpContextBase httpContext, string basePath, string path, params object[] pathParts)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }

            if (pathParts != null)
            {
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (pathParts[i] == null)
                    {
                        throw new ArgumentNullException("pathParts");
                    }
                }
            }

            if (basePath != null)
            {
                path = VirtualPathUtility.Combine(basePath, path);
            }

            string query;
            string processedPath = BuildUrl(path, out query, pathParts);

            // many of the methods we call internally can't handle query strings properly, so tack it on after processing
            // the virtual app path and url rewrites
            if (String.IsNullOrEmpty(query))
            {
                return GenerateClientUrlInternal(httpContext, processedPath);
            }
            else
            {
                return GenerateClientUrlInternal(httpContext, processedPath) + query;
            }
        }

        private static string GenerateClientUrlInternal(HttpContextBase httpContext, string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
            {
                return contentPath;
            }

            // can't call VirtualPathUtility.IsAppRelative since it throws on some inputs
            bool isAppRelative = contentPath[0] == '~';
            if (isAppRelative)
            {
                string absoluteContentPath = VirtualPathUtility.ToAbsolute(contentPath, httpContext.Request.ApplicationPath);
                return GenerateClientUrlInternal(httpContext, absoluteContentPath);
            }

            // we only want to manipulate the path if URL rewriting is active for this request, else we risk breaking the generated URL
            bool wasRequestRewritten = _urlRewriterHelper.WasRequestRewritten(httpContext);
            if (!wasRequestRewritten)
            {
                return contentPath;
            }

            // Since the rawUrl represents what the user sees in his browser, it is what we want to use as the base
            // of our absolute paths. For example, consider mysite.example.com/foo, which is internally
            // rewritten to content.example.com/mysite/foo. When we want to generate a link to ~/bar, we want to
            // base it from / instead of /foo, otherwise the user ends up seeing mysite.example.com/foo/bar,
            // which is incorrect.
            string relativeUrlToDestination = MakeRelative(httpContext.Request.Path, contentPath);
            string absoluteUrlToDestination = MakeAbsolute(httpContext.Request.RawUrl, relativeUrlToDestination);
            return absoluteUrlToDestination;
        }

        public static string MakeAbsolute(string basePath, string relativePath)
        {
            // The Combine() method can't handle query strings on the base path, so we trim it off.
            string query;
            basePath = StripQuery(basePath, out query);
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public static string MakeRelative(string fromPath, string toPath)
        {
            string relativeUrl = VirtualPathUtility.MakeRelative(fromPath, toPath);
            if (String.IsNullOrEmpty(relativeUrl) || relativeUrl[0] == '?')
            {
                // Sometimes VirtualPathUtility.MakeRelative() will return an empty string when it meant to return '.',
                // but links to {empty string} are browser dependent. We replace it with an explicit path to force
                // consistency across browsers.
                relativeUrl = "./" + relativeUrl;
            }
            return relativeUrl;
        }

        private static string StripQuery(string path, out string query)
        {
            int queryIndex = path.IndexOf('?');
            if (queryIndex >= 0)
            {
                query = path.Substring(queryIndex);
                return path.Substring(0, queryIndex);
            }
            else
            {
                query = null;
                return path;
            }
        }

        internal static void ResetUrlRewriterHelper()
        {
            _urlRewriterHelper = new UrlRewriterHelper();
        }

        internal static string BuildUrl(string path, out string query, params object[] pathParts)
        {
            // Performance senstive 
            // 
            // This code branches on the number of path-parts to either favor string.Concat or StringBuilder 
            // for performance. The most common case (for WebPages) will provide a single int value as a 
            // path-part - string.Concat can be more efficient when we know the number of strings to join.
            if (pathParts == null || pathParts.Length == 0)
            {
                query = String.Empty;
                return HttpUtility.UrlPathEncode(path);
            }
            else if (pathParts.Length == 1)
            {
                object pathPart = pathParts[0];
                if (IsDisplayableType(pathPart.GetType()))
                {
                    string displayablePath = Convert.ToString(pathPart, CultureInfo.InvariantCulture);
                    path = path + "/" + displayablePath;
                    query = String.Empty;
                    return HttpUtility.UrlPathEncode(path);
                }
                else
                {
                    StringBuilder queryBuilder = new StringBuilder();
                    AppendToQueryString(queryBuilder, pathPart);

                    query = queryBuilder.ToString();
                    return HttpUtility.UrlPathEncode(path);
                }
            }
            else
            {
                StringBuilder pathBuilder = new StringBuilder(path);
                StringBuilder queryBuilder = new StringBuilder();

                for (int i = 0; i < pathParts.Length; i++)
                {
                    object pathPart = pathParts[i];
                    if (IsDisplayableType(pathPart.GetType()))
                    {
                        var displayablePath = Convert.ToString(pathPart, CultureInfo.InvariantCulture);
                        pathBuilder.Append('/');
                        pathBuilder.Append(displayablePath);
                    }
                    else
                    {
                        AppendToQueryString(queryBuilder, pathPart);
                    }
                }

                query = queryBuilder.ToString();
                return HttpUtility.UrlPathEncode(pathBuilder.ToString());
            }
        }

        private static void AppendToQueryString(StringBuilder queryString, object obj)
        {
            // If this method is called, then obj isn't a type that we can put in the path, instead
            // we want to format it as key-value pairs for the query string. The mostly likely 
            // user scenario for this is an anonymous type.
            IDictionary<string, object> dictionary = TypeHelper.ObjectToDictionary(obj);

            foreach (var item in dictionary)
            {
                if (queryString.Length == 0)
                {
                    queryString.Append('?');
                }
                else
                {
                    queryString.Append('&');
                }

                string stringValue = Convert.ToString(item.Value, CultureInfo.InvariantCulture);

                queryString.Append(HttpUtility.UrlEncode(item.Key))
                    .Append('=')
                    .Append(HttpUtility.UrlEncode(stringValue));
            }
        }

        /// <summary>
        /// Determines if a type is displayable as part of a Url path.
        /// </summary>
        /// <remarks>
        /// If a type is a displayable type, then we format values of that type as part of the Url Path. If not, then
        /// we attempt to create a RouteValueDictionary, and encode the value as key-value pairs in the query string.
        /// 
        /// We determine if a type is displayable by whether or not it implements any interfaces. The built-in simple
        /// types like Int32 implement IFormattable, which will be used to convert it to a string. 
        /// 
        /// Primarily we do this check to allow anonymous types to represent key-value pairs (anonymous types don't 
        /// implement any interfaces). 
        /// </remarks>
        private static bool IsDisplayableType(Type t)
        {
            return t.GetInterfaces().Length > 0;
        }
    }
}
