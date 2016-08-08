// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Metadata;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// This describes *how* the binding will happen. Does not actually bind. 
    /// This is static for a given action descriptor and can be reused across requests. 
    /// This may be a nice thing to log. Or set a breakpoint after we create and preview what's about to happen. 
    /// In theory, this could be precompiled for each Action descriptor.  
    /// </summary>
    public class HttpActionBinding
    {
        private HttpActionDescriptor _actionDescriptor;
        private HttpParameterBinding[] _parameterBindings;

        private ModelMetadataProvider _metadataProvider;

        public HttpActionBinding()
        {
        }

        public HttpActionBinding(HttpActionDescriptor actionDescriptor, HttpParameterBinding[] bindings)
        {
            ActionDescriptor = actionDescriptor;
            ParameterBindings = bindings;
        }

        /// <summary>
        /// Back pointer to the action this binding is for. 
        /// This can also provide the Type[], string[] names for the parameters.
        /// </summary>
        public HttpActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _actionDescriptor = value;
            }
        }

        /// <summary>
        /// Specifies synchronous bindings for each parameter.This is a parallel array to the ActionDescriptor's parameter array. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Want an array")]
        public HttpParameterBinding[] ParameterBindings
        {
            get
            {
                return _parameterBindings;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _parameterBindings = value;
            }
        }

        public virtual Task ExecuteBindingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (_parameterBindings.Length == 0)
            {
                return TaskHelpers.Completed();
            }

            // First, make sure the actionBinding is valid before trying to execute it. This keeps us in a known state in case of errors.
            for (int i = 0; i < ParameterBindings.Length; i++)
            {
                HttpParameterBinding parameterBinder = ParameterBindings[i];
                if (!parameterBinder.IsValid)
                {
                    // Throwing an exception because the webService developer's action signature is bad.
                    // This exception will be caught and converted into a 500 by the dispatcher
                    throw new InvalidOperationException(parameterBinder.ErrorMessage);
                }
            }

            if (_metadataProvider == null)
            {
                HttpConfiguration config = actionContext.ControllerContext.Configuration;
                _metadataProvider = config.Services.GetModelMetadataProvider();
            }

            return ExecuteBindingAsyncCore(actionContext, cancellationToken);
        }

        private async Task ExecuteBindingAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // Execute all the binders.
            for (int index = 0; index < ParameterBindings.Length; index++)
            {
                HttpParameterBinding parameterBinder = ParameterBindings[index];

                await parameterBinder.ExecuteBindingAsync(_metadataProvider, actionContext, cancellationToken);
            }
        }
    }
}
