// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// A single route that is the composite of multiple "sub routes".  
    /// </summary>
    /// <remarks>
    /// Corresponds to the MVC implementation of attribute routing in System.Web.Mvc.Routing.RouteCollectionRoute.
    /// </remarks>
    internal class RouteCollectionRoute : IHttpRoute, IReadOnlyCollection<IHttpRoute>
    {
        // Key for accessing SubRoutes on a RouteData.
        // We expose this through the RouteData.Values instead of a derived class because 
        // RouteData can get wrapped in another type, but Values still gets persisted through the wrappers. 
        // Prefix with a \0 to protect against conflicts with user keys. 
        public const string SubRouteDataKey = "MS_SubRoutes";

        private IReadOnlyCollection<IHttpRoute> _subRoutes;

        private static readonly IDictionary<string, object> _empty = EmptyReadOnlyDictionary<string, object>.Value;
        
        public RouteCollectionRoute()
        {
        }

        // This will enumerate all controllers and action descriptors, which will run those 
        // Initialization hooks, which may try to initialize controller-specific config, which
        // may call back to the initialize hook. So guard against that reentrancy.
        private bool _beingInitialized;

        // deferred hook for initializing the sub routes. The composite route can be added during the middle of 
        // intializing, but then the actual sub routes can get populated after initialization has finished. 
        public void EnsureInitialized(Func<IReadOnlyCollection<IHttpRoute>> initializer)
        {
            if (_beingInitialized && _subRoutes == null)
            {
                // Avoid reentrant initialization
                return;
            }

            try
            {
                _beingInitialized = true;

                _subRoutes = initializer();
                Contract.Assert(_subRoutes != null);                
            }
            finally
            {
                _beingInitialized = false;
            }
        }

        private IReadOnlyCollection<IHttpRoute> SubRoutes
        {
            get
            {
                // Caller should have already explicitly called EnsureInitialize. 
                // Avoid lazy initilization from within the route table because the route table
                // is shared resource and init can happen 
                if (_subRoutes == null)
                {
                    string msg = Error.Format(SRResources.Object_NotYetInitialized);
                    throw new InvalidOperationException(msg);
                }

                return _subRoutes;
            }
        }

        public string RouteTemplate
        {
            get { return String.Empty; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _empty; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _empty; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return null; }
        }

        public HttpMessageHandler Handler
        {
            get
            {
                return null;
            }
        }
                
        // Returns null if no match. 
        // Else, returns a composite route data that encapsulates the possible routes this may match against. 
        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            List<IHttpRouteData> matches = new List<IHttpRouteData>();
            foreach (IHttpRoute route in SubRoutes)
            {
                IHttpRouteData match = route.GetRouteData(virtualPathRoot, request);
                if (match != null)
                {
                    matches.Add(match);
                }
            }
            if (matches.Count == 0)
            {
                return null;  // no matches
            }

            return new RouteCollectionRouteData(this, matches.ToArray());
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            // Use LinkGenerationRoute stubs to get placeholders for all the sub routes. 
            return null;
        }

        public int Count
        {
            get { return SubRoutes.Count; }
        }

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        // Represents a union of multiple IHttpRouteDatas. 
        private class RouteCollectionRouteData : IHttpRouteData
        {
            public RouteCollectionRouteData(IHttpRoute parent, IHttpRouteData[] subRouteDatas)
            {
                Route = parent;

                // Each sub route may have different values. Callers need to enumerate the subroutes 
                // and individually query each. 
                // Find sub-routes via the SubRouteDataKey; don't expose as a property since the RouteData 
                // can be wrapped in an outer type that doesn't propagate properties. 
                Values = new HttpRouteValueDictionary() { { SubRouteDataKey, subRouteDatas } };
            }

            public IHttpRoute Route { get; private set; }

            public IDictionary<string, object> Values { get; private set; }
        }        
    }
}