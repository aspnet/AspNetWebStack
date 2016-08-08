// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public class ViewEngineResult
    {
        public ViewEngineResult(IEnumerable<string> searchedLocations)
        {
            if (searchedLocations == null)
            {
                throw new ArgumentNullException("searchedLocations");
            }

            SearchedLocations = searchedLocations;
        }

        public ViewEngineResult(IView view, IViewEngine viewEngine)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (viewEngine == null)
            {
                throw new ArgumentNullException("viewEngine");
            }

            View = view;
            ViewEngine = viewEngine;
        }

        public IEnumerable<string> SearchedLocations { get; private set; }

        public IView View { get; private set; }

        public IViewEngine ViewEngine { get; private set; }
    }
}
