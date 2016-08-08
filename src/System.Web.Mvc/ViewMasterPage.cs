// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Mvc.Properties;
using System.Web.UI;

namespace System.Web.Mvc
{
    [FileLevelControlBuilder(typeof(ViewMasterPageControlBuilder))]
    public class ViewMasterPage : MasterPage
    {
        public AjaxHelper<object> Ajax
        {
            get { return ViewPage.Ajax; }
        }

        public HtmlHelper<object> Html
        {
            get { return ViewPage.Html; }
        }

        public object Model
        {
            get { return ViewData.Model; }
        }

        public TempDataDictionary TempData
        {
            get { return ViewPage.TempData; }
        }

        public UrlHelper Url
        {
            get { return ViewPage.Url; }
        }

        public dynamic ViewBag
        {
            get { return ViewPage.ViewBag; }
        }

        public ViewContext ViewContext
        {
            get { return ViewPage.ViewContext; }
        }

        public ViewDataDictionary ViewData
        {
            get { return ViewPage.ViewData; }
        }

        internal ViewPage ViewPage
        {
            get
            {
                ViewPage viewPage = Page as ViewPage;
                if (viewPage == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MvcResources.ViewMasterPage_RequiresViewPage));
                }
                return viewPage;
            }
        }

        public HtmlTextWriter Writer
        {
            get { return ViewPage.Writer; }
        }
    }
}
