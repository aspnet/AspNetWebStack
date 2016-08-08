// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Metadata;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Describes how a parameter is bound. The binding should be static (based purely on the descriptor) and 
    /// can be shared across requests. 
    /// </summary>
    public abstract class HttpParameterBinding
    {
        private readonly HttpParameterDescriptor _descriptor;

        protected HttpParameterBinding(HttpParameterDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw Error.ArgumentNull("descriptor");
            }
            _descriptor = descriptor;
        }

        /// <summary>
        /// True iff this binding owns the body. This is important since the body can be a stream that is only read once.
        /// This lets us know who is trying to read the body, and enforce that there is only one reader. 
        /// </summary>
        public virtual bool WillReadBody
        {
            get { return false; }
        }

        /// <summary>
        /// True if the binding was successful and ExecuteBinding can be called. 
        /// False if there was an error determining this binding. This means a developer error somewhere, such as 
        /// configuration, parameter types, proper attributes, etc. 
        /// </summary>
        public bool IsValid
        {
            get { return ErrorMessage == null; }
        }

        /// <summary>
        /// Get an error message describing why this binding is invalid. 
        /// </summary>
        public virtual string ErrorMessage
        {
            get { return null; }
        }

        public HttpParameterDescriptor Descriptor
        {
            get { return _descriptor; }
        }

        /// <summary>
        /// Execute the binding for the given request.
        /// On success, this will add the parameter to the actionContext.ActionArguments dictionary.
        /// Caller ensures <see cref="IsValid"/> is true.
        /// </summary>
        /// <param name="metadataProvider">metadata provider to use for validation.</param>
        /// <param name="actionContext">action context for the binding. This contains the parameter dictionary that will get populated.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the binding operation. Or a binder can also bind a parameter to this.</param>
        /// <returns>Task that is signaled when the binding is complete. For simple bindings from a URI, this should be signalled immediately.
        /// For bindings that read the content body, this may do network IO.</returns>
        public abstract Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken);

        /// <summary>
        /// Helper to get the parameter value from the action context's argument dictionary
        /// </summary>
        /// <param name="actionContext">action context </param>
        /// <returns>the value for this parameter in the given action context, or null if the parameter has not yet been set.</returns>
        protected object GetValue(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            object value;
            actionContext.ActionArguments.TryGetValue(Descriptor.ParameterName, out value);
            return value;
        }

        /// <summary>
        /// Helper to set the result of this parameter binding in the action context's argument dictionary. 
        /// </summary>
        /// <param name="actionContext">action context.</param>
        /// <param name="value">parameter value.</param>
        protected void SetValue(HttpActionContext actionContext, object value)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }
            actionContext.ActionArguments[Descriptor.ParameterName] = value;
        }
    }
}
