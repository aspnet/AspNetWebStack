// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Cors.Properties;
using System.Web.Http.Tracing;

namespace System.Web.Http.Cors.Tracing
{
    internal class CorsPolicyProviderFactoryTracer : ICorsPolicyProviderFactory
    {
        private ICorsPolicyProviderFactory _innerPolicyProviderFactory;
        private ITraceWriter _traceWriter;
        private const string MethodName = "GetCorsPolicyProvider";

        public CorsPolicyProviderFactoryTracer(ICorsPolicyProviderFactory innerPolicyProviderFactory, ITraceWriter traceWriter)
        {
            Contract.Assert(innerPolicyProviderFactory != null);
            Contract.Assert(traceWriter != null);

            _innerPolicyProviderFactory = innerPolicyProviderFactory;
            _traceWriter = traceWriter;
        }

        public ICorsPolicyProvider GetCorsPolicyProvider(HttpRequestMessage request)
        {
            ICorsPolicyProvider policyProvider = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.CorsCategory,
                TraceLevel.Info,
                _innerPolicyProviderFactory.GetType().Name,
                MethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = String.Format(CultureInfo.CurrentCulture, SRResources.TraceCorsRequestContext, request.GetCorsRequestContext());
                },
                execute: () => { policyProvider = _innerPolicyProviderFactory.GetCorsPolicyProvider(request); },
                endTrace: (tr) =>
                {
                    if (policyProvider != null)
                    {
                        tr.Message = String.Format(CultureInfo.CurrentCulture, SRResources.TraceEndPolicyProviderReturned, policyProvider);
                    }
                    else
                    {
                        tr.Message = SRResources.TraceEndNoPolicyProviderReturned;
                    }
                },
                errorTrace: null);

            if (policyProvider != null)
            {
                return new CorsPolicyProviderTracer(policyProvider, _traceWriter);
            }

            return null;
        }
    }
}