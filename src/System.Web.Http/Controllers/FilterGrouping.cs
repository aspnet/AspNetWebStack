// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Quickly split filters into different types
    /// </summary>
    internal class FilterGrouping
    {
        private IActionFilter[] _actionFilters;
        private IAuthorizationFilter[] _authorizationFilters;
        private IAuthenticationFilter[] _authenticationFilters;
        private IExceptionFilter[] _exceptionFilters;

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            // evaluate the 'filters' enumerable only once since the operation can be quite expensive
            List<FilterInfo> orderedCache = filters.ToList();

            List<FilterInfo> overrides = orderedCache.Where(f => f.Instance is IOverrideFilter).ToList();

            FilterScope actionOverride = SelectLastOverrideScope<IActionFilter>(overrides);
            FilterScope authorizationOverride = SelectLastOverrideScope<IAuthorizationFilter>(overrides);
            FilterScope authenticationOverride = SelectLastOverrideScope<IAuthenticationFilter>(overrides);
            FilterScope exceptionOverride = SelectLastOverrideScope<IExceptionFilter>(overrides);

            _actionFilters = SelectAvailable<IActionFilter>(orderedCache, actionOverride);
            _authorizationFilters = SelectAvailable<IAuthorizationFilter>(orderedCache, authorizationOverride);
            _authenticationFilters = SelectAvailable<IAuthenticationFilter>(orderedCache, authenticationOverride);
            _exceptionFilters = SelectAvailable<IExceptionFilter>(orderedCache, exceptionOverride);
        }

        public IActionFilter[] ActionFilters
        {
            get { return _actionFilters; }
        }

        public IAuthorizationFilter[] AuthorizationFilters
        {
            get { return _authorizationFilters; }
        }

        public IAuthenticationFilter[] AuthenticationFilters
        {
            get { return _authenticationFilters; }
        }

        public IExceptionFilter[] ExceptionFilters
        {
            get { return _exceptionFilters; }
        }

        private static T[] SelectAvailable<T>(List<FilterInfo> filters, FilterScope overrideFiltersBeforeScope)
        {
            // Determine which filters are available for this filter type, given the current overrides in place.
            // A filter should be processed if:
            //  1. It implements the appropriate interface for this filter type.
            //  2. It has not been overridden (its scope is not before the scope of the last override for this
            //     type).
            return filters.Where(f => f.Scope >= overrideFiltersBeforeScope
                && (f.Instance is T)).Select(f => (T)f.Instance).ToArray();
        }

        private static FilterScope SelectLastOverrideScope<T>(List<FilterInfo> overrideFilters)
        {
            // A filter type (such as action filter) can be overridden, which means every filter of that type at an
            // earlier scope must be ignored. Determine the scope of the last override filter (if any). Only
            // filters at this scope or later will be processed.

            FilterInfo lastOverride = overrideFilters.Where(
                f => ((IOverrideFilter)f.Instance).FiltersToOverride == typeof(T)).LastOrDefault();

            // If no override is present, the filter is not overridden (and filters at any scope, starting with
            // First are processed). Not overriding a filter is equivalent to placing an override at the First
            // filter scope (since there's nothing before First to override).
            if (lastOverride == null)
            {
                return FilterScope.Global;
            }

            return lastOverride.Scope;
        }
    }
}
