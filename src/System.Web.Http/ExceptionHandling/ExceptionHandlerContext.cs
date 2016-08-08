// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents the context within which unhandled exception handling occurs.</summary>
    public class ExceptionHandlerContext
    {
        private readonly ExceptionContext _exceptionContext;

        /// <summary>Initializes a new instance of the <see cref="ExceptionHandlerContext"/> class.</summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionHandlerContext(ExceptionContext exceptionContext)
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

        /// <summary>Gets or sets the result providing the response message when the exception is handled.</summary>
        /// <remarks>
        /// If this value is <see langword="null"/>, the exception is left unhandled and will be re-thrown.
        /// </remarks>
        public IHttpActionResult Result { get; set; }

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
    }
}
