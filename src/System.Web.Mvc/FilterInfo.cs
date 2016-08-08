// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Mvc.Filters;

namespace System.Web.Mvc
{
    public class FilterInfo
    {
        private readonly List<IActionFilter> _actionFilters = new List<IActionFilter>();
        private readonly List<IAuthenticationFilter> _authenticationFilters = new List<IAuthenticationFilter>();
        private readonly List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
        private readonly List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();
        private readonly List<IResultFilter> _resultFilters = new List<IResultFilter>();

        public FilterInfo()
        {
        }

        public FilterInfo(IEnumerable<Filter> filters)
        {
            // Determine the override scope for each filter type and cache the filters list.
            OverrideFilterInfo processed = ProcessOverrideFilters(filters);
            // Split the cached filters list based on filter type and override scope.
            SplitFilters(processed);
        }

        public IList<IActionFilter> ActionFilters
        {
            get { return _actionFilters; }
        }

        public IList<IAuthenticationFilter> AuthenticationFilters
        {
            get { return _authenticationFilters; }
        }

        public IList<IAuthorizationFilter> AuthorizationFilters
        {
            get { return _authorizationFilters; }
        }

        public IList<IExceptionFilter> ExceptionFilters
        {
            get { return _exceptionFilters; }
        }

        public IList<IResultFilter> ResultFilters
        {
            get { return _resultFilters; }
        }

        private static OverrideFilterInfo ProcessOverrideFilters(IEnumerable<Filter> filters)
        {
            OverrideFilterInfo result = new OverrideFilterInfo
            {
                ActionOverrideScope = FilterScope.First,
                AuthenticationOverrideScope = FilterScope.First,
                AuthorizationOverrideScope = FilterScope.First,
                ExceptionOverrideScope = FilterScope.First,
                ResultOverrideScope = FilterScope.First,
                Filters = new List<Filter>()
            };

            // Evaluate the 'filters' enumerable only once since the operation can be quite expensive.
            foreach (Filter filter in filters)
            {
                if (filter == null)
                {
                    continue;
                }
                IOverrideFilter overrideFilter = filter.Instance as IOverrideFilter;

                if (overrideFilter != null)
                {
                    if (overrideFilter.FiltersToOverride == typeof(IActionFilter)
                        && filter.Scope >= result.ActionOverrideScope)
                    {
                        result.ActionOverrideScope = filter.Scope;
                    }
                    else if (overrideFilter.FiltersToOverride == typeof(IAuthenticationFilter)
                        && filter.Scope >= result.AuthenticationOverrideScope)
                    {
                        result.AuthenticationOverrideScope = filter.Scope;
                    }
                    else if (overrideFilter.FiltersToOverride == typeof(IAuthorizationFilter)
                        && filter.Scope >= result.AuthorizationOverrideScope)
                    {
                        result.AuthorizationOverrideScope = filter.Scope;
                    }
                    else if (overrideFilter.FiltersToOverride == typeof(IExceptionFilter)
                        && filter.Scope >= result.ExceptionOverrideScope)
                    {
                        result.ExceptionOverrideScope = filter.Scope;
                    }
                    else if (overrideFilter.FiltersToOverride == typeof(IResultFilter)
                        && filter.Scope >= result.ResultOverrideScope)
                    {
                        result.ResultOverrideScope = filter.Scope;
                    }
                }

                // Cache filters to avoid having to enumerate it again (expensive). Do so here to avoid an extra loop.
                result.Filters.Add(filter);
            }

            return result;
        }

        private void SplitFilters(OverrideFilterInfo info)
        {
            Contract.Assert(info.Filters != null);

            foreach (Filter filter in info.Filters)
            {
                Contract.Assert(filter != null);

                IActionFilter actionFilter = filter.Instance as IActionFilter;

                if (actionFilter != null && filter.Scope >= info.ActionOverrideScope)
                {
                    _actionFilters.Add(actionFilter);
                }

                IAuthenticationFilter authenticationFilter = filter.Instance as IAuthenticationFilter;

                if (authenticationFilter != null && filter.Scope >= info.AuthenticationOverrideScope)
                {
                    _authenticationFilters.Add(authenticationFilter);
                }

                IAuthorizationFilter authorizationFilter = filter.Instance as IAuthorizationFilter;

                if (authorizationFilter != null && filter.Scope >= info.AuthorizationOverrideScope)
                {
                    _authorizationFilters.Add(authorizationFilter);
                }

                IExceptionFilter exceptionFilter = filter.Instance as IExceptionFilter;

                if (exceptionFilter != null && filter.Scope >= info.ExceptionOverrideScope)
                {
                    _exceptionFilters.Add(exceptionFilter);
                }

                IResultFilter resultFilter = filter.Instance as IResultFilter;

                if (resultFilter != null && filter.Scope >= info.ResultOverrideScope)
                {
                    _resultFilters.Add(resultFilter);
                }
            }
        }

        private struct OverrideFilterInfo
        {
            public FilterScope ActionOverrideScope;
            public FilterScope AuthenticationOverrideScope;
            public FilterScope AuthorizationOverrideScope;
            public FilterScope ExceptionOverrideScope;
            public FilterScope ResultOverrideScope;

            public List<Filter> Filters;
        }
    }
}
