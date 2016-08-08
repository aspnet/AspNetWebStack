// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    // Though many of the properties on ControllerContext and its subclassed types are virtual, there are still sealed
    // properties (like ControllerContext.RequestContext, ActionExecutingContext.Result, etc.). If these properties
    // were virtual, a mocking framework might override them with incorrect behavior (property getters would return
    // null, property setters would be no-ops). By sealing these properties, we are forcing them to have the default
    // "get or store a value" semantics that they were intended to have.

    public class ControllerContext
    {
        internal const string ParentActionViewContextToken = "ParentActionViewContext";
        private HttpContextBase _httpContext;
        private RequestContext _requestContext;
        private RouteData _routeData;

        // parameterless constructor used for mocking
        public ControllerContext()
        {
        }

        // copy constructor - allows for subclassed types to take an existing ControllerContext as a parameter
        // and we'll automatically set the appropriate properties
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        protected ControllerContext(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            Controller = controllerContext.Controller;
            RequestContext = controllerContext.RequestContext;
        }

        public ControllerContext(HttpContextBase httpContext, RouteData routeData, ControllerBase controller)
            : this(new RequestContext(httpContext, routeData), controller)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ControllerContext(RequestContext requestContext, ControllerBase controller)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            RequestContext = requestContext;
            Controller = controller;
        }

        public virtual ControllerBase Controller { get; set; }

        public IDisplayMode DisplayMode
        {
            get { return DisplayModeProvider.GetDisplayMode(HttpContext); }
            set { DisplayModeProvider.SetDisplayMode(HttpContext, value); }
        }

        public virtual HttpContextBase HttpContext
        {
            get
            {
                if (_httpContext == null)
                {
                    _httpContext = (_requestContext != null) ? _requestContext.HttpContext : new EmptyHttpContext();
                }
                return _httpContext;
            }
            set { _httpContext = value; }
        }

        public virtual bool IsChildAction
        {
            get
            {
                RouteData routeData = RouteData;
                if (routeData == null)
                {
                    return false;
                }
                return routeData.DataTokens.ContainsKey(ParentActionViewContextToken);
            }
        }

        public ViewContext ParentActionViewContext
        {
            get { return RouteData.DataTokens[ParentActionViewContextToken] as ViewContext; }
        }

        public RequestContext RequestContext
        {
            get
            {
                if (_requestContext == null)
                {
                    // still need explicit calls to constructors since the property getters are virtual and might return null
                    HttpContextBase httpContext = HttpContext ?? new EmptyHttpContext();
                    RouteData routeData = RouteData ?? new RouteData();

                    _requestContext = new RequestContext(httpContext, routeData);
                }
                return _requestContext;
            }
            set { _requestContext = value; }
        }

        public virtual RouteData RouteData
        {
            get
            {
                if (_routeData == null)
                {
                    _routeData = (_requestContext != null) ? _requestContext.RouteData : new RouteData();
                }
                return _routeData;
            }
            set { _routeData = value; }
        }

        private sealed class EmptyHttpContext : HttpContextBase
        {
        }
    }
}
