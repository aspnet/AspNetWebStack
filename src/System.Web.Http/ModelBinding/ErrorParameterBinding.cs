// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describe a binding error.  This includes a message that can give meaningful information to a client.
    /// </summary>
    public class ErrorParameterBinding : HttpParameterBinding
    {
        private readonly string _message;

        public ErrorParameterBinding(HttpParameterDescriptor descriptor, string message)
            : base(descriptor)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }
            _message = message;
        }

        public override string ErrorMessage
        {
            get
            {
                return _message;
            }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // Caller should have already checked IsError before executing, so we shoulnd't be here. 
            return TaskHelpers.FromError(new InvalidOperationException());
        }
    }
}
