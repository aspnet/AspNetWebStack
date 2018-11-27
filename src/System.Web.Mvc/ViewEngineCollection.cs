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
        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator, Type[] genericTypes)
        {
            // First, look up using the cacheLocator and do not track the searched paths in non-matching view engines
            // Then, look up using the normal locator and track the searched paths so that an error view engine can be returned
            return Find(cacheLocator, trackSearchedPaths: false, genericTypes: genericTypes)
                   ?? Find(locator, trackSearchedPaths: true, genericTypes: genericTypes);
        }
        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths, Type[] genericTypes)
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
        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, Type[] genericTypes)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }

            return Find(e => e.FindPartialView(controllerContext, partialViewName, true, genericTypes),
                        e => e.FindPartialView(controllerContext, partialViewName, false, genericTypes));
        }
        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, Type[] genericTypes)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewName");
            }

            return Find(e => e.FindView(controllerContext, viewName, masterName, true, genericTypes),
                        e => e.FindView(controllerContext, viewName, masterName, false, genericTypes));
        }
        #region generic methods
        // -------------- Generic Method: 1 parameter
        private ViewEngineResult Find<T>(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            return Find(cacheLocator, locator, new Type[] { typeof(T) });
        }
        private ViewEngineResult Find<T>(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            return Find(lookup, trackSearchedPaths, new Type[] { typeof(T) });
        }
        public virtual ViewEngineResult FindPartialView<T>(ControllerContext controllerContext, string partialViewName)
        {
            return FindPartialView(controllerContext, partialViewName, new Type[] { typeof(T) });
        }
        public virtual ViewEngineResult FindView<T>(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindView(controllerContext, viewName, masterName, new Type[] { typeof(T) });
        }
        // -------------- Generic Method: 2 parameter
        private ViewEngineResult Find<T1, T2>(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            return Find(cacheLocator, locator, new Type[] { typeof(T1), typeof(T2) });
        }
        private ViewEngineResult Find<T1, T2>(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            return Find(lookup, trackSearchedPaths, new Type[] { typeof(T1), typeof(T2) });
        }
        public virtual ViewEngineResult FindPartialView<T1, T2>(ControllerContext controllerContext, string partialViewName)
        {
            return FindPartialView(controllerContext, partialViewName, new Type[] { typeof(T1), typeof(T2) });
        }
        public virtual ViewEngineResult FindView<T1, T2>(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindView(controllerContext, viewName, masterName, new Type[] { typeof(T1), typeof(T2) });
        }
        // -------------- Generic Method: 3 parameter
        private ViewEngineResult Find<T1, T2, T3>(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            return Find(cacheLocator, locator, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        private ViewEngineResult Find<T1, T2, T3>(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            return Find(lookup, trackSearchedPaths, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        public virtual ViewEngineResult FindPartialView<T1, T2, T3>(ControllerContext controllerContext, string partialViewName)
        {
            return FindPartialView(controllerContext, partialViewName, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        public virtual ViewEngineResult FindView<T1, T2, T3>(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindView(controllerContext, viewName, masterName, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        // -------------- Generic Method: 4 parameter
        private ViewEngineResult Find<T1, T2, T3, T4>(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            return Find(cacheLocator, locator, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        private ViewEngineResult Find<T1, T2, T3, T4>(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            return Find(lookup, trackSearchedPaths, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        public virtual ViewEngineResult FindPartialView<T1, T2, T3, T4>(ControllerContext controllerContext, string partialViewName)
        {
            return FindPartialView(controllerContext, partialViewName, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        public virtual ViewEngineResult FindView<T1, T2, T3, T4>(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindView(controllerContext, viewName, masterName, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        // -------------- Generic Method: 5 parameter
        private ViewEngineResult Find<T1, T2, T3, T4, T5>(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator)
        {
            return Find(cacheLocator, locator, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
        private ViewEngineResult Find<T1, T2, T3, T4, T5>(Func<IViewEngine, ViewEngineResult> lookup, bool trackSearchedPaths)
        {
            return Find(lookup, trackSearchedPaths, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
        public virtual ViewEngineResult FindPartialView<T1, T2, T3, T4, T5>(ControllerContext controllerContext, string partialViewName)
        {
            return FindPartialView(controllerContext, partialViewName, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
        public virtual ViewEngineResult FindView<T1, T2, T3, T4, T5>(ControllerContext controllerContext, string viewName, string masterName)
        {
            return FindView(controllerContext, viewName, masterName, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
        #endregion
    }
}
