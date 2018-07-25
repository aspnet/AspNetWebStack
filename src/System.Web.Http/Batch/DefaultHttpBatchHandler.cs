// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Default implementation of <see cref="HttpBatchHandler"/> that encodes the HTTP request/response messages as MIME multipart.
    /// </summary>
    /// <remarks>
    /// By default, it buffers the HTTP request messages in memory during parsing.
    /// </remarks>
    public class DefaultHttpBatchHandler : HttpBatchHandler
    {
        private const string MultiPartContentSubtype = "mixed";
        private const string MultiPartMixed = "multipart/mixed";
        private BatchExecutionOrder _executionOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        public DefaultHttpBatchHandler(HttpServer httpServer)
            : base(httpServer)
        {
            ExecutionOrder = BatchExecutionOrder.Sequential;
            SupportedContentTypes = new List<string>() { MultiPartMixed };
        }

        /// <summary>
        /// Gets or sets the execution order for the batch requests. The default execution order is sequential.
        /// </summary>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">value</exception>
        public BatchExecutionOrder ExecutionOrder
        {
            get
            {
                return _executionOrder;
            }
            set
            {
                if (!Enum.IsDefined(typeof(BatchExecutionOrder), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(BatchExecutionOrder));
                }
                _executionOrder = value;
            }
        }

        /// <summary>
        /// Gets the supported content types for the batch request.
        /// </summary>
        public IList<string> SupportedContentTypes { get; private set; }

        /// <summary>
        /// Creates the batch response message.
        /// </summary>
        /// <param name="responses">The responses for the batch requests.</param>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The batch response message.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        public virtual Task<HttpResponseMessage> CreateResponseMessageAsync(IList<HttpResponseMessage> responses, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responses == null)
            {
                throw Error.ArgumentNull("responses");
            }
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            MultipartContent batchContent = new MultipartContent(MultiPartContentSubtype);

            foreach (HttpResponseMessage batchResponse in responses)
            {
                batchContent.Add(new HttpMessageContent(batchResponse));
            }

            HttpResponseMessage response = request.CreateResponse();
            response.Content = batchContent;
            return Task.FromResult(response);
        }

        /// <inheritdoc/>
        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ValidateRequest(request);

            IList<HttpRequestMessage> subRequests = await ParseBatchRequestsAsync(request, cancellationToken);

            try
            {
                IList<HttpResponseMessage> responses = await ExecuteRequestMessagesAsync(subRequests, cancellationToken);
                return await CreateResponseMessageAsync(responses, request, cancellationToken);
            }
            finally
            {
                foreach (HttpRequestMessage subRequest in subRequests)
                {
                    request.RegisterForDispose(subRequest.GetResourcesForDisposal());
                    request.RegisterForDispose(subRequest);
                }
            }
        }

        /// <summary>
        /// Executes the batch request messages.
        /// </summary>
        /// <param name="requests">The collection of batch request messages.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpResponseMessage"/> for the batch requests.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of response messages asynchronously.")]
        public virtual async Task<IList<HttpResponseMessage>> ExecuteRequestMessagesAsync(IEnumerable<HttpRequestMessage> requests, CancellationToken cancellationToken)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull("requests");
            }

            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();

            try
            {
                switch (ExecutionOrder)
                {
                    case BatchExecutionOrder.Sequential:
                        foreach (HttpRequestMessage request in requests)
                        {
                            responses.Add(await Invoker.SendAsync(request, cancellationToken));
                        }
                        break;

                    case BatchExecutionOrder.NonSequential:
                        responses.AddRange(await Task.WhenAll(requests.Select(request => Invoker.SendAsync(request, cancellationToken))));
                        break;
                }
            }
            catch
            {
                foreach (HttpResponseMessage response in responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
                throw;
            }

            return responses;
        }

        /// <summary>
        /// Converts the incoming batch request into a collection of request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpRequestMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public virtual async Task<IList<HttpRequestMessage>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            List<HttpRequestMessage> requests = new List<HttpRequestMessage>();
            cancellationToken.ThrowIfCancellationRequested();
            MultipartStreamProvider streamProvider = await request.Content.ReadAsMultipartAsync();
            foreach (HttpContent httpContent in streamProvider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpRequestMessage innerRequest = request.RequestUri == null ? await httpContent.ReadAsHttpRequestMessageAsync() : await httpContent.ReadAsHttpRequestMessageAsync(request.RequestUri.Scheme);
                innerRequest.CopyBatchRequestProperties(request);
                requests.Add(innerRequest);
            }
            return requests;
        }

        /// <summary>
        /// Validates the incoming request that contains the batch request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        public virtual void ValidateRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.Content == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BatchRequestMissingContent));
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;
            if (contentType == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BatchContentTypeMissing));
            }

            if (!SupportedContentTypes.Contains(contentType.MediaType, StringComparer.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Error.Format(SRResources.BatchMediaTypeNotSupported, contentType.MediaType)));
            }
        }
    }
}