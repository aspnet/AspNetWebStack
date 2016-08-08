// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Filters;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public sealed class GlobalFilterCollection : IEnumerable<Filter>, IFilterProvider
    {
        private List<Filter> _filters = new List<Filter>();

        public int Count
        {
            get { return _filters.Count; }
        }

        public void Add(object filter)
        {
            AddInternal(filter, order: null);
        }

        public void Add(object filter, int order)
        {
            AddInternal(filter, order);
        }

        private void AddInternal(object filter, int? order)
        {
            ValidateFilterInstance(filter);
            _filters.Add(new Filter(filter, FilterScope.Global, order));
        }

        public void Clear()
        {
            _filters.Clear();
        }

        public bool Contains(object filter)
        {
            return _filters.Any(f => f.Instance == filter);
        }

        public IEnumerator<Filter> GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        IEnumerable<Filter> IFilterProvider.GetFilters(ControllerContext controllerContext,
            ActionDescriptor actionDescriptor)
        {
            return this;
        }

        public void Remove(object filter)
        {
            _filters.RemoveAll(f => f.Instance == filter);
        }

        private static void ValidateFilterInstance(object instance)
        {
            if (instance != null && !(
                instance is IActionFilter ||
                instance is IAuthorizationFilter ||
                instance is IExceptionFilter ||
                instance is IResultFilter ||
                instance is IAuthenticationFilter))
            {
                throw Error.InvalidOperation(MvcResources.GlobalFilterCollection_UnsupportedFilterInstance,
                    typeof(IAuthorizationFilter).FullName,
                    typeof(IActionFilter).FullName,
                    typeof(IResultFilter).FullName,
                    typeof(IExceptionFilter).FullName,
                    typeof(IAuthenticationFilter).FullName);
            }
        }
    }
}
