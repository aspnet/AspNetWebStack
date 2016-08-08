// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.WebPages.Html;
using System.Web.WebPages.Instrumentation;

namespace System.Web.WebPages
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Even though this is essentially a static class, we need helper classes to inherit from it to get their static methods")]
    public class HelperPage
    {
        private static WebPageContext _pageContext;
        private static InstrumentationService _instrumentationService = null;

        private static InstrumentationService InstrumentationService
        {
            get
            {
                if (_instrumentationService == null)
                {
                    _instrumentationService = new InstrumentationService();
                }
                return _instrumentationService;
            }
        }

        public static HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public static WebPageRenderingBase CurrentPage
        {
            get { return PageContext.Page; }
        }

        public static dynamic Page
        {
            get { return CurrentPage.Page; }
        }

        public static dynamic Model
        {
            get
            {
                WebPage currentWebPage = CurrentPage as WebPage;
                if (currentWebPage == null)
                {
                    return null;
                }
                return currentWebPage.Model;
            }
        }

        public static ModelStateDictionary ModelState
        {
            get
            {
                WebPage currentWebPage = CurrentPage as WebPage;
                if (currentWebPage == null)
                {
                    return null;
                }
                return currentWebPage.ModelState;
            }
        }

        public static HtmlHelper Html
        {
            get
            {
                WebPage currentWebPage = CurrentPage as WebPage;
                if (currentWebPage == null)
                {
                    return null;
                }
                return currentWebPage.Html;
            }
        }

        public static WebPageContext PageContext
        {
            get { return _pageContext ?? WebPageContext.Current; }
            set { _pageContext = value; }
        }

        public static HttpApplicationStateBase AppState
        {
            get
            {
                if (Context != null)
                {
                    return Context.Application;
                }
                return null;
            }
        }

        public static dynamic App
        {
            get { return CurrentPage.App; }
        }

        public static string VirtualPath
        {
            get { return PageContext.Page.VirtualPath; }
        }

        public static Cache Cache
        {
            get
            {
                if (Context != null)
                {
                    return Context.Cache;
                }
                return null;
            }
        }

        public static HttpRequestBase Request
        {
            get
            {
                if (Context != null)
                {
                    return Context.Request;
                }
                return null;
            }
        }

        public static HttpResponseBase Response
        {
            get
            {
                if (Context != null)
                {
                    return Context.Response;
                }
                return null;
            }
        }

        public static HttpServerUtilityBase Server
        {
            get
            {
                if (Context != null)
                {
                    return Context.Server;
                }
                return null;
            }
        }

        public static HttpSessionStateBase Session
        {
            get
            {
                if (Context != null)
                {
                    return Context.Session;
                }
                return null;
            }
        }

        public static IList<string> UrlData
        {
            get { return CurrentPage.UrlData; }
        }

        public static IPrincipal User
        {
            get { return CurrentPage.User; }
        }

        public static bool IsPost
        {
            get { return CurrentPage.IsPost; }
        }

        public static bool IsAjax
        {
            get { return CurrentPage.IsAjax; }
        }

        public static IDictionary<object, dynamic> PageData
        {
            get { return PageContext.PageData; }
        }

        protected static string HelperVirtualPath { get; set; }

        public static string Href(string path, params object[] pathParts)
        {
            return CurrentPage.Href(path, pathParts);
        }

        public static void WriteTo(TextWriter writer, object value)
        {
            WebPageBase.WriteTo(writer, value);
        }

        public static void WriteLiteralTo(TextWriter writer, object value)
        {
            WebPageBase.WriteLiteralTo(writer, value);
        }

        public static void WriteTo(TextWriter writer, HelperResult value)
        {
            WebPageBase.WriteTo(writer, value);
        }

        public static void WriteLiteralTo(TextWriter writer, HelperResult value)
        {
            WebPageBase.WriteLiteralTo(writer, value);
        }

        public static void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            CurrentPage.WriteAttributeTo(VirtualPath, writer, name, prefix, suffix, values);
        }

        public static void BeginContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
            BeginContext(PageContext.Page.GetOutputWriter(), virtualPath, startPosition, length, isLiteral);
        }

        public static void BeginContext(TextWriter writer, string virtualPath, int startPosition, int length, bool isLiteral)
        {
            // Double check that the instrumentation service is active because WriteAttribute always calls this
            if (InstrumentationService.IsAvailable)
            {
                InstrumentationService.BeginContext(Context,
                                                    virtualPath,
                                                    writer,
                                                    startPosition,
                                                    length,
                                                    isLiteral);
            }
        }

        public static void EndContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
            EndContext(PageContext.Page.GetOutputWriter(), virtualPath, startPosition, length, isLiteral);
        }

        public static void EndContext(TextWriter writer, string virtualPath, int startPosition, int length, bool isLiteral)
        {
            // Double check that the instrumentation service is active because WriteAttribute always calls this
            if (InstrumentationService.IsAvailable)
            {
                InstrumentationService.EndContext(Context,
                                                  virtualPath,
                                                  writer,
                                                  startPosition,
                                                  length,
                                                  isLiteral);
            }
        }
    }
}
