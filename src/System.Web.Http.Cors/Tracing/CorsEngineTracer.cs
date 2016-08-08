// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Web.Cors;
using System.Web.Http.Cors.Properties;
using System.Web.Http.Tracing;

namespace System.Web.Http.Cors.Tracing
{
    internal class CorsEngineTracer : ICorsEngine
    {
        private ICorsEngine _innerCorsEngine;
        private ITraceWriter _traceWriter;
        private const string MethodName = "EvaluatePolicy";

        public CorsEngineTracer(ICorsEngine corsEngine, ITraceWriter traceWriter)
        {
            Contract.Assert(corsEngine != null);
            Contract.Assert(traceWriter != null);

            _innerCorsEngine = corsEngine;
            _traceWriter = traceWriter;
        }

        public CorsResult EvaluatePolicy(CorsRequestContext requestContext, CorsPolicy policy)
        {
            CorsResult corsResult = null;
            object request;
            requestContext.Properties.TryGetValue(typeof(HttpRequestMessage).FullName, out request);

            _traceWriter.TraceBeginEnd(
                request as HttpRequestMessage,
                TraceCategories.CorsCategory,
                TraceLevel.Info,
                _innerCorsEngine.GetType().Name,
                MethodName,
                beginTrace: null,
                execute: () => { corsResult = _innerCorsEngine.EvaluatePolicy(requestContext, policy); },
                endTrace: (tr) =>
                {
                    if (corsResult != null)
                    {
                        tr.Message = String.Format(CultureInfo.CurrentCulture, SRResources.TraceEndCorsResultReturned, corsResult);
                    }
                    else
                    {
                        tr.Message = SRResources.TraceEndNoCorsResultReturned;
                    }
                },
                errorTrace: null);

            return corsResult;
        }
    }
}