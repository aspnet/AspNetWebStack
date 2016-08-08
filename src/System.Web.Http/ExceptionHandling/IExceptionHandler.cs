// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Defines an unhandled exception handler.</summary>
    /// <remarks>
    /// An unhandled exception handler can either handle the exception (by providing a response) or allow it to
    /// propagate (by providing no response).
    /// </remarks>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Process an unhandled exception, either allowing it to propagate or handling it by providing a response
        /// message to return instead.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous exception handling operation.</returns>
        /// <remarks>
        /// <para>
        /// The exception handler either handles the exception or allows it to propagate. An exception is handled by
        /// setting <see cref="ExceptionHandlerContext.Result"/>, which provides the response message to return in
        /// place of the exception thrown. If <see cref="ExceptionHandlerContext.Result"/> is
        /// <see langword="null"/>, the exception remains unhandled, and the exception will continue to propagate up
        /// the call stack.
        /// </para>
        /// <para>
        /// If the exception propagates when <see cref="ExceptionContextCatchBlock.IsTopLevel"/> is
        /// <see langword="true"/>, the host will see the exception thrown. If the exception propagates when
        /// <see cref="ExceptionContextCatchBlock.IsTopLevel"/> is <see langword="false"/>, another catch block within
        /// Web API will be the next to see the exception, and the exception handler will be called again to make a
        /// decision at that point.
        /// </para>
        /// </remarks>
        Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken);
    }
}
