// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action or a controller to disable CORS.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DisableCorsAttribute : Attribute, ICorsPolicyProvider
    {
        /// <inheritdoc />
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult<CorsPolicy>(null);
        }
    }
}