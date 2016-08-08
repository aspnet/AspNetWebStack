// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Parameter binding that will read from the body and invoke the formatters. 
    /// </summary>
    public class FormatterParameterBinding : HttpParameterBinding
    {
        // Magic key to pass cancellation token through the request property bag to maintain backward compat.
        private const string CancellationTokenKey = "MS_FormatterParameterBinding_CancellationToken";

        private IEnumerable<MediaTypeFormatter> _formatters;
        private string _errorMessage;

        public FormatterParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator)
            : base(descriptor)
        {
            if (descriptor.IsOptional)
            {
                _errorMessage = Error.Format(SRResources.OptionalBodyParameterNotSupported, descriptor.Prefix ?? descriptor.ParameterName, GetType().Name);
            }
            Formatters = formatters;
            BodyModelValidator = bodyModelValidator;
        }

        public override bool WillReadBody
        {
            get { return true; }
        }

        public override string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get { return _formatters; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("formatters");
                }
                _formatters = value;
            }
        }

        public IBodyModelValidator BodyModelValidator
        {
            get;
            set;
        }

        public virtual Task<object> ReadContentAsync(HttpRequestMessage request, Type type,
            IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            // Try to get the cancellation token if it is set earlier during the magic handshake
            // to maintain backward compatibility.
            object cancellationToken;
            if (!request.Properties.TryGetValue(CancellationTokenKey, out cancellationToken))
            {
                cancellationToken = CancellationToken.None;
            }

            return ReadContentAsync(request, type, formatters, formatterLogger, (CancellationToken)cancellationToken);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed later")]
        public virtual Task<object> ReadContentAsync(HttpRequestMessage request, Type type,
            IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            HttpContent content = request.Content;
            if (content == null)
            {
                object defaultValue = MediaTypeFormatter.GetDefaultValueForType(type);
                if (defaultValue == null)
                {
                    return TaskHelpers.NullResult();
                }
                else
                {
                    return Task.FromResult(defaultValue);
                }
            }

            try
            {
                return content.ReadAsAsync(type, formatters, formatterLogger, cancellationToken);
            }
            catch (UnsupportedMediaTypeException exception)
            {
                // If there is no Content-Type header, provide a better error message
                string errorFormat = content.Headers.ContentType == null ?
                    SRResources.UnsupportedMediaTypeNoContentType :
                    SRResources.UnsupportedMediaType;

                throw new HttpResponseException(
                    request.CreateErrorResponse(
                        HttpStatusCode.UnsupportedMediaType,
                        Error.Format(errorFormat, exception.MediaType.MediaType),
                        exception));
            }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            HttpParameterDescriptor paramFromBody = this.Descriptor;
            Type type = paramFromBody.ParameterType;
            HttpRequestMessage request = actionContext.ControllerContext.Request;
            IFormatterLogger formatterLogger = new ModelStateFormatterLogger(actionContext.ModelState, paramFromBody.ParameterName);

            return ExecuteBindingAsyncCore(metadataProvider, actionContext, paramFromBody, type, request, formatterLogger, cancellationToken);
        }

        // Perf-sensitive - keeping the async method as small as possible
        private async Task ExecuteBindingAsyncCore(ModelMetadataProvider metadataProvider, HttpActionContext actionContext,
            HttpParameterDescriptor paramFromBody, Type type, HttpRequestMessage request, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            // pass the cancellation token through the request as we cannot call the ReadContentAsync overload that takes
            // CancellationToken for backword compatibility reasons.
            request.Properties[CancellationTokenKey] = cancellationToken;
            object model = await ReadContentAsync(request, type, _formatters, formatterLogger);

            // Put the parameter result into the action context.
            SetValue(actionContext, model);

            // validate the object graph.
            // null indicates we want no body parameter validation
            if (BodyModelValidator != null)
            {
                BodyModelValidator.Validate(model, type, metadataProvider, actionContext, paramFromBody.ParameterName);
            }
        }
    }
}
