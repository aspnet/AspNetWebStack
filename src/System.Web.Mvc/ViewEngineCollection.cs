// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class ViewEngineCollection : Collection<IViewEngine>
    {
        private IViewEngine[] _combinedItems;
        private IDependencyResolver _dependencyResolver;

        public ViewEngineCollection()
        {
        }

        public ViewEngineCollection(IList<IViewEngine> list)
            : base(list)
        {
        }

        internal ViewEngineCollection(IList<IViewEngine> list, IDependencyResolver dependencyResolver)
            : base(list)
        {
            _dependencyResolver = dependencyResolver;
        }

        internal IViewEngine[] CombinedItems
        {
            get
            {
                IViewEngine[] combinedItems = _combinedItems;
                if (combinedItems == null)
                {
                    combinedItems = MultiServiceResolver.GetCombined<IViewEngine>(Items, _dependencyResolver);
                    _combinedItems = combinedItems;
                }
                return combinedItems;
            }
        }

        protected override void ClearItems()
        {
            _combinedItems = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, IViewEngine item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _combinedItems = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, IViewEngine item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.SetItem(index, item);
        }

        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            // First, look up using the cacheLocator and do not track the searched paths in non-matching view engines
            // Then, look up using the normal locator and track the searched paths so that an error view engine can be returned
            return Find(cacheLocator, trackSearchedPaths: false)
                   ?? Find(locator, trackSearchedPaths: true);
        }

        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            // Returns
            //    1st result
            // OR list of searched paths (if trackSearchedPaths == true)
            // OR null
            ViewEngineResult result;

            List<string> searched = null;
            if (trackSearchedPaths)
            {
                searched = new List<string>();
            }

            foreach (IViewEngine engine in CombinedItems)
            {
                if (engine != null)
                {
                    result = lookup(engine);

                    if (result.View != null)
                    {
                        return result;
                    }

                    if (trackSearchedPaths)
                    {
                        searched.AddRange(result.SearchedLocations);
                    }
                }
            }

            if (trackSearchedPaths)
            {
                // Remove duplicate search paths since multiple view engines could have potentially looked at the same path
                return new ViewEngineResult(searched.Distinct().ToList());
            }
            else
            {
                return null;
            }
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }

            return Find(e => e.FindPartialView(controllerContext, partialViewName, true),
                        e => e.FindPartialView(controllerContext, partialViewName, false));
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewName");
            }

            return Find(e => e.FindView(controllerContext, viewName, masterName, true),
                        e => e.FindView(controllerContext, viewName, masterName, false));
        }
    }
}
