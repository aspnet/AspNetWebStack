// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Web.Http.Routing;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>'s from path extension appearing in <see cref="IHttpRouteData"/>.
    /// It uses the value of the {ext} URL parameter from <see cref="IHttpRouteData"/> for a match.
    /// </summary>
    /// <example>
    /// This sample shows how to use the UriPathExtensionMapping to map urls ending with ".json" to "application/json"
    /// <code>
    /// config.Routes.MapHttpRoute("Default", "{controller}");
    /// config.Routes.MapHttpRoute("DefaultWithExt", "{controller}.{ext}");
    /// config.Formatters.JsonFormatter.AddUriPathExtensionMapping("json", "application/json");
    /// </code>
    /// </example>
    public class UriPathExtensionMapping : MediaTypeMapping
    {
        public static readonly string UriPathExtensionKey = "ext";

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPathExtensionMapping"/> class.
        /// </summary>
        /// <param name="uriPathExtension">The extension corresponding to <paramref name="mediaType"/>.
        /// This value should not include a dot or wildcards.</param>
        /// <param name="mediaType">The media type that will be returned if <paramref name="uriPathExtension"/> is matched.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public UriPathExtensionMapping(string uriPathExtension, string mediaType)
            : base(mediaType)
        {
            Initialize(uriPathExtension);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPathExtensionMapping"/> class.
        /// </summary>
        /// <param name="uriPathExtension">The extension corresponding to <paramref name="mediaType"/>.
        /// This value should not include a dot or wildcards.</param>
        /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> that will be returned if <paramref name="uriPathExtension"/> is matched.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public UriPathExtensionMapping(string uriPathExtension, MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
            Initialize(uriPathExtension);
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> path extension.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public string UriPathExtension { get; private set; }

        /// <summary>
        /// Returns a value indicating whether this <see cref="UriPathExtensionMapping"/>
        /// instance can provide a <see cref="MediaTypeHeaderValue"/> for the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this <paramref name="request"/>'s route data contains a match for <see cref="UriPathExtension"/>
        /// it returns <c>1.0</c> otherwise <c>0.0</c>.</returns>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string extension = GetUriPathExtensionOrNull(request);
            return String.Equals(extension, UriPathExtension, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;
        }

        private static string GetUriPathExtensionOrNull(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();

            if (routeData != null)
            {
                string extension;
                if (routeData.Values.TryGetValue(UriPathExtensionKey, out extension))
                {
                    return extension;
                }
            }

            return null;
        }

        private void Initialize(string uriPathExtension)
        {
            if (String.IsNullOrWhiteSpace(uriPathExtension))
            {
                throw new ArgumentNullException("uriPathExtension");
            }

            UriPathExtension = uriPathExtension.Trim().TrimStart('.');
        }
    }
}
