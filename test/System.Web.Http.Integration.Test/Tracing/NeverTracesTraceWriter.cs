// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// This <see cref="ITraceWriter"/> unconditionally responds that
    /// all categories and levels are disabled.  
    /// All attempts to trace do not call back to the user for trace information.
    /// </summary>
    /// <para>
    /// Its use forces all tracers to be installed and to execute,
    /// but all their trace statements are not called back for
    /// their trace information.
    /// </para>
    public class NeverTracesTraceWriter : ITestTraceWriter
    {
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            DidReceiveTraceRequests = true;
        }

        public void Start()
        {
            DidReceiveTraceRequests = false;
        }

        public bool DidReceiveTraceRequests { get; set; }

        public void Finish()
        {
        }
    }
}
