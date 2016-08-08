// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Attribute indicating that the controller supports multiple formats (HTML, XML, JSON etc), HTTP method based dispatch
    /// and HTTP error handling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This class is designed to be overridden")]
    public class WebApiEnabledAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public WebApiEnabledAttribute()
        {
            this.StatusOnNullModel = HttpStatusCode.NotFound;
        }

        /// <summary>
        /// The HTTP status code to use in case a null value is returned from the controller action method.
        /// The default is NotFound
        /// </summary>
        public HttpStatusCode StatusOnNullModel { get; set; }

        public static bool IsDefined(ControllerBase controller)
        {
            Type controllerType = controller.GetType();
            WebApiEnabledAttribute[] rea = controllerType.GetCustomAttributes(typeof(WebApiEnabledAttribute), true) as WebApiEnabledAttribute[];
            return rea != null && rea.Length > 0;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            MultiFormatActionResult multiFormatResult = filterContext.Result as MultiFormatActionResult;
            if (multiFormatResult == null)
            {
                ViewResultBase viewResult = filterContext.Result as ViewResultBase;
                if (viewResult != null && viewResult.ViewData != null)
                {
                    bool handled = false;
                    foreach (ContentType responseFormat in filterContext.RequestContext.GetResponseFormats())
                    {
                        // CONSIDER: making this lookup optional if perf is an issue
                        for (int i = 0; i < FormatManager.Current.ResponseFormatHandlers.Count; ++i)
                        {
                            IResponseFormatHandler handler = FormatManager.Current.ResponseFormatHandlers[i];
                            if (handler.CanSerialize(responseFormat))
                            {
                                // we can't use the full ContentType's name (EG: "text/xml")
                                // instead we use the FriendlyName on the matching IResponseFormatHandler
                                string friendlyName = handler.FriendlyName;
                                string viewName = viewResult.ViewName;
                                if (String.IsNullOrEmpty(viewName))
                                {
                                    viewName = filterContext.RouteData.GetRequiredString("action");
                                }
                                // CONSIDER: is this naming convention sufficient? look at extensibility (how can I customize the FindView process?)
                                viewName = viewName + "." + friendlyName;
                                // CONSIDER: ViewEngineCollection queries view engines in registration order and returns 1st match,
                                // would it make sense to let the client provide a hint in case
                                ViewEngineResult result = viewResult.ViewEngineCollection.FindView(filterContext, viewName, null);
                                // ignore errors and fallback to default behavior
                                if (result != null && result.View != null)
                                {
                                    Encoding encoding = Encoding.UTF8;
                                    if (!String.IsNullOrEmpty(responseFormat.CharSet))
                                    {
                                        try
                                        {
                                            encoding = Encoding.GetEncoding(responseFormat.CharSet);
                                        }
                                        catch (ArgumentException)
                                        {
                                            throw new HttpException((int)HttpStatusCode.NotAcceptable, String.Format(CultureInfo.CurrentCulture, MvcResources.Resources_UnsupportedFormat, responseFormat));
                                        }
                                    }
                                    responseFormat.CharSet = encoding.HeaderName;
                                    filterContext.HttpContext.Response.ContentType = responseFormat.ToString();
                                    filterContext.HttpContext.Response.ContentEncoding = encoding;
                                    // we have set the Response.ContentType but know that the webforms view engine will override it
                                    // a different ViewPage base class that sets this can be used to workaround this
                                    // so we make the computed responseFormat available in ViewData
                                    viewResult.ViewData[DefaultFormatHelper.ResponseFormatKey] = responseFormat;
                                    viewResult.View = result.View;
                                    viewResult.ViewName = viewName;
                                    handled = true;
                                    break;
                                }
                            }
                        }
                        if (handled)
                        {
                            break;
                        }
                        if (TryGetResult(viewResult, responseFormat, out multiFormatResult))
                        {
                            if (multiFormatResult != null)
                            {
                                filterContext.Result = multiFormatResult;
                            }
                            handled = true;
                            break;
                        }
                    }
                    if (!handled)
                    {
                        // if enumeration doesn't yield a handler the request is not acceptable
                        // CONSIDER: returning all formats considered in the exception messages
                        throw new HttpException((int)HttpStatusCode.NotAcceptable, "None of the formats specified by the accept header is supported.");
                    }
                }
            }
            base.OnActionExecuted(filterContext);
            RedirectToRouteResult redirectResult = filterContext.Result as RedirectToRouteResult;
            if (redirectResult != null && !filterContext.RequestContext.IsBrowserRequest())
            {
                filterContext.Result = new ResourceRedirectToRouteResult(redirectResult);
            }
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            HttpException he = filterContext.Exception as HttpException;
            if (he != null)
            {
                ResourceErrorActionResult rear;
                if (TryGetErrorResult2(filterContext.RequestContext, he, out rear))
                {
                    if (rear != null)
                    {
                        filterContext.Result = rear;
                        filterContext.ExceptionHandled = true;
                    }
                    return;
                }
                // if enumeration doesn't yield a handler the request is not acceptable
                // CONSIDER: returning all formats considered in the exception messages
                throw new HttpException((int)HttpStatusCode.NotAcceptable, "None of the formats specified by the accept header is supported.");
            }
        }

        public virtual bool TryGetErrorResult(HttpException exception, ContentType responseFormat, out ResourceErrorActionResult actionResult)
        {
            if (FormatManager.Current.CanSerialize(responseFormat))
            {
                actionResult = new ResourceErrorActionResult(exception, responseFormat);
                return true;
            }
            switch (responseFormat.MediaType)
            {
                case "application/octet-stream":
                case "application/x-www-form-urlencoded":
                case "text/html":
                case "*/*":
                    actionResult = null;
                    return true;
                default:
                    actionResult = null;
                    return false;
            }
        }

        public virtual bool TryGetResult(ViewResultBase viewResult, ContentType responseFormat, out MultiFormatActionResult actionResult)
        {
            if (FormatManager.Current.CanSerialize(responseFormat))
            {
                if (viewResult.ViewData.Model == null)
                {
                    throw new HttpException((int)this.StatusOnNullModel, this.StatusOnNullModel.ToString());
                }
                actionResult = new MultiFormatActionResult(viewResult.ViewData.Model, responseFormat);
                return true;
            }

            switch (responseFormat.MediaType)
            {
                case "application/octet-stream":
                case "application/x-www-form-urlencoded":
                case "text/html":
                case "*/*":
                    actionResult = null;
                    return true;
                default:
                    actionResult = null;
                    return false;
            }
        }

        internal static bool TryGetErrorResult2(RequestContext requestContext, HttpException he, out ResourceErrorActionResult actionResult)
        {
            foreach (ContentType responseFormat in requestContext.GetResponseFormats())
            {
                WebApiEnabledAttribute dummy = new WebApiEnabledAttribute();
                if (dummy.TryGetErrorResult(he, responseFormat, out actionResult))
                {
                    return true;
                }
            }
            actionResult = null;
            return false;
        }
    }
}
