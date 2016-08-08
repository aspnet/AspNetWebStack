// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Parameter binds to the request
    /// </summary>
    public class HttpRequestParameterBinding : HttpParameterBinding
    {
        public HttpRequestParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {            
        }        

        // Execute the binding for the given request.
        // On success, this will add the parameter to the actionContext.ActionArguments dictionary.
        // Caller ensures IsError==false. 
        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string name = Descriptor.ParameterName;
            HttpRequestMessage request = actionContext.ControllerContext.Request;
            actionContext.ActionArguments.Add(name, request);

            return TaskHelpers.Completed();
        }        
    }
}
