// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Represents a handler that specifies routing should not handle requests for a route template. When a route provides this class as a handler, requests matching against the route will be ignored.
    /// </summary>
    public sealed class StopRoutingHandler : HttpMessageHandler
    {
        /// <summary>
        /// Like <see cref="T:System.Web.Routing.StopRoutingHandler"/>, the handler does nothing but throws a NotSupportedException. This method should never be called,
        /// and the NotSupportedException should never be thrown directly, because this handler will be replaced by responding a message saying that no route is matched.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The notification that operations should be canceled.</param>
        /// <returns>Throws NotSupportedException.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
