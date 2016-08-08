// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

namespace System.Web.Mvc
{
    public class MvcHandler : IHttpAsyncHandler, IHttpHandler, IRequiresSessionState
    {
        private static readonly object _processRequestTag = new object();

        internal static readonly string MvcVersion = GetMvcVersionString();
        public static readonly string MvcVersionHeaderName = "X-AspNetMvc-Version";
        private ControllerBuilder _controllerBuilder;

        public MvcHandler(RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            RequestContext = requestContext;
        }

        internal ControllerBuilder ControllerBuilder
        {
            get
            {
                if (_controllerBuilder == null)
                {
                    _controllerBuilder = ControllerBuilder.Current;
                }
                return _controllerBuilder;
            }
            set { _controllerBuilder = value; }
        }

        public static bool DisableMvcResponseHeader { get; set; }

        protected virtual bool IsReusable
        {
            get { return false; }
        }

        public RequestContext RequestContext { get; private set; }

        protected internal virtual void AddVersionHeader(HttpContextBase httpContext)
        {
            if (!DisableMvcResponseHeader)
            {
                httpContext.Response.AppendHeader(MvcVersionHeaderName, MvcVersion);
            }
        }

        protected virtual IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            return BeginProcessRequest(httpContextBase, callback, state);
        }

        protected internal virtual IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object state)
        {
            IController controller;
            IControllerFactory factory;
            ProcessRequestInit(httpContext, out controller, out factory);

            IAsyncController asyncController = controller as IAsyncController;
            if (asyncController != null)
            {
                // asynchronous controller

                // Ensure delegates continue to use the C# Compiler static delegate caching optimization.
                BeginInvokeDelegate<ProcessRequestState> beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState, ProcessRequestState innerState)
                {
                    try
                    {
                        return innerState.AsyncController.BeginExecute(innerState.RequestContext, asyncCallback, asyncState);
                    }
                    catch
                    {
                        innerState.ReleaseController();
                        throw;
                    }
                };

                EndInvokeVoidDelegate<ProcessRequestState> endDelegate = delegate(IAsyncResult asyncResult, ProcessRequestState innerState)
                {
                    try
                    {
                        innerState.AsyncController.EndExecute(asyncResult);
                    }
                    finally
                    {
                        innerState.ReleaseController();
                    }
                };
                ProcessRequestState outerState = new ProcessRequestState() 
                {
                    AsyncController = asyncController, Factory = factory, RequestContext = RequestContext
                };
                
                SynchronizationContext callbackSyncContext = SynchronizationContextUtil.GetSynchronizationContext();
                return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, outerState, _processRequestTag, callbackSyncContext: callbackSyncContext);
            }
            else
            {
                // synchronous controller
                Action action = delegate
                {
                    try
                    {
                        controller.Execute(RequestContext);
                    }
                    finally
                    {
                        factory.ReleaseController(controller);
                    }
                };

                return AsyncResultWrapper.BeginSynchronous(callback, state, action, _processRequestTag);
            }
        }

        protected internal virtual void EndProcessRequest(IAsyncResult asyncResult)
        {
            AsyncResultWrapper.End(asyncResult, _processRequestTag);
        }

        private static string GetMvcVersionString()
        {
            // DevDiv 216459:
            // This code originally used Assembly.GetName(), but that requires FileIOPermission, which isn't granted in
            // medium trust. However, Assembly.FullName *is* accessible in medium trust.
            return new AssemblyName(typeof(MvcHandler).Assembly.FullName).Version.ToString(2);
        }

        protected virtual void ProcessRequest(HttpContext httpContext)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            ProcessRequest(httpContextBase);
        }

        protected internal virtual void ProcessRequest(HttpContextBase httpContext)
        {
            IController controller;
            IControllerFactory factory;
            ProcessRequestInit(httpContext, out controller, out factory);

            try
            {
                controller.Execute(RequestContext);
            }
            finally
            {
                factory.ReleaseController(controller);
            }
        }

        private void ProcessRequestInit(HttpContextBase httpContext, out IController controller, out IControllerFactory factory)
        {
            // If request validation has already been enabled, make it lazy. This allows attributes like [HttpPost] (which looks
            // at Request.Form) to work correctly without triggering full validation.
            // Tolerate null HttpContext for testing.
            HttpContext currentContext = HttpContext.Current;
            if (currentContext != null)
            {
                bool? isRequestValidationEnabled = ValidationUtility.IsValidationEnabled(currentContext);
                if (isRequestValidationEnabled == true)
                {
                    ValidationUtility.EnableDynamicValidation(currentContext);
                }
            }

            AddVersionHeader(httpContext);
            RemoveOptionalRoutingParameters();

            // Get the controller type
            string controllerName = RequestContext.RouteData.GetRequiredString("controller");

            // Instantiate the controller and call Execute
            factory = ControllerBuilder.GetControllerFactory();
            controller = factory.CreateController(RequestContext, controllerName);
            if (controller == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.ControllerBuilder_FactoryReturnedNull,
                        factory.GetType(),
                        controllerName));
            }
        }

        private void RemoveOptionalRoutingParameters()
        {
            RouteValueDictionary rvd = RequestContext.RouteData.Values;

            // Ensure delegate is stateless
            rvd.RemoveFromDictionary((entry) => entry.Value == UrlParameter.Optional);
        }

        #region IHttpHandler Members

        bool IHttpHandler.IsReusable
        {
            get { return IsReusable; }
        }

        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {
            ProcessRequest(httpContext);
        }

        #endregion

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

        // Keep as value type to avoid allocating
        private struct ProcessRequestState
        {
            internal IAsyncController AsyncController;
            internal IControllerFactory Factory;
            internal RequestContext RequestContext;

            internal void ReleaseController()
            {
                Factory.ReleaseController(AsyncController);
            }
        }
    }
}
