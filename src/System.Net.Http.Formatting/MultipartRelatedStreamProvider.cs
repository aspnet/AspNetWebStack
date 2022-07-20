// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// An <see cref="MultipartStreamProvider"/> suited for reading MIME body parts following the
    /// <c>multipart/related</c> media type as defined in RFC 2387 (see http://www.ietf.org/rfc/rfc2387.txt).
    /// </summary>
    public class MultipartRelatedStreamProvider : MultipartStreamProvider
    {
        private const string RelatedSubType = "related";
        private const string ContentID = "Content-ID";
        private const string StartParameter = "Start";

        private HttpContent _rootContent;
        private HttpContent _parent;

        /// <summary>
        /// Gets the <see cref="HttpContent"/> instance that has been marked as the <c>root</c> content in the
        /// MIME multipart related message using the <c>start</c> parameter. If no <c>start</c> parameter is
        /// present then pick the first of the children.
        /// </summary>
        public HttpContent RootContent
        {
            get
            {
                if (_rootContent == null)
                {
                    _rootContent = FindRootContent(_parent, Contents);
                }

                return _rootContent;
            }
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
            {
                throw Error.ArgumentNull("parent");
            }

            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            // See if we need to remember the parent to be able to determine the root content
            if (_parent == null)
            {
                _parent = parent;
            }

            return new MemoryStream();
        }

        /// <summary>
        /// Looks for the "start" parameter of the parent's content type and then finds the corresponding
        /// child HttpContent with a matching Content-ID header field.
        /// </summary>
        /// <returns>The matching child or null if none found.</returns>
        private static HttpContent FindRootContent(HttpContent parent, IEnumerable<HttpContent> children)
        {
            Contract.Assert(children != null);

            // Find 'start' parameter from parent content type. The value is used
            // to identify the MIME body with the corresponding Content-ID header value.
            NameValueHeaderValue startNameValue = FindMultipartRelatedParameter(parent, StartParameter);
            if (startNameValue == null)
            {
                // If we didn't find a "start" parameter then take the first child.
                return children.FirstOrDefault();
            }

            // Look for the child with a Content-ID header that corresponds to the "start" value.
            // If no matching child is found then we return null.
            string startValue = FormattingUtilities.UnquoteToken(startNameValue.Value);
            return children.FirstOrDefault(
                content =>
                {
                    IEnumerable<string> values;
                    if (content.Headers.TryGetValues(ContentID, out values))
                    {
                        return String.Equals(
                            FormattingUtilities.UnquoteToken(values.ElementAt(0)),
                            startValue,
                            StringComparison.OrdinalIgnoreCase);
                    }

                    return false;
                });
        }

        /// <summary>
        /// Looks for a parameter in the <see cref="MediaTypeHeaderValue"/>.
        /// </summary>
        /// <returns>The matching parameter or null if none found.</returns>
        private static NameValueHeaderValue FindMultipartRelatedParameter(HttpContent content, string parameterName)
        {
            // If no parent then we are done
            if (content == null)
            {
                return null;
            }

            // Check that we have a parent content type and that it is indeed multipart/related
            MediaTypeHeaderValue parentContentType = content.Headers.ContentType;
            if (parentContentType == null || !content.IsMimeMultipartContent(RelatedSubType))
            {
                return null;
            }

            // Look for parameter
            return parentContentType.Parameters.FirstOrDefault(nvp => String.Equals(nvp.Name, parameterName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
