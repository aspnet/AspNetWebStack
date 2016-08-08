// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Available trace levels.
    /// </summary>
    /// <remarks>
    /// The interpretation of these levels is the responsibility of the
    /// <see cref="ITraceWriter"/> implementation.   The general convention is that
    /// enabling a particular trace level also enables all levels greater than or
    /// equal to it.  For example, tracing at <see cref="Warn"/> level would
    /// generally trace if the trace writer was enabled to trace at level <see cref="Info"/>.
    /// </remarks>
    public enum TraceLevel
    {
        /// <summary>
        /// Tracing is disabled
        /// </summary>
        Off = 0,

        /// <summary>
        /// Trace level for debugging traces
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Trace level for informational traces
        /// </summary>
        Info = 2,

        /// <summary>
        /// Trace level for warning traces
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Trace level for error traces
        /// </summary>
        Error = 4,

        /// <summary>
        /// Trace level for fatal traces
        /// </summary>
        Fatal = 5
    }
}
