// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web;
using System.Web.Http.Hosting;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Provides an implementation of <see cref="IHostBufferPolicySelector"/> suited for use
    /// in an ASP.NET environment which provides direct support for input and output buffering.
    /// </summary>
    public class WebHostBufferPolicySelector : IHostBufferPolicySelector
    {
        /// <summary>
        /// Determines whether the host should buffer the entity body when processing a request with content.
        /// </summary>
        /// <param name="hostContext">The host-specific context.  In this case, it is an instance
        /// of <see cref="HttpContextBase"/>.</param>
        /// <returns><c>true</c> if buffering should be used; otherwise a streamed request should be used.</returns>
        public virtual bool UseBufferedInputStream(object hostContext)
        {
            if (hostContext == null)
            {
                throw Error.ArgumentNull("hostContext");
            }

            return true;
        }

        /// <summary>
        /// Determines whether the host should buffer the <see cref="HttpResponseMessage"/> entity body.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>response for which to determine
        /// whether host output buffering should be used for the response entity body.</param>
        /// <returns><c>true</c> if buffering should be used; otherwise a streamed response should be used.</returns>
        public virtual bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            // Any HttpContent that knows its length is presumably already buffered internally.
            HttpContent content = response.Content;
            if (content != null)
            {
                long? contentLength = content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value >= 0)
                {
                    return false;
                }

                // Content length is null or -1 (meaning not known).  
                // Buffer any HttpContent except StreamContent and PushStreamContent
                return !(content is StreamContent || content is PushStreamContent);
            }

            return false;
        }
    }
}
