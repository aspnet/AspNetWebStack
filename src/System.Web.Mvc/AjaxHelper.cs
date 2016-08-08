// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Routing;

namespace System.Web.Mvc
{
    public class AjaxHelper
    {
        private static string _globalizationScriptPath;

        private DynamicViewDataDictionary _dynamicViewDataDictionary;

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }
            if (viewDataContainer == null)
            {
                throw new ArgumentNullException("viewDataContainer");
            }
            if (routeCollection == null)
            {
                throw new ArgumentNullException("routeCollection");
            }
            ViewContext = viewContext;
            ViewDataContainer = viewDataContainer;
            RouteCollection = routeCollection;
        }

        public static string GlobalizationScriptPath
        {
            get
            {
                if (String.IsNullOrEmpty(_globalizationScriptPath))
                {
                    _globalizationScriptPath = "~/Scripts/Globalization";
                }
                return _globalizationScriptPath;
            }
            set { _globalizationScriptPath = value; }
        }

        public RouteCollection RouteCollection { get; private set; }

        public dynamic ViewBag
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

        public ViewContext ViewContext { get; private set; }

        public ViewDataDictionary ViewData
        {
            get { return ViewDataContainer.ViewData; }
        }

        public IViewDataContainer ViewDataContainer { get; internal set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Instance method for consistency with other helpers.")]
        public string JavaScriptStringEncode(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                return message;
            }

            return HttpUtility.JavaScriptStringEncode(message);
        }
    }
}
