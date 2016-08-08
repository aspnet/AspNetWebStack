// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Specialized ControllerFactory that augments the base controller factory to make it RESTful - specifically, adding
    /// support for multiple formats, HTTP method based dispatch to controller methods and HTTP error handling
    /// </summary>
    public class ResourceControllerFactory : IControllerFactory
    {
        private const string RestActionToken = "$REST$";

        private IControllerFactory _inner;

        public ResourceControllerFactory()
        {
            _inner = ControllerBuilder.Current.GetControllerFactory();
        }

        public ResourceControllerFactory(IControllerFactory inner)
        {
            _inner = inner;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            IController ic = _inner.CreateController(requestContext, controllerName);
            Controller c = ic as Controller;
            if (c != null && WebApiEnabledAttribute.IsDefined(c))
            {
                IActionInvoker iai = c.ActionInvoker;
                ControllerActionInvoker cai = iai as ControllerActionInvoker;
                if (cai != null)
                {
                    c.ActionInvoker = new ResourceControllerActionInvoker();

                    string actionName = requestContext.RouteData.Values["action"] as string;
                    if (String.IsNullOrEmpty(actionName))
                    {
                        // set it to a well known dummy value to avoid not having an action as that would prevent the fixup
                        // code in ResourceControllerActionInvoker, which is based on ActionDescriptor, from running
                        requestContext.RouteData.Values["action"] = RestActionToken;
                    }
                }
            }
            return ic;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return _inner.GetControllerSessionBehavior(requestContext, controllerName);
        }

        public void ReleaseController(IController controller)
        {
            _inner.ReleaseController(controller);
        }

        // This ActionInvoker allows us to dispatch to a controller when no action was provided by the routing
        // infrastructure, but the information is available in the request's HTTP verb (GET/PUT/POST/DELETE)
        private class ResourceControllerActionInvoker : ControllerActionInvoker
        {
            protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                if (actionName == RestActionToken)
                {
                    // cleanup the restActionToken we set earlier
                    controllerContext.RequestContext.RouteData.Values["action"] = null;

                    List<ActionDescriptor> matches = new List<ActionDescriptor>();
                    foreach (ActionDescriptor ad in controllerDescriptor.GetCanonicalActions())
                    {
                        object[] acceptVerbs = ad.GetCustomAttributes(typeof(AcceptVerbsAttribute), false);
                        if (acceptVerbs.Length > 0)
                        {
                            foreach (object o in acceptVerbs)
                            {
                                AcceptVerbsAttribute ava = o as AcceptVerbsAttribute;
                                if (ava != null)
                                {
                                    if (ava.Verbs.Contains(controllerContext.HttpContext.Request.GetHttpMethodOverride().ToUpperInvariant()))
                                    {
                                        matches.Add(ad);
                                    }
                                }
                            }
                        }
                    }
                    switch (matches.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            ActionDescriptor ad = matches[0];
                            actionName = ad.ActionName;
                            controllerContext.RequestContext.RouteData.Values["action"] = actionName;
                            return ad;
                        default:
                            StringBuilder matchesString = new StringBuilder(matches[0].ActionName);
                            for (int index = 1; index < matches.Count; index++)
                            {
                                matchesString.Append(", ");
                                matchesString.Append(matches[index].ActionName);
                            }
                            return new ResourceErrorActionDescriptor(
                                controllerDescriptor,
                                HttpStatusCode.Conflict,
                                String.Format(
                                    CultureInfo.CurrentCulture,
                                    MvcResources.ResourceControllerFactory_ConflictingActions,
                                    controllerDescriptor.ControllerName,
                                    matchesString));
                    }
                }
                return base.FindAction(controllerContext, controllerDescriptor, actionName) ??
                       new ResourceErrorActionDescriptor(
                           controllerDescriptor,
                           HttpStatusCode.NotFound,
                           String.Format(
                               CultureInfo.CurrentCulture,
                               MvcResources.ResourceControllerFactory_NoActions,
                               controllerDescriptor.ControllerName));
            }

            // This class is used when we don't find an ActionDescriptor or find multiple matches
            // in this case we want to return an error response but throwing an HttpException from
            // FindAction bypasses the InvokeExceptionFilters, so instead we throw in this custom ActionDescriptor
            private class ResourceErrorActionDescriptor : ActionDescriptor
            {
                private ControllerDescriptor controllerDescriptor;
                private string message;
                private HttpStatusCode statusCode;

                public ResourceErrorActionDescriptor(ControllerDescriptor controllerDescriptor, HttpStatusCode statusCode, string message)
                {
                    this.message = message;
                    this.statusCode = statusCode;
                    this.controllerDescriptor = controllerDescriptor;
                }

                public override string ActionName
                {
                    get { return RestActionToken; }
                }

                public override ControllerDescriptor ControllerDescriptor
                {
                    get { return this.controllerDescriptor; }
                }

                public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
                {
                    HttpException he = new HttpException((int)this.statusCode, this.message);
                    ResourceErrorActionResult rear;
                    if (!WebApiEnabledAttribute.TryGetErrorResult2(controllerContext.RequestContext, he, out rear))
                    {
                        rear = new ResourceErrorActionResult(new HttpException((int)this.statusCode, this.message), new ContentType("text/plain"));
                    }
                    return rear;
                }

                public override ParameterDescriptor[] GetParameters()
                {
                    return new ParameterDescriptor[0];
                }
            }
        }
    }
}
