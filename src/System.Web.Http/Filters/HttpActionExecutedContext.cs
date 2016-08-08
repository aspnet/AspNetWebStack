// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public class HttpActionExecutedContext
    {
        private HttpActionContext _actionContext;

        public HttpActionExecutedContext(HttpActionContext actionContext, Exception exception)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            Exception = exception;
            _actionContext = actionContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionExecutedContext"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpActionExecutedContext()
        {
        }

        public HttpActionContext ActionContext
        {
            get { return _actionContext; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionContext = value;
            }
        }

        public Exception Exception { get; set; }

        public HttpResponseMessage Response
        {
            get
            {
                return ActionContext != null ? ActionContext.Response : null;
            }

            set
            {
                ActionContext.Response = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage Request
        {
            get
            {
                return (ActionContext != null && ActionContext.ControllerContext != null)
                           ? ActionContext.ControllerContext.Request
                           : null;
            }
        }
    }
}
