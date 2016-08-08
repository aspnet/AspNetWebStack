// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Bind directly to the cancellation token
    /// </summary>
    public class CancellationTokenParameterBinding : HttpParameterBinding
    {
        public CancellationTokenParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string name = Descriptor.ParameterName;
            actionContext.ActionArguments.Add(name, cancellationToken);
            return TaskHelpers.Completed();
        }
    }
}
