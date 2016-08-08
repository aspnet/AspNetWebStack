// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Web.Mvc.Properties;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public abstract class WebViewPage : WebPageBase, IViewDataContainer, IViewStartPageChild
    {
        private ViewDataDictionary _viewData;
        private DynamicViewDataDictionary _dynamicViewData;
        private HttpContextBase _context;
        private HtmlHelper<object> _html;
        private AjaxHelper<object> _ajax;

        public override HttpContextBase Context
        {
            // REVIEW why are we forced to override this?
            get { return _context ?? ViewContext.HttpContext; }
            set { _context = value; }
        }

        public HtmlHelper<object> Html
        {
            get
            {
                if (_html == null && ViewContext != null)
                {
                    _html = new HtmlHelper<object>(ViewContext, this);
                }
                return _html;
            }
            set
            {
                _html = value;
            }
        }

        public AjaxHelper<object> Ajax
        {
            get
            {
                if (_ajax == null && ViewContext != null)
                {
                    _ajax = new AjaxHelper<object>(ViewContext, this);
                }
                return _ajax;
            }
            set
            {
                _ajax = value;
            }
        }

        public object Model
        {
            get { return ViewData.Model; }
        }

        internal string OverridenLayoutPath { get; set; }

        public TempDataDictionary TempData
        {
            get { return ViewContext.TempData; }
        }

        public UrlHelper Url { get; set; }

        public dynamic ViewBag
        {
            get
            {
                if (_dynamicViewData == null)
                {
                    _dynamicViewData = new DynamicViewDataDictionary(() => ViewData);
                }
                return _dynamicViewData;
            }
        }

        public ViewContext ViewContext { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is the mechanism by which the ViewPage gets its ViewDataDictionary object.")]
        public ViewDataDictionary ViewData
        {
            get
            {
                if (_viewData == null)
                {
                    SetViewData(new ViewDataDictionary());
                }
                return _viewData;
            }
            set { SetViewData(value); }
        }

        protected override void ConfigurePage(WebPageBase parentPage)
        {
            var baseViewPage = parentPage as WebViewPage;
            if (baseViewPage == null)
            {
                // TODO : review if this check is even necessary.
                // When this method is called by the framework parentPage should already be an instance of WebViewPage
                // Need to review what happens if this method gets called in Plan9 pointing at an MVC view
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MvcResources.CshtmlView_WrongViewBase, parentPage.VirtualPath));
            }

            // Set ViewContext and ViewData here so that the layout page inherits ViewData from the main page
            ViewContext = baseViewPage.ViewContext;
            ViewData = baseViewPage.ViewData;
            InitHelpers();
        }

        public override void ExecutePageHierarchy()
        {
            // Change the Writer so that things like Html.BeginForm work correctly
            TextWriter oldWriter = ViewContext.Writer;
            ViewContext.Writer = Output;

            base.ExecutePageHierarchy();

            // Overwrite LayoutPage so that returning a view with a custom master page works.
            if (!String.IsNullOrEmpty(OverridenLayoutPath))
            {
                Layout = OverridenLayoutPath;
            }

            // Restore the old View Context Writer
            ViewContext.Writer = oldWriter;
        }

        public virtual void InitHelpers()
        {
            // Html and Ajax helpers are lazily initialized since they are not directly visible to a Razor page.
            // In order to ensure back-compat, in the event that this instance gets re-used, we'll reset these
            // properties so they get reinitialized the very next time they get accessed.
            Html = null;
            Ajax = null;
            Url = new UrlHelper(ViewContext.RequestContext);
        }

        protected virtual void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = viewData;
        }
    }
}
