// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents an unhandled exception logger.</summary>
    public abstract class ExceptionLogger : IExceptionLogger
    {
        internal const string LoggedByKey = "MS_LoggedBy";

        /// <inheritdoc />
        Task IExceptionLogger.LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            if (!ShouldLog(context))
            {
                return TaskHelpers.Completed();
            }

            return LogAsync(context, cancellationToken);
        }

        /// <summary>When overridden in a derived class, logs the exception asynchronously.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception logging operation.</returns>
        public virtual Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            Log(context);
            return TaskHelpers.Completed();
        }

        /// <summary>When overridden in a derived class, logs the exception synchronously.</summary>
        /// <param name="context">The exception logger context.</param>
        public virtual void Log(ExceptionLoggerContext context)
        {
        }

        /// <summary>Determines whether the exception should be logged.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <returns>
        /// <see langword="true"/> if the exception should be logged; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The default decision is only to log an exception instance the first time it is seen by this logger.
        /// </remarks>
        public virtual bool ShouldLog(ExceptionLoggerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            IDictionary data = exceptionContext.Exception.Data;

            if (data == null || data.IsReadOnly)
            {
                // If the exception doesn't have a mutable Data collection, we can't prevent duplicate logging. In this
                // case, just log every time.
                return true;
            }

            ICollection<object> loggedBy;

            if (data.Contains(LoggedByKey))
            {
                object untypedLoggedBy = data[LoggedByKey];

                loggedBy = untypedLoggedBy as ICollection<object>;

                if (loggedBy == null)
                {
                    // If exception.Data["MS_LoggedBy"] exists but is not of the right type, we can't prevent duplicate
                    // logging. In this case, just log every time.
                    return true;
                }

                if (loggedBy.Contains(this))
                {
                    // If this logger has already logged this exception, don't log again.
                    return false;
                }
            }
            else
            {
                loggedBy = new List<object>();
                data.Add(LoggedByKey, loggedBy);
            }

            // Either loggedBy did not exist before (we just added it) or it already existed of the right type and did
            // not already contain this logger. Log now, but mark not to log this exception again for this logger.
            Contract.Assert(loggedBy != null);
            loggedBy.Add(this);
            return true;
        }
    }
}
