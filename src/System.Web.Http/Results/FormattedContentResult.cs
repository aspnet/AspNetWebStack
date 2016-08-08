// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>Represents an action result that returns formatted content.</summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class FormattedContentResult<T> : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly T _content;
        private readonly MediaTypeFormatter _formatter;
        private readonly MediaTypeHeaderValue _mediaType;
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedContentResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="content">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">
        /// The value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </param>
        /// <param name="request">The request message which led to this result.</param>
        public FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, HttpRequestMessage request)
            : this(statusCode, content, formatter, mediaType, new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedContentResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="content">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">
        /// The value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, ApiController controller)
            : this(statusCode, content, formatter, mediaType, new StatusCodeResult.ApiControllerDependencyProvider(
                controller))
        {
        }

        private FormattedContentResult(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, StatusCodeResult.IDependencyProvider dependencies)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            Contract.Assert(dependencies != null);

            _statusCode = statusCode;
            _content = content;
            _formatter = formatter;
            _mediaType = mediaType;
            _dependencies = dependencies;
        }

        /// <summary>Gets the HTTP status code for the response message.</summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary>Gets the content value to format in the entity body.</summary>
        public T Content
        {
            get { return _content; }
        }

        /// <summary>Gets the formatter to use to format the content.</summary>
        public MediaTypeFormatter Formatter
        {
            get { return _formatter; }
        }

        /// <summary>
        /// Gets the value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </summary>
        public MediaTypeHeaderValue MediaType
        {
            get { return _mediaType; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            return Execute(_statusCode, _content, _formatter, _mediaType, _dependencies.Request);
        }

        internal static HttpResponseMessage Execute(HttpStatusCode statusCode, T content, MediaTypeFormatter formatter,
            MediaTypeHeaderValue mediaType, HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode);

            try
            {
                response.Content = new ObjectContent<T>(content, formatter, mediaType);
                response.RequestMessage = request;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }
    }
}
