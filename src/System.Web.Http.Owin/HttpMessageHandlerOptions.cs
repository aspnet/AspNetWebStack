// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;

namespace System.Web.Http.Owin
{
    /// <summary>Represents the options for configuring an <see cref="HttpMessageHandlerAdapter"/>.</summary>
    public class HttpMessageHandlerOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpMessageHandler"/> to submit requests to.
        /// </summary>
        public HttpMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IHostBufferPolicySelector"/> that determines whether or not to buffer requests
        /// and responses.
        /// </summary>
        public IHostBufferPolicySelector BufferPolicySelector { get; set; }

        /// <summary>Gets or sets the <see cref="IExceptionLogger"/> to use to log unhandled exceptions.</summary>
        public IExceptionLogger ExceptionLogger { get; set; }

        /// <summary>Gets or sets the <see cref="IExceptionHandler"/> to use to process unhandled exceptions.</summary>
        public IExceptionHandler ExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CancellationToken"/> that triggers cleanup of the
        /// <see cref="HttpMessageHandlerAdapter"/>.
        /// </summary>
        public CancellationToken AppDisposing { get; set; }
    }
}
