// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Async;
using System.Web.Routing;
using System.Web.SessionState;

namespace System.Web.Mvc
{
    public class MvcHttpHandler : UrlRoutingHandler, IHttpAsyncHandler, IRequiresSessionState
    {
        private static readonly object _processRequestTag = new object();

        protected virtual IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            return BeginProcessRequest(httpContextBase, callback, state);
        }

        protected internal virtual IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object state)
        {
            IHttpHandler httpHandler = GetHttpHandler(httpContext);
            IHttpAsyncHandler httpAsyncHandler = httpHandler as IHttpAsyncHandler;

            if (httpAsyncHandler != null)
            {
                // asynchronous handler

                // Ensure delegates continue to use the C# Compiler static delegate caching optimization.
                BeginInvokeDelegate<IHttpAsyncHandler> beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState, IHttpAsyncHandler innerHandler)
                {
                    return innerHandler.BeginProcessRequest(HttpContext.Current, asyncCallback, asyncState);
                };
                EndInvokeVoidDelegate<IHttpAsyncHandler> endDelegate = delegate(IAsyncResult asyncResult, IHttpAsyncHandler innerHandler)
                {
                    innerHandler.EndProcessRequest(asyncResult);
                };
                return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, httpAsyncHandler, _processRequestTag);
            }
            else
            {
                // synchronous handler
                Action action = delegate
                {
                    httpHandler.ProcessRequest(HttpContext.Current);
                };
                return AsyncResultWrapper.BeginSynchronous(callback, state, action, _processRequestTag);
            }
        }

        protected internal virtual void EndProcessRequest(IAsyncResult asyncResult)
        {
            AsyncResultWrapper.End(asyncResult, _processRequestTag);
        }

        private static IHttpHandler GetHttpHandler(HttpContextBase httpContext)
        {
            DummyHttpHandler dummyHandler = new DummyHttpHandler();
            dummyHandler.PublicProcessRequest(httpContext);
            return dummyHandler.HttpHandler;
        }

        // synchronous code
        protected override void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext)
        {
            if (httpHandler == null)
            {
                throw new ArgumentNullException("httpHandler");
            }

            httpHandler.ProcessRequest(HttpContext.Current);
        }

        #region IHttpAsyncHandler Members

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return BeginProcessRequest(context, cb, extraData);
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
            EndProcessRequest(result);
        }

        #endregion

        // Since UrlRoutingHandler.ProcessRequest() does the heavy lifting of looking at the RouteCollection for
        // a matching route, we need to call into it. However, that method is also responsible for kicking off
        // the synchronous request, and we can't allow it to do that. The purpose of this dummy class is to run
        // only the lookup portion of UrlRoutingHandler.ProcessRequest(), then intercept the handler it returns
        // and execute it asynchronously.

        private sealed class DummyHttpHandler : UrlRoutingHandler
        {
            public IHttpHandler HttpHandler;

            public void PublicProcessRequest(HttpContextBase httpContext)
            {
                ProcessRequest(httpContext);
            }

            protected override void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext)
            {
                // don't process the request, just store a reference to it
                HttpHandler = httpHandler;
            }
        }
    }
}
