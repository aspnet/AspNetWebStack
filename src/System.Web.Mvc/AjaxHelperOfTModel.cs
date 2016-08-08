// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc
{
    public class AjaxHelper<TModel> : AjaxHelper
    {
        private DynamicViewDataDictionary _dynamicViewDataDictionary;
        private ViewDataDictionary<TModel> _viewData;

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(viewContext, viewDataContainer, routeCollection)
        {
            _viewData = new ViewDataDictionary<TModel>(viewDataContainer.ViewData);
        }

        public new dynamic ViewBag
        {
            get
            {
                if (_dynamicViewDataDictionary == null)
                {
                    _dynamicViewDataDictionary = new DynamicViewDataDictionary(() => ViewData);
                }

                return _dynamicViewDataDictionary;
            }
        }

        public new ViewDataDictionary<TModel> ViewData
        {
            get { return _viewData; }
        }
    }
}
