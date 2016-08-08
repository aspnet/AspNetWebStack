// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the context within which unhandled exception logging occurs.</summary>
    public class ExceptionLoggerContext
    {
        private readonly ExceptionContext _exceptionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerContext"/> class using the values provided.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionLoggerContext(ExceptionContext exceptionContext)
        {
            if (exceptionContext == null)
            {
                throw new ArgumentNullException("exceptionContext");
            }

            _exceptionContext = exceptionContext;
        }

        /// <summary>Gets the exception context providing the exception and related data.</summary>
        public ExceptionContext ExceptionContext
        {
            get { return _exceptionContext; }
        }

        /// <summary>Gets the exception caught.</summary>
        public Exception Exception
        {
            get { return _exceptionContext.Exception; }
        }

        /// <summary>Gets the catch block in which the exception was caught.</summary>
        public ExceptionContextCatchBlock CatchBlock
        {
            get { return _exceptionContext.CatchBlock; }
        }

        /// <summary>Gets the request being processed when the exception was caught.</summary>
        public HttpRequestMessage Request
        {
            get { return _exceptionContext.Request; }
        }

        /// <summary>Gets the request context in which the exception occurred.</summary>
        public HttpRequestContext RequestContext
        {
            get { return _exceptionContext.RequestContext; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the exception can subsequently be handled by an
        /// <see cref="IExceptionHandler"/> to produce a new response message.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some exceptions are caught after a response is already partially sent, which prevents sending a new
        /// response to handle the exception. In such cases, <see cref="IExceptionLogger"/> will be called to log the
        /// exception, but the <see cref="IExceptionHandler"/> will not be called.
        /// </para>
        /// <para>
        /// If this value is <see langword="true"/>, exceptions from this catch block will be provided to both
        /// <see cref="IExceptionLogger"/> and <see cref="IExceptionHandler"/>. If this value is
        /// see langword="false"/>, exceptions from this catch block cannot be handled and will only be provided to
        /// <see cref="IExceptionLogger"/>.
        /// </para>
        /// </remarks>
        public bool CallsHandler
        {
            get
            {
                Contract.Assert(_exceptionContext != null);
                ExceptionContextCatchBlock catchBlock = _exceptionContext.CatchBlock;

                return catchBlock.CallsHandler;
            }
        }
    }
}
