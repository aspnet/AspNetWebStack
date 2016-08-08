// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    internal class AuthorizationFilterResult : IHttpActionResult
    {
        private readonly HttpActionContext _context;
        private readonly IAuthorizationFilter[] _filters;
        private readonly IHttpActionResult _innerResult;

        public AuthorizationFilterResult(HttpActionContext context, IAuthorizationFilter[] filters,
            IHttpActionResult innerResult)
        {
            Contract.Assert(context != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerResult != null);

            _context = context;
            _filters = filters;
            _innerResult = innerResult;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // We need to reverse the filter list so that least specific filters (Global) get run first and the most
            // specific filters (Action) get run last.
            Func<Task<HttpResponseMessage>> result = () => _innerResult.ExecuteAsync(cancellationToken);
            for (int i = _filters.Length - 1; i >= 0; i--)
            {
                IAuthorizationFilter filter = _filters[i];
                Func<Func<Task<HttpResponseMessage>>, IAuthorizationFilter, Func<Task<HttpResponseMessage>>>
                    chainContinuation = (continuation, innerFilter) =>
                    {
                        return () => innerFilter.ExecuteAuthorizationFilterAsync(_context, cancellationToken,
                            continuation);
                    };
                result = chainContinuation(result, filter);
            }

            return result();
        }
    }
}
