// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Tracing.Tracers;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Interface used to mark <see cref="MediaTypeFormatterTracer"/> classes.
    /// </summary>
    internal interface IFormatterTracer
    {
        /// <summary>
        /// Gets the associated <see cref="HttpRequestMessage"/>.
        /// </summary>
        HttpRequestMessage Request { get; }

        /// <summary>
        /// Gets the inner <see cref="MediaTypeFormatter"/> this tracer is monitoring.
        /// </summary>
        MediaTypeFormatter InnerFormatter { get; }
    }
}
