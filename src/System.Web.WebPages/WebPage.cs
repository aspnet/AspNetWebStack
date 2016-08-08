// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.WebPages.Html;
using System.Web.WebPages.Scope;

namespace System.Web.WebPages
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is a core class which needs to have references to many other classes")]
    public abstract class WebPage : WebPageBase
    {
        private static readonly List<IWebPageRequestExecutor> _executors = new List<IWebPageRequestExecutor>();

        private HttpContextBase _context;
        // Expose the model as dynamic
        private dynamic _model;

        // True if this is a 'top level' page (URL addressable), vs a 'satellite' page like a user control or master
        internal bool TopLevelPage { get; set; }

        public override HttpContextBase Context
        {
            get
            {
                if (_context == null)
                {
                    return PageContext.HttpContext;
                }
                return _context;
            }
            set { _context = value; }
        }

        public HtmlHelper Html { get; private set; }

        public ValidationHelper Validation
        {
            get { return PageContext.Validation; }
        }

        public dynamic Model
        {
            get
            {
                if (_model == null)
                {
                    // Instead of directly returning the model, we wrap it in our own custom DynamicObject.
                    // This allows it to perform private reflection, which would normally fail. This is useful
                    // when dealing with anonymous objects, which are always internal
                    _model = ReflectionDynamicObject.WrapObjectIfInternal(PageContext.Model);
                }
                return _model;
            }
        }

        public ModelStateDictionary ModelState
        {
            get { return PageContext.ModelState; }
        }

        public static void RegisterPageExecutor(IWebPageRequestExecutor executor)
        {
            _executors.Add(executor);
        }

        public override void ExecutePageHierarchy()
        {
            using (ScopeStorage.CreateTransientScope(new ScopeStorageDictionary(ScopeStorage.CurrentScope, PageData)))
            {
                ExecutePageHierarchy(_executors);
            }
        }

        internal void ExecutePageHierarchy(IEnumerable<IWebPageRequestExecutor> executors)
        {
            // Call all the executors until we find one that wants to handle it. This is used to implement features
            // such as AJAX Page methods without having to bake them into the framework.
            // Note that we only do this for 'top level' pages, as these are request-level executors that should not run for each user control/master
            if (!TopLevelPage || !executors.Any(executor => executor.Execute(this)))
            {
                // No executor handled the request, so use normal processing
                base.ExecutePageHierarchy();
            }
        }

        public override HelperResult RenderPage(string path, params object[] data)
        {
            return base.RenderPage(path, data);
        }

        protected override void InitializePage()
        {
            base.InitializePage();

            Html = new HtmlHelper(ModelState, Validation);
        }
    }
}
