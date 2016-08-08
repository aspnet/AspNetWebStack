// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.Hosting
{
    /// <summary>
    /// Interface for controlling the use of buffering requests and responses in the host. If a host
    /// provides support for buffering requests and/or responses then it can use this interface to 
    /// determine the policy for when buffering is to be used.
    /// </summary>
    public interface IHostBufferPolicySelector
    {
        /// <summary>
        /// Determines whether the host should buffer the entity body when processing a request with content.
        /// </summary>
        /// <param name="hostContext">The host-specific context.</param>
        /// <returns><c>true</c> if buffering should be used; otherwise a streamed request should be used.</returns>
        bool UseBufferedInputStream(object hostContext);

        /// <summary>
        /// Determines whether the host should buffer the <see cref="HttpResponseMessage"/> entity body.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>response for which to determine
        /// whether host output buffering should be used for the response entity body.</param>
        /// <returns><c>true</c> if buffering should be used; otherwise a streamed response should be used.</returns>
        bool UseBufferedOutputStream(HttpResponseMessage response);
    }
}
