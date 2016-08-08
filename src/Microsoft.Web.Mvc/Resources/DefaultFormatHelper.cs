// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Default implementation of FormatHelper
    /// The results for GetRequestFormat() and GetResponseFormats() are cached on the HttpContext.Items dictionary:
    /// HttpContext.Items["requestFormat"]
    /// HttpContext.Items["responseFormat"]
    /// </summary>
    public class DefaultFormatHelper : FormatHelper
    {
        private const string FormatVariableName = "format";
        private const string QualityFactor = "q";

        public const string RequestFormatKey = "requestFormat";
        public const string ResponseFormatKey = "responseFormat";

        /// <summary>
        /// Returns the format of a given request, according to the following
        /// rules:
        /// 1. If a Content-Type header exists it returns a ContentType for it or fails if one can't be created
        /// 2. Otherwie, if a Content-Type header does not exists it provides the default ContentType of "application/octet-stream" (per RFC 2616 7.2.1)
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns>The format of the request.</returns>
        /// <exception cref="HttpException">If the format is unrecognized or not supported.</exception>
        public override ContentType GetRequestFormat(RequestContext requestContext)
        {
            ContentType result;
            if (!requestContext.HttpContext.Items.Contains(RequestFormatKey))
            {
                result = GetRequestFormat(requestContext.HttpContext.Request, true);
                requestContext.HttpContext.Items.Add(RequestFormatKey, result);
            }
            else
            {
                result = (ContentType)requestContext.HttpContext.Items[RequestFormatKey];
            }
            return result;
        }

        internal static ContentType GetRequestFormat(HttpRequestBase request, bool throwOnError)
        {
            if (!String.IsNullOrEmpty(request.ContentType))
            {
                ContentType contentType = ParseContentType(request.ContentType);
                if (contentType != null)
                {
                    return contentType;
                }
                if (throwOnError)
                {
                    throw new HttpException((int)HttpStatusCode.UnsupportedMediaType, String.Format(CultureInfo.CurrentCulture, MvcResources.Resources_UnsupportedMediaType, request.ContentType));
                }
                return null;
            }
            return new ContentType();
        }

        /// <summary>
        /// Returns the preferred content type to use for the response, based on the request, according to the following
        /// rules:
        /// 1. If the RouteData contains a value for a key called "format", its value is returned as the content type
        /// 2. Otherwise, if the query string contains a key called "format", its value is returned as the content type
        /// 3. Otherwise, if the request has an Accepts header, the list of content types in order of preference is returned
        /// 4. Otherwise, if the request has a content type, its value is returned
        /// </summary>
        /// <param name="requestContext">The request.</param>
        /// <returns>The formats to use for rendering a response.</returns>
        public override IEnumerable<ContentType> GetResponseFormats(RequestContext requestContext)
        {
            IEnumerable<ContentType> result;
            if (!requestContext.HttpContext.Items.Contains(ResponseFormatKey))
            {
                result = GetResponseFormatsRouteAware(requestContext);
                requestContext.HttpContext.Items.Add(ResponseFormatKey, result);
            }
            else
            {
                result = (IEnumerable<ContentType>)requestContext.HttpContext.Items[ResponseFormatKey];
            }
            return result;
        }

        private static List<ContentType> GetResponseFormatsRouteAware(RequestContext requestContext)
        {
            List<ContentType> result = GetResponseFormatsCore(requestContext.HttpContext.Request);
            ContentType contentType;
            if (result == null)
            {
                contentType = FormatManager.Current.FormatHelper.GetRequestFormat(requestContext);
                result = new List<ContentType>(new[] { contentType });
            }
            if (TryGetFromRouteData(requestContext.RouteData, out contentType))
            {
                result.Insert(0, contentType);
            }
            return result;
        }

        /// <summary>
        /// Returns the preferred content type to use for the response, based on the request, according to the following
        /// rules:
        /// 1. If the query string contains a key called "format", its value is returned as the content type
        /// 2. Otherwise, if the request has an Accepts header, the list of content types in order of preference is returned
        /// 3. Otherwise, if the request has a content type, its value is returned
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static List<ContentType> GetResponseFormats(HttpRequestBase request)
        {
            List<ContentType> result = GetResponseFormatsCore(request);
            if (result == null)
            {
                ContentType contentType = GetRequestFormat(request, true);
                result = new List<ContentType>(new[] { contentType });
            }
            return result;
        }

        private static List<ContentType> GetResponseFormatsCore(HttpRequestBase request)
        {
            ContentType contentType;
            if (TryGetFromUri(request, out contentType))
            {
                return new List<ContentType>(new[] { contentType });
            }
            string[] accepts = request.AcceptTypes;
            if (accepts != null && accepts.Length > 0)
            {
                return GetAcceptHeaderElements(accepts);
            }
            return null;
        }

        // CONSIDER: we currently don't process the Accept-Charset header, need to take it into account, EG:
        // Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
        private static List<ContentType> GetAcceptHeaderElements(string[] acceptHeaderElements)
        {
            List<ContentType> contentTypeList = new List<ContentType>(acceptHeaderElements.Length);
            foreach (string acceptHeaderElement in acceptHeaderElements)
            {
                if (acceptHeaderElement != null)
                {
                    ContentType contentType = ParseContentType(acceptHeaderElement);
                    // ignore unknown formats to allow fallback
                    if (contentType != null)
                    {
                        contentTypeList.Add(contentType);
                    }
                }
            }
            contentTypeList.Sort(new AcceptHeaderElementComparer());
            // CONSIDER: we used the "q" parameter for sorting, so now we strip it
            // it might be ebtter to strip it later in case someone needs to access it
            foreach (ContentType ct in contentTypeList)
            {
                if (ct.Parameters.ContainsKey(QualityFactor))
                {
                    ct.Parameters.Remove(QualityFactor);
                }
            }
            return contentTypeList;
        }

        public override bool IsBrowserRequest(RequestContext requestContext)
        {
            return IsBrowserRequest(requestContext.HttpContext.Request);
        }

        // Parses a string into a ContentType instance, supports
        // friendly names and enforces a charset (which defaults to utf-8)
        internal static ContentType ParseContentType(string contentTypeString)
        {
            ContentType contentType = null;
            try
            {
                contentType = new ContentType(contentTypeString);
            }
            catch (FormatException)
            {
                // This may be a friendly name (for example, "xml" instead of "text/xml").
                // if so, try mapping to a content type
                if (!FormatManager.Current.TryMapFormatFriendlyName(contentTypeString, out contentType))
                {
                    return null;
                }
            }
            Encoding encoding = Encoding.UTF8;
            if (!String.IsNullOrEmpty(contentType.CharSet))
            {
                try
                {
                    encoding = Encoding.GetEncoding(contentType.CharSet);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            contentType.CharSet = encoding.HeaderName;
            return contentType;
        }

        // Route-based format override so clients can use a route variable
        private static bool TryGetFromRouteData(RouteData routeData, out ContentType contentType)
        {
            contentType = null;
            if (routeData != null)
            {
                string fromRouteData = routeData.Values[FormatVariableName] as string;
                if (!String.IsNullOrEmpty(fromRouteData))
                {
                    contentType = ParseContentType(fromRouteData);
                }
            }
            return contentType != null;
        }

        // Uri-based format override so clients can use a query string
        // also useful when using the browser where you can't set headerss
        private static bool TryGetFromUri(HttpRequestBase request, out ContentType contentType)
        {
            string fromParams = request.QueryString[FormatVariableName];
            if (fromParams != null)
            {
                contentType = ParseContentType(fromParams);
                if (contentType != null)
                {
                    return true;
                }
            }
            contentType = null;
            return false;
        }

        /// <summary>
        /// Determines whether the specified HTTP request was sent by a Browser.
        /// A request is considered to be from the browser if:
        /// it's a GET or POST
        /// and does not have a non-HTML entity format (XML/JSON)
        /// and has a known User-Agent header (as determined by the request's BrowserCapabilities property),
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>true if the specified HTTP request is a Browser request; otherwise, false.</returns>
        internal static bool IsBrowserRequest(HttpRequestBase request)
        {
            if (!request.IsHttpMethod(HttpVerbs.Get) && !request.IsHttpMethod(HttpVerbs.Post))
            {
                return false;
            }
            ContentType requestFormat = GetRequestFormat(request, false);
            if (requestFormat == null || String.Compare(requestFormat.MediaType, FormatManager.UrlEncoded, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (FormatManager.Current.CanDeserialize(requestFormat))
                {
                    return false;
                }
            }
            HttpBrowserCapabilitiesBase browserCapabilities = request.Browser;
            if (browserCapabilities != null && !String.IsNullOrEmpty(request.Browser.Browser) && request.Browser.Browser != "Unknown")
            {
                return true;
            }
            return false;
        }

        private class AcceptHeaderElementComparer : IComparer<ContentType>
        {
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "x, y",
                Justification = "No need to fix this since this is a private class.")]
            public int Compare(ContentType x, ContentType y)
            {
                string[] xTypeSubType = x.MediaType.Split('/');
                string[] yTypeSubType = y.MediaType.Split('/');

                if (String.Equals(xTypeSubType[0], yTypeSubType[0], StringComparison.OrdinalIgnoreCase))
                {
                    if (String.Equals(xTypeSubType[1], yTypeSubType[1], StringComparison.OrdinalIgnoreCase))
                    {
                        // need to check the number of parameters to determine which is more specific
                        bool xHasParam = HasParameters(x);
                        bool yHasParam = HasParameters(y);
                        if (xHasParam && !yHasParam)
                        {
                            return 1;
                        }
                        else if (!xHasParam && yHasParam)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (xTypeSubType[1][0] == '*' && xTypeSubType[1].Length == 1)
                        {
                            return 1;
                        }
                        if (yTypeSubType[1][0] == '*' && yTypeSubType[1].Length == 1)
                        {
                            return -1;
                        }
                    }
                }
                else if (xTypeSubType[0][0] == '*' && xTypeSubType[0].Length == 1)
                {
                    return 1;
                }
                else if (yTypeSubType[0][0] == '*' && yTypeSubType[0].Length == 1)
                {
                    return -1;
                }

                decimal qualityDifference = GetQualityFactor(x) - GetQualityFactor(y);
                if (qualityDifference < 0)
                {
                    return 1;
                }
                else if (qualityDifference > 0)
                {
                    return -1;
                }
                return 0;
            }

            private static decimal GetQualityFactor(ContentType contentType)
            {
                decimal result;
                foreach (string key in contentType.Parameters.Keys)
                {
                    if (String.Equals(QualityFactor, key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Decimal.TryParse(contentType.Parameters[key], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result) &&
                            (result <= (decimal)1.0))
                        {
                            return result;
                        }
                    }
                }

                return (decimal)1.0;
            }

            private static bool HasParameters(ContentType contentType)
            {
                int number = 0;
                foreach (string param in contentType.Parameters.Keys)
                {
                    if (!String.Equals(QualityFactor, param, StringComparison.OrdinalIgnoreCase))
                    {
                        number++;
                    }
                }

                return (number > 0);
            }
        }
    }
}
