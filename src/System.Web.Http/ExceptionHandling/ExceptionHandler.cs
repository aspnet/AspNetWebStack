// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents an unhandled exception handler.</summary>
    public abstract class ExceptionHandler : IExceptionHandler
    {
        /// <inheritdoc />
        Task IExceptionHandler.HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            if (!ShouldHandle(context))
            {
                return TaskHelpers.Completed();
            }

            return HandleAsync(context, cancellationToken);
        }

        /// <summary>When overridden in a derived class, handles the exception asynchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception handling operation.</returns>
        public virtual Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return TaskHelpers.Completed();
        }

        /// <summary>When overridden in a derived class, handles the exception synchronously.</summary>
        /// <param name="context">The exception handler context.</param>
        public virtual void Handle(ExceptionHandlerContext context)
        {
        }

        /// <summary>Determines whether the exception should be handled.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <returns>
        /// <see langword="true"/> if the exception should be handled; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>The default decision is only to handle exceptions caught at top-level catch blocks.</remarks>
        public virtual bool ShouldHandle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            ExceptionContextCatchBlock catchBlock = exceptionContext.CatchBlock;

            return catchBlock.IsTopLevel;
        }
    }
}
