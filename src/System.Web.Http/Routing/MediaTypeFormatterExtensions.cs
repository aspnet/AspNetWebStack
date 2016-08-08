// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Web.Http.Routing;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Extensions for adding <see cref="MediaTypeMapping"/> items to a <see cref="MediaTypeFormatter"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MediaTypeFormatterExtensions
    {
        /// <summary>
        /// Updates the given <paramref name="formatter"/>'s set of <see cref="MediaTypeMapping"/> elements
        /// so that it associates the <paramref name="mediaType"/> with <see cref="HttpRequestMessage"/> whose <see cref="IHttpRouteData"/> contains a URL Parameter {ext}
        /// with the given <paramref name="uriPathExtension"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="MediaTypeFormatter"/> to receive the new <see cref="UriPathExtensionMapping"/> item.</param>
        /// <param name="uriPathExtension">The string of the <see cref="Uri"/> path extension.</param>
        /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> to associate with.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public static void AddUriPathExtensionMapping(
            this MediaTypeFormatter formatter,
            string uriPathExtension,
            MediaTypeHeaderValue mediaType)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            formatter.MediaTypeMappings.Add(mapping);
        }

        /// <summary>
        /// Updates the given <paramref name="formatter"/>'s set of <see cref="MediaTypeMapping"/> elements
        /// so that it associates the <paramref name="mediaType"/> with <see cref="HttpRequestMessage"/> whose <see cref="IHttpRouteData"/> contains a URL Parameter {ext}
        /// with the given <paramref name="uriPathExtension"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="MediaTypeFormatter"/> to receive the new <see cref="UriPathExtensionMapping"/> item.</param>
        /// <param name="uriPathExtension">The string of the <see cref="Uri"/> path extension.</param>
        /// <param name="mediaType">The string media type to associate with.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public static void AddUriPathExtensionMapping(this MediaTypeFormatter formatter, string uriPathExtension, string mediaType)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            formatter.MediaTypeMappings.Add(mapping);
        }
    }
}
