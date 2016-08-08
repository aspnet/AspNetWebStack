// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>Represents an action result that returns a specified response message.</summary>
    public class ResponseMessageResult : IHttpActionResult
    {
        private readonly HttpResponseMessage _response;

        /// <summary>Initializes a new instance of the <see cref="ResponseMessageResult"/> class.</summary>
        /// <param name="response">The response message.</param>
        public ResponseMessageResult(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            _response = response;
        }

        /// <summary>Gets the response message.</summary>
        public HttpResponseMessage Response
        {
            get { return _response; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
