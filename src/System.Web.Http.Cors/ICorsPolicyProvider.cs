// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// Provides an abstraction for getting the <see cref="CorsPolicy"/>.
    /// </summary>
    public interface ICorsPolicyProvider
    {
        /// <summary>
        /// Gets the <see cref="CorsPolicy"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The <see cref="CorsPolicy"/>.</returns>
        Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}