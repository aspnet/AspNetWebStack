// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class AuthorizationFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(HttpActionContext actionContext)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                OnAuthorization(actionContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }
            if (continuation == null)
            {
                throw Error.ArgumentNull("continuation");
            }

            return ExecuteAuthorizationFilterAsyncCore(actionContext, cancellationToken, continuation);
        }

        private async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            await OnAuthorizationAsync(actionContext, cancellationToken);

            if (actionContext.Response != null)
            {
                return actionContext.Response;
            }
            else
            {
                return await continuation();
            }
        }
    }
}
