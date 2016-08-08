// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    public class FilterAttributeFilterProvider : IFilterProvider
    {
        private readonly bool _cacheAttributeInstances;

        public FilterAttributeFilterProvider()
            : this(true)
        {
        }

        public FilterAttributeFilterProvider(bool cacheAttributeInstances)
        {
            _cacheAttributeInstances = cacheAttributeInstances;
        }

        protected virtual IEnumerable<FilterAttribute> GetActionAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetFilterAttributes(_cacheAttributeInstances);
        }

        protected virtual IEnumerable<FilterAttribute> GetControllerAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.ControllerDescriptor.GetFilterAttributes(_cacheAttributeInstances);
        }

        public virtual IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            // Results are low in number in the common case so use yield return to avoid creating intermediate collections or nested enumerables
            if (controllerContext.Controller != null)
            {
                foreach (FilterAttribute attr in GetControllerAttributes(controllerContext, actionDescriptor))
                {
                    yield return new Filter(attr, FilterScope.Controller, order: null);
                }
                foreach (FilterAttribute attr in GetActionAttributes(controllerContext, actionDescriptor))
                {
                    yield return new Filter(attr, FilterScope.Action, order: null);
                }
            }             
        }
    }
}
