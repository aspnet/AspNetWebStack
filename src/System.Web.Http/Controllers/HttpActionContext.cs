// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Contains information for the executing action.
    /// </summary>
    public class HttpActionContext
    {
        private readonly ModelStateDictionary _modelState = new ModelStateDictionary();
        private readonly Dictionary<string, object> _operationArguments = new Dictionary<string, object>();
        private HttpActionDescriptor _actionDescriptor;
        private HttpControllerContext _controllerContext;
       
        public HttpActionContext(HttpControllerContext controllerContext, HttpActionDescriptor actionDescriptor)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            _controllerContext = controllerContext;
            _actionDescriptor = actionDescriptor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionContext"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpActionContext()
        {
        }

        public HttpControllerContext ControllerContext
        {
            get { return _controllerContext; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controllerContext = value;
            }
        }

        public HttpActionDescriptor ActionDescriptor
        {
            get { return _actionDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _actionDescriptor = value;
            }
        }

        public ModelStateDictionary ModelState
        {
            get { return _modelState; }
        }

        public Dictionary<string, object> ActionArguments
        {
            get { return _operationArguments; }
        }

        public HttpResponseMessage Response { get; set; }

        /// <summary>
        /// Gets the current <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _controllerContext != null ? _controllerContext.Request : null; }
        }

        /// <summary>
        /// Gets the current <see cref="HttpRequestContext"/>.
        /// </summary>
        public HttpRequestContext RequestContext
        {
            get { return _controllerContext != null ? _controllerContext.RequestContext : null; }
        }
    }
}
