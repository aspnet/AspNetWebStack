// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns an <see cref="HttpStatusCode.Unauthorized"/> response.
    /// </summary>
    public class UnauthorizedResult : IHttpActionResult
    {
        private readonly IEnumerable<AuthenticationHeaderValue> _challenges;
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>Initializes a new instance of the <see cref="StatusCodeResult"/> class.</summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <param name="request">The request message which led to this result.</param>
        public UnauthorizedResult(IEnumerable<AuthenticationHeaderValue> challenges, HttpRequestMessage request)
            : this(challenges, new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="StatusCodeResult"/> class.</summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public UnauthorizedResult(IEnumerable<AuthenticationHeaderValue> challenges, ApiController controller)
            : this(challenges, new StatusCodeResult.ApiControllerDependencyProvider(controller))
        {
        }

        private UnauthorizedResult(IEnumerable<AuthenticationHeaderValue> challenges,
            StatusCodeResult.IDependencyProvider dependencies)
        {
            if (challenges == null)
            {
                throw new ArgumentNullException("challenges");
            }

            Contract.Assert(dependencies != null);

            _challenges = challenges;
            _dependencies = dependencies;
        }

        /// <summary>Gets the WWW-Authenticate challenges.</summary>
        public IEnumerable<AuthenticationHeaderValue> Challenges
        {
            get { return _challenges; }
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
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            try
            {
                foreach (AuthenticationHeaderValue challenge in _challenges)
                {
                    response.Headers.WwwAuthenticate.Add(challenge);
                }

                response.RequestMessage = _dependencies.Request;
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
