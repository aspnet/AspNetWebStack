// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Defines an unhandled exception logger.</summary>
    public interface IExceptionLogger
    {
        /// <summary>Logs an unhandled exception.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception logging operation.</returns>
        Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken);
    }
}
