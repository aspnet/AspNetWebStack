// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.WebPages;

namespace Microsoft.Web.Helpers
{
    public class UrlBuilder
    {
        private static readonly VirtualPathUtilityWrapper _defaultVirtualPathUtility = new VirtualPathUtilityWrapper();
        private readonly VirtualPathUtilityBase _virtualPathUtility;
        private readonly StringBuilder _params = new StringBuilder();
        private string _path;

        /// <summary>
        /// Constructs an Url with the current page's virtual path and no query string parameters
        /// </summary>
        public UrlBuilder()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructs an Url with the specified path and no query string parameters.
        /// </summary>
        public UrlBuilder(string path)
            : this(path, null)
        {
        }

        /// <summary>
        /// Constructs an Url with the current page's virtual path and the parameters 
        /// </summary>
        /// <param name="parameters"></param>
        public UrlBuilder(object parameters)
            : this(null, parameters)
        {
        }

        public UrlBuilder(string path, object parameters)
            : this(GetHttpContext(), null, path, parameters)
        {
        }

        internal UrlBuilder(HttpContextBase httpContext, VirtualPathUtilityBase virtualPathUtility, string path, object parameters)
        {
            _virtualPathUtility = virtualPathUtility;
            Uri uri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                _path = uri.GetLeftPart(UriPartial.Path);
                _params.Append(uri.Query);
            }
            else
            {
                // If the url is being built as part of a WebPages request, use the template stack to identify the current template's virtual path.
                _path = GetPageRelativePath(httpContext, path);
                int queryStringIndex = (_path ?? String.Empty).IndexOf('?');
                if (queryStringIndex != -1)
                {
                    _params.Append(_path.Substring(queryStringIndex));
                    _path = _path.Substring(0, queryStringIndex);
                }
            }

            if (parameters != null)
            {
                AddParam(parameters);
            }
        }

        internal static VirtualPathUtilityBase DefaultVirtualPathUtility
        {
            get { return _defaultVirtualPathUtility; }
        }

        public string Path
        {
            get { return _path; }
        }

        public string QueryString
        {
            get { return _params.ToString(); }
        }

        private VirtualPathUtilityBase VirtualPathUtility
        {
            get { return _virtualPathUtility ?? _defaultVirtualPathUtility; }
        }

        /// <summary>
        /// Factory method to create an UrlBuilder instance
        /// </summary>
        public static UrlBuilder Create(string path, object parameters = null)
        {
            return new UrlBuilder(path, parameters);
        }

        public UrlBuilder AddPath(string path)
        {
            _path = EnsureTrailingSlash(_path);
            if (!path.IsEmpty())
            {
                _path += HttpUtility.UrlPathEncode(path.TrimStart('/'));
            }
            return this;
        }

        public UrlBuilder AddPath(params string[] pathTokens)
        {
            foreach (var token in pathTokens)
            {
                AddPath(token);
            }
            return this;
        }

        public UrlBuilder AddParam(string name, object value)
        {
            if (!String.IsNullOrEmpty(name))
            {
                _params.Append(_params.Length == 0 ? '?' : '&');
                _params.Append(HttpUtility.UrlEncode(name))
                    .Append('=')
                    .Append(HttpUtility.UrlEncode(Convert.ToString(value, CultureInfo.InvariantCulture)));
            }
            return this;
        }

        public UrlBuilder AddParam(object values)
        {
            var dictionary = new RouteValueDictionary(values);
            foreach (var item in dictionary)
            {
                AddParam(item.Key, item.Value);
            }

            return this;
        }

        public override string ToString()
        {
            return _path + _params;
        }

        private static HttpContextBase GetHttpContext()
        {
            return HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : null;
        }

        private static string EnsureTrailingSlash(string path)
        {
            if (!path.IsEmpty() && path[path.Length - 1] != '/')
            {
                path += '/';
            }
            return path;
        }

        private string GetPageRelativePath(HttpContextBase httpContext, string path)
        {
            if (httpContext == null)
            {
                return path;
            }
            var templateFile = TemplateStack.GetCurrentTemplate(httpContext);
            if (templateFile != null)
            {
                var templateVirtualPath = templateFile.TemplateInfo.VirtualPath;
                if (path.IsEmpty())
                {
                    path = templateVirtualPath;
                }
                else
                {
                    path = VirtualPathUtility.Combine(templateVirtualPath, path);
                }
            }
            return VirtualPathUtility.ToAbsolute(path ?? "~/");
        }

        public static implicit operator string(UrlBuilder builder)
        {
            return builder.ToString();
        }
    }
}
