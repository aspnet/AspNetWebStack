// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http.Tracing;

namespace System.Web.Http
{
    /// <summary>
    /// This static class contains helper methods related to the registration
    /// of <see cref="ITraceWriter"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationTracingExtensions
    {
        /// <summary>
        /// Creates and registers an <see cref="ITraceWriter"/> implementation to use
        /// for this application.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> for which
        /// to register the created trace writer.</param>
        /// <remarks>The returned SystemDiagnosticsTraceWriter may be further configured to change it's default settings.</remarks>
        /// <returns>The <see cref="SystemDiagnosticsTraceWriter"/> which was created and registered.</returns>
        public static SystemDiagnosticsTraceWriter EnableSystemDiagnosticsTracing(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            SystemDiagnosticsTraceWriter traceWriter =
                new SystemDiagnosticsTraceWriter()
                {
                    MinimumLevel = TraceLevel.Info,
                    IsVerbose = false
                };

            configuration.Services.Replace(typeof(ITraceWriter), traceWriter);

            return traceWriter;
        }
    }
}