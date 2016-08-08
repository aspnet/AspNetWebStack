// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

namespace System.Web.WebPages.Administration.PackageManager
{
    internal static class PageUtils
    {
        private const string WebPagesPreferredTag = " aspnetwebpages ";

        internal static string GetPackagesHome()
        {
            return SiteAdmin.GetVirtualPath("~/packages");
        }

        internal static string GetPageVirtualPath(string page)
        {
            return SiteAdmin.GetVirtualPath("~/packages/" + page);
        }

        internal static WebPackageSource GetPackageSource(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return PackageManagerModule.ActiveSource;
            }
            // If no source is found for the specified name, default to the ActiveSource
            return PackageManagerModule.GetSource(name) ?? PackageManagerModule.ActiveSource;
        }

        internal static string GetFilterValue(HttpRequestBase request, string cookieName, string key)
        {
            var value = request.QueryString[key];
            if (String.IsNullOrEmpty(value))
            {
                var cookie = request.Cookies[cookieName];
                if (cookie != null)
                {
                    value = cookie[key];
                }
            }
            return value;
        }

        internal static void PersistFilter(HttpResponseBase response, string cookieName, IDictionary<string, string> filterItems)
        {
            var cookie = response.Cookies[cookieName];
            if (cookie == null)
            {
                cookie = new HttpCookie(cookieName);
                response.Cookies.Add(cookie);
            }
            foreach (var item in filterItems)
            {
                cookie[item.Key] = item.Value;
            }
        }

        internal static bool IsValidLicenseUrl(Uri licenseUri)
        {
            return Uri.UriSchemeHttp.Equals(licenseUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                   Uri.UriSchemeHttps.Equals(licenseUri.Scheme, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Constructs a query string from an IDictionary
        /// </summary>
        internal static string BuildQueryString(IDictionary<string, string> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var param in parameters)
            {
                stringBuilder.Append(stringBuilder.Length == 0 ? '?' : '&')
                    .Append(HttpUtility.UrlEncode(param.Key))
                    .Append('=')
                    .Append(HttpUtility.UrlEncode(param.Value));
            }
            return stringBuilder.ToString();
        }
    }
}
