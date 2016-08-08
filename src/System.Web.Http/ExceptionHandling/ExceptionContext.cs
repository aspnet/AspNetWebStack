// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Represents an exception and the contextual data associated with it when it was caught.</summary>
    public class ExceptionContext
    {
        /// <summary>Initializes a new instance of the <see cref="ExceptionContext"/> class.</summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <remarks>This constructor is for unit testing purposes only.</remarks>
        public ExceptionContext(Exception exception, ExceptionContextCatchBlock catchBlock)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;

            if (catchBlock == null)
            {
                throw new ArgumentNullException("catchBlock");
            }

            CatchBlock = catchBlock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="actionContext">The action context in which the exception occurred.</param>
        public ExceptionContext(Exception exception, ExceptionContextCatchBlock catchBlock,
            HttpActionContext actionContext)
            : this(exception, catchBlock)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            ActionContext = actionContext;

            HttpControllerContext controllerContext = actionContext.ControllerContext;

            if (controllerContext == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpActionContext).Name, "ControllerContext"), "actionContext");
            }

            ControllerContext = controllerContext;

            HttpRequestContext requestContext = controllerContext.RequestContext;
            Contract.Assert(requestContext != null);
            RequestContext = requestContext;

            HttpRequestMessage request = controllerContext.Request;

            if (request == null)
            {
                throw new ArgumentException(Error.Format(SRResources.TypePropertyMustNotBeNull,
                    typeof(HttpControllerContext).Name, "Request"), "actionContext");
            }

            Request = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        public ExceptionContext(Exception exception, ExceptionContextCatchBlock catchBlock, HttpRequestMessage request)
            : this(exception, catchBlock)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Request = request;
            RequestContext = request.GetRequestContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContext"/> class using the values provided.
        /// </summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="catchBlock">The catch block where the exception was caught.</param>
        /// <param name="request">The request being processed when the exception was caught.</param>
        /// <param name="response">The repsonse being returned when the exception was caught.</param>
        public ExceptionContext(Exception exception, ExceptionContextCatchBlock catchBlock, HttpRequestMessage request,
            HttpResponseMessage response) : this(exception, catchBlock)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Request = request;
            RequestContext = request.GetRequestContext();

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            Response = response;
        }

        /// <summary>Gets the exception caught.</summary>
        public Exception Exception { get; private set; }

        /// <summary>Gets the catch block in which the exception was caught.</summary>
        public ExceptionContextCatchBlock CatchBlock { get; private set; }

        /// <summary>Gets the request being processed when the exception was caught.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public HttpRequestMessage Request { get; set; }

        /// <summary>Gets the request context in which the exception occurred.</summary>
        /// <remarks>The setter is for unit testing purposes only.</remarks>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>Gets the controller context in which the exception occurred, if available.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpControllerContext ControllerContext { get; set; }

        /// <summary>Gets the action context in which the exception occurred, if available.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpActionContext ActionContext { get; set; }

        /// <summary>Gets the response being sent when the exception was caught.</summary>
        /// <remarks>
        /// <para>This property will be <see langword="null"/> in most cases.</para>
        /// <para>The setter is for unit testing purposes only.</para>
        /// </remarks>
        public HttpResponseMessage Response { get; set; }
    }
}
