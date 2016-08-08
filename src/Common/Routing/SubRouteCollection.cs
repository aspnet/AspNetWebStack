// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

#if ASPNETWEBAPI
using System.Web.Http.Properties;
using TRoute = System.Web.Http.Routing.IHttpRoute;
using TRouteEntry = System.Web.Http.Routing.RouteEntry;
#else
using System.Web.Mvc.Properties;
using TRoute = System.Web.Routing.Route;
using TRouteEntry = System.Web.Mvc.Routing.RouteEntry;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents a collection of route entries and the routes they contain.</summary>
    /// <remarks>
    /// This is used in attribute routing, where we want to match multiple routes, and then later
    /// disambiguate which one is best.
    /// </remarks>
    internal class SubRouteCollection : IReadOnlyCollection<TRoute>
    {
        private readonly List<TRoute> _routes = new List<TRoute>();
        private readonly List<TRouteEntry> _entries = new List<TRouteEntry>();

        public void Add(TRouteEntry entry)
        {
            Contract.Assert(entry != null);
            TRoute route = entry.Route;
            Contract.Assert(route != null);

            string name = entry.Name;

            if (name != null)
            {
                TRouteEntry duplicateEntry = _entries.SingleOrDefault((e) => e.Name == name);

                if (duplicateEntry != null)
                {
                    ThrowExceptionForDuplicateRouteNames(name, route, duplicateEntry.Route);
                }
            }

            _routes.Add(route);
            _entries.Add(entry);
        }

        public void AddRange(IEnumerable<TRouteEntry> entries)
        {
            foreach (RouteEntry entry in entries)
            {
                Add(entry);
            }
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public IEnumerator<TRoute> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_routes).GetEnumerator();
        }

        public IReadOnlyCollection<TRouteEntry> Entries
        {
            get { return _entries; }
        }

        private static void ThrowExceptionForDuplicateRouteNames(string name, TRoute route1, TRoute route2)
        {
#if ASPNETWEBAPI
            throw new InvalidOperationException(String.Format(
                CultureInfo.CurrentCulture,
                SRResources.SubRouteCollection_DuplicateRouteName,
                name,
                route1.RouteTemplate,
                route2.RouteTemplate));
#else
            throw new InvalidOperationException(String.Format(
                CultureInfo.CurrentCulture,
                MvcResources.SubRouteCollection_DuplicateRouteName,
                name,
                route1.Url,
                route2.Url));
#endif
        }
    }
}
