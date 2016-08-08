// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class ActionFilterAttribute : FilterAttribute, IActionFilter
    {
        public virtual void OnActionExecuting(HttpActionContext actionContext)
        {
        }

        public virtual void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                OnActionExecuting(actionContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "exception is flowed through the task")]
        public virtual Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            try
            {
                OnActionExecuted(actionExecutedContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }

            return TaskHelpers.Completed();
        }

        Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }
            if (continuation == null)
            {
                throw Error.ArgumentNull("continuation");
            }

            return ExecuteActionFilterAsyncCore(actionContext, cancellationToken, continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        private async Task<HttpResponseMessage> ExecuteActionFilterAsyncCore(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            await OnActionExecutingAsync(actionContext, cancellationToken);

            if (actionContext.Response != null)
            {
                return actionContext.Response;
            }

            return await CallOnActionExecutedAsync(actionContext, cancellationToken, continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to intercept all exceptions")]
        private async Task<HttpResponseMessage> CallOnActionExecutedAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            ExceptionDispatchInfo exceptionInfo = null;
            try
            {
                response = await continuation();
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            Exception exception;

            if (exceptionInfo == null)
            {
                exception = null;
            }
            else
            {
                exception = exceptionInfo.SourceException;
            }

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, exception)
            {
                Response = response
            };

            try
            {
                await OnActionExecutedAsync(executedContext, cancellationToken);
            }
            catch
            {
                // Catch is running because OnActionExecuted threw an exception, so we just want to re-throw.
                // We also need to reset the response to forget about it since a filter threw an exception.
                actionContext.Response = null;
                throw;
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }

            Exception newException = executedContext.Exception;

            if (newException != null)
            {
                if (newException == exception)
                {
                    exceptionInfo.Throw();
                }
                else
                {
                    throw newException;
                }
            }

            throw Error.InvalidOperation(SRResources.ActionFilterAttribute_MustSupplyResponseOrException, GetType().Name);
        }
    }
}
