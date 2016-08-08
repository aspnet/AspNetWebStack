// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    public class ApiControllerActionInvoker : IHttpActionInvoker
    {
        public virtual Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return InvokeActionAsyncCore(actionContext, cancellationToken);
        }

        private static async Task<HttpResponseMessage> InvokeActionAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;
            Contract.Assert(actionDescriptor != null);

            HttpControllerContext controllerContext = actionContext.ControllerContext;
            Contract.Assert(controllerContext != null);

            try
            {
                object result = await actionDescriptor.ExecuteAsync(controllerContext, actionContext.ActionArguments, cancellationToken);

                // This is cached in a local for performance reasons. ReturnType is a virtual property on HttpActionDescriptor,
                // or else we'd want to cache this as part of that class.
                bool isDeclaredTypeActionResult = typeof(IHttpActionResult).IsAssignableFrom(actionDescriptor.ReturnType);
                if (result == null && isDeclaredTypeActionResult)
                {
                    // If the return type of the action descriptor is IHttpActionResult, it's not valid to return null
                    throw Error.InvalidOperation(SRResources.ApiControllerActionInvoker_NullHttpActionResult);
                }
                
                if (isDeclaredTypeActionResult || actionDescriptor.ReturnType == typeof(object))
                {
                    IHttpActionResult actionResult = result as IHttpActionResult;

                    if (actionResult == null && isDeclaredTypeActionResult)
                    {
                        // If the return type of the action descriptor is IHttpActionResult, it's not valid to return an
                        // object that doesn't implement IHttpActionResult
                        throw Error.InvalidOperation(SRResources.ApiControllerActionInvoker_InvalidHttpActionResult, result.GetType());
                    }
                    else if (actionResult != null)
                    {
                        HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
                        if (response == null)
                        {
                            throw Error.InvalidOperation(SRResources.ResponseMessageResultConverter_NullHttpResponseMessage);
                        }

                        response.EnsureResponseHasRequest(actionContext.Request);
                        return response;
                    }
                }
 
                // This is a non-IHttpActionResult, so run the converter
                return actionDescriptor.ResultConverter.Convert(controllerContext, result);
            }
            catch (HttpResponseException httpResponseException)
            {
                HttpResponseMessage response = httpResponseException.Response;
                response.EnsureResponseHasRequest(actionContext.Request);

                return response;
            }
        }
    }
}
