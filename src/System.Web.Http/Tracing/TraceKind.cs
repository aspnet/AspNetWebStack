// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Describes the kind of <see cref="TraceRecord"/> for an individual trace operation.
    /// </summary>
    public enum TraceKind
    {
        /// <summary>
        /// Single trace, not part of a Begin/End trace pair
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Trace marking the beginning of some operation.
        /// </summary>
        Begin,

        /// <summary>
        /// Trace marking the end of some operation.
        /// </summary>
        End,
    }
}
