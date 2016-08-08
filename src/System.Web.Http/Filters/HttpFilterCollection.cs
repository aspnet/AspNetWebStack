// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http.Properties;

namespace System.Web.Http.Filters
{
    public class HttpFilterCollection : IEnumerable<FilterInfo>
    {
        private readonly List<FilterInfo> _filters = new List<FilterInfo>();

        public int Count
        {
            get { return _filters.Count; }
        }

        public void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw Error.ArgumentNull("filter");
            }

            _filters.Add(CreateFilterInfo(filter));
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the filter collection.
        /// </summary>
        /// <param name="filters">The collection of filters to add.</param>
        public void AddRange(IEnumerable<IFilter> filters)
        {
            if (filters == null)
            {
                throw Error.ArgumentNull("filters");
            }

            // ToArray used here to avoid iterating the filter collection twice.
            IFilter[] cachedFilters = filters.ToArray();
            for (int i = 0; i < cachedFilters.Length; i++)
            {
                if (cachedFilters[i] == null)
                {
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, SRResources.CollectionParameterContainsNullElement, "filters"),
                        "filters");
                }
            }

            for (int i = 0; i < cachedFilters.Length; i++)
            {
                _filters.Add(CreateFilterInfo(cachedFilters[i]));
            }
        }

        private static FilterInfo CreateFilterInfo(IFilter filter)
        {
            Contract.Assert(filter != null);
            return new FilterInfo(filter, FilterScope.Global);
        }

        public void Clear()
        {
            _filters.Clear();
        }

        public bool Contains(IFilter filter)
        {
            return _filters.Any(f => f.Instance == filter);
        }

        public IEnumerator<FilterInfo> GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(IFilter filter)
        {
            _filters.RemoveAll(f => f.Instance == filter);
        }
    }
}
