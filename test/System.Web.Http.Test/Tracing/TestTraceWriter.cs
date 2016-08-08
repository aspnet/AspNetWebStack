// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Test spy used internally to capture <see cref="TraceRecord"/>s.
    /// </summary>
    internal class TestTraceWriter : ITraceWriter
    {
        private List<TraceRecord> _traceRecords = new List<TraceRecord>();

        public Func<HttpRequestMessage, string, TraceLevel, bool> TraceSelector { get; set; }

        public IList<TraceRecord> Traces { get { return _traceRecords;  } }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (TraceSelector == null || TraceSelector(request, category, level))
            {
                TraceRecord traceRecord = new TraceRecord(request, category, level);
                traceAction(traceRecord);
                lock (_traceRecords)
                {
                    _traceRecords.Add(traceRecord);
                }
            }
        }
    }
}
