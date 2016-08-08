// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http
{
    /// <summary>Defines a command that asynchronously creates an <see cref="HttpResponseMessage"/>.</summary>
    public interface IHttpActionResult
    {
        /// <summary>Creates an <see cref="HttpResponseMessage"/> asynchronously.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that, when completed, contains the <see cref="HttpResponseMessage"/>.</returns>
        Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken);
    }
}
