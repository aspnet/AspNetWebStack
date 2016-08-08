// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Razor;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public class RazorView : BuildManagerCompiledView
    {
        public RazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions)
            : this(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions, null)
        {
        }

        public RazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions, IViewPageActivator viewPageActivator)
            : base(controllerContext, viewPath, viewPageActivator)
        {
            LayoutPath = layoutPath ?? String.Empty;
            RunViewStartPages = runViewStartPages;
            StartPageLookup = StartPage.GetStartPage;
            ViewStartFileExtensions = viewStartFileExtensions ?? Enumerable.Empty<string>();
        }

        public string LayoutPath { get; private set; }

        public bool RunViewStartPages { get; private set; }

        internal StartPageLookupDelegate StartPageLookup { get; set; }

        internal IVirtualPathFactory VirtualPathFactory { get; set; }

        internal DisplayModeProvider DisplayModeProvider { get; set; }

        public IEnumerable<string> ViewStartFileExtensions { get; private set; }

        protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            WebViewPage webViewPage = instance as WebViewPage;
            if (webViewPage == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.CshtmlView_WrongViewBase,
                        ViewPath));
            }

            // An overriden master layout might have been specified when the ViewActionResult got returned.
            // We need to hold on to it so that we can set it on the inner page once it has executed.
            webViewPage.OverridenLayoutPath = LayoutPath;
            webViewPage.VirtualPath = ViewPath;
            webViewPage.ViewContext = viewContext;
            webViewPage.ViewData = viewContext.ViewData;

            webViewPage.InitHelpers();

            if (VirtualPathFactory != null)
            {
                webViewPage.VirtualPathFactory = VirtualPathFactory;
            }
            if (DisplayModeProvider != null)
            {
                webViewPage.DisplayModeProvider = DisplayModeProvider;
            }

            WebPageRenderingBase startPage = null;
            if (RunViewStartPages)
            {
                startPage = StartPageLookup(webViewPage, RazorViewEngine.ViewStartFileName, ViewStartFileExtensions);
            }
            webViewPage.ExecutePageHierarchy(new WebPageContext(context: viewContext.HttpContext, page: null, model: null), writer, startPage);
        }
    }
}
