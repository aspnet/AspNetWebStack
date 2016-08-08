// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// This <see cref="ITraceWriter"/> unconditionally responds that
    /// all categories and levels are enabled.  
    /// All attempts to trace call back the caller for trace information
    /// and the information is kept in memory for later use.
    /// <para>
    /// Its use forces all trace statements in all tracers to
    /// evaluate their information and update their TraceRecord.
    /// </para>
    /// </summary>
    public class MemoryTraceWriter : ITestTraceWriter
    {
        private List<TraceRecord> _records = new List<TraceRecord>();

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            TraceRecord record = new TraceRecord(request, category, level);
            traceAction(record);
            _records.Add(record);
        }

        public void Start()
        {
            _records.Clear();
        }

        public bool DidReceiveTraceRequests { get { return _records.Count != 0; } }

        public void Finish()
        {
        }

        public IList<TraceRecord> Records { get { return _records; } }
    }
}