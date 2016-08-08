// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>
    /// Represents an authentication challenge context containing information for executing an authentication
    /// challenge.
    /// </summary>
    public class HttpAuthenticationChallengeContext
    {
        private IHttpActionResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAuthenticationChallengeContext"/> class.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="result">The current action result.</param>
        public HttpAuthenticationChallengeContext(HttpActionContext actionContext, IHttpActionResult result)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            ActionContext = actionContext;
            Result = result;
        }

        /// <summary>Gets the action context.</summary>
        public HttpActionContext ActionContext { get; private set; }

        /// <summary>Gets or sets the action result to execute.</summary>
        /// <remarks>
        /// Authentication filters wishing to add an authentication challenge will often chain the result (set the
        /// value to an action result that executes the previous value and then does something additional, such as add
        /// a WWW-Authenticate header).
        /// </remarks>
        public IHttpActionResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _result = value;
            }
        }

        /// <summary>Gets the request message.</summary>
        public HttpRequestMessage Request
        {
            get
            {
                Contract.Assert(ActionContext != null);
                return ActionContext.Request;
            }
        }
    }
}
