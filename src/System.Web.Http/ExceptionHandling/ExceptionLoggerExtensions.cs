// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides extension methods for <see cref="IExceptionLogger"/>.</summary>
    public static class ExceptionLoggerExtensions
    {
        /// <summary>Calls an exception logger.</summary>
        /// <param name="logger">The unhandled exception logger.</param>
        /// <param name="context">The exception context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception logging operation.</returns>
        public static Task LogAsync(this IExceptionLogger logger, ExceptionContext context,
            CancellationToken cancellationToken)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionLoggerContext loggerContext = new ExceptionLoggerContext(context);
            return logger.LogAsync(loggerContext, cancellationToken);
        }
    }
}
