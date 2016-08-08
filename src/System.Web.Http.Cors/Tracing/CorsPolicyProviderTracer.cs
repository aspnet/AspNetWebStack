// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors.Properties;
using System.Web.Http.Tracing;

namespace System.Web.Http.Cors.Tracing
{
    internal class CorsPolicyProviderTracer : ICorsPolicyProvider
    {
        private ICorsPolicyProvider _innerPolicyProvider;
        private ITraceWriter _traceWriter;
        private const string MethodName = "GetCorsPolicyAsync";

        public CorsPolicyProviderTracer(ICorsPolicyProvider innerPolicyProvider, ITraceWriter traceWriter)
        {
            Contract.Assert(innerPolicyProvider != null);
            Contract.Assert(traceWriter != null);

            _innerPolicyProvider = innerPolicyProvider;
            _traceWriter = traceWriter;
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<CorsPolicy>(
                request,
                TraceCategories.CorsCategory,
                TraceLevel.Info,
                _innerPolicyProvider.GetType().Name,
                MethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = String.Format(CultureInfo.CurrentCulture, SRResources.TraceCorsRequestContext, request.GetCorsRequestContext());
                },
                execute: () => _innerPolicyProvider.GetCorsPolicyAsync(request, cancellationToken),
                endTrace: (tr, policy) =>
                {
                    if (policy != null)
                    {
                        tr.Message = String.Format(CultureInfo.CurrentCulture, SRResources.TraceEndPolicyReturned, policy);
                    }
                    else
                    {
                        tr.Message = SRResources.TraceEndNoPolicyReturned;
                    }
                },
                errorTrace: null);
        }
    }
}