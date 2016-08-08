// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns a <see cref="HttpStatusCode.BadRequest"/> response and performs
    /// content negotiation on an <see cref="HttpError"/> based on a <see cref="ModelStateDictionary"/>.
    /// </summary>
    public class InvalidModelStateResult : IHttpActionResult
    {
        private readonly ModelStateDictionary _modelState;
        private readonly ExceptionResult.IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="InvalidModelStateResult"/> class.</summary>
        /// <param name="modelState">The model state to include in the error.</param>
        /// <param name="includeErrorDetail">
        /// <see langword="true"/> if the error should include exception messages; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
        /// <param name="request">The request message which led to this result.</param>
        /// <param name="formatters">The formatters to use to negotiate and format the content.</param>
        public InvalidModelStateResult(ModelStateDictionary modelState, bool includeErrorDetail,
            IContentNegotiator contentNegotiator, HttpRequestMessage request,
            IEnumerable<MediaTypeFormatter> formatters)
            : this(modelState, new ExceptionResult.DirectDependencyProvider(includeErrorDetail, contentNegotiator,
                request, formatters))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="InvalidModelStateResult"/> class.</summary>
        /// <param name="modelState">The model state to include in the error.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public InvalidModelStateResult(ModelStateDictionary modelState, ApiController controller)
            : this(modelState, new ExceptionResult.ApiControllerDependencyProvider(controller))
        {
        }

        private InvalidModelStateResult(ModelStateDictionary modelState,
            ExceptionResult.IDependencyProvider dependencies)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException("modelState");
            }

            Contract.Assert(dependencies != null);

            _modelState = modelState;
            _dependencies = dependencies;
        }

        /// <summary>Gets the model state to include in the error.</summary>
        public ModelStateDictionary ModelState
        {
            get { return _modelState; }
        }

        /// <summary>Gets a value indicating whether the error should include exception messages.</summary>
        public bool IncludeErrorDetail
        {
            get { return _dependencies.IncludeErrorDetail; }
        }

        /// <summary>Gets the content negotiator to handle content negotiation.</summary>
        public IContentNegotiator ContentNegotiator
        {
            get { return _dependencies.ContentNegotiator; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <summary>Gets the formatters to use to negotiate and format the content.</summary>
        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get { return _dependencies.Formatters; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpError error = new HttpError(_modelState, _dependencies.IncludeErrorDetail);
            return NegotiatedContentResult<HttpError>.Execute(HttpStatusCode.BadRequest, error,
                _dependencies.ContentNegotiator, _dependencies.Request, _dependencies.Formatters);
        }
    }
}
