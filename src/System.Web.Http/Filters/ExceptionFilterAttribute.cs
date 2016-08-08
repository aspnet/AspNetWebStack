// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(HttpActionExecutedContext actionExecutedContext)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is flowed in the task")]
        public virtual Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            try
            {
                OnException(actionExecutedContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        Task IExceptionFilter.ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            return ExecuteExceptionFilterAsyncCore(actionExecutedContext, cancellationToken);
        }

        private async Task ExecuteExceptionFilterAsyncCore(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await OnExceptionAsync(actionExecutedContext, cancellationToken);
        }
    }
}
