// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Properties;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public abstract class ViewStartPage : StartPage, IViewStartPageChild
    {
        private IViewStartPageChild _viewStartPageChild;

        public HtmlHelper<object> Html
        {
            get { return ViewStartPageChild.Html; }
        }

        public UrlHelper Url
        {
            get { return ViewStartPageChild.Url; }
        }

        public ViewContext ViewContext
        {
            get { return ViewStartPageChild.ViewContext; }
        }

        internal IViewStartPageChild ViewStartPageChild
        {
            get
            {
                if (_viewStartPageChild == null)
                {
                    IViewStartPageChild child = ChildPage as IViewStartPageChild;
                    if (child == null)
                    {
                        throw new InvalidOperationException(MvcResources.ViewStartPage_RequiresMvcRazorView);
                    }
                    _viewStartPageChild = child;
                }

                return _viewStartPageChild;
            }
        }
    }
}
