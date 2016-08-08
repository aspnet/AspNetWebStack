// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Manages a cache of <see cref="System.Web.Http.Controllers.IHttpController"/> types detected in the system.
    /// </summary>
    internal sealed class HttpControllerTypeCache
    {
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, ILookup<string, Type>>> _cache;

        public HttpControllerTypeCache(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
            _cache = new Lazy<Dictionary<string, ILookup<string, Type>>>(InitializeCache);
        }

        internal Dictionary<string, ILookup<string, Type>> Cache
        {
            get { return _cache.Value; }
        }

        public ICollection<Type> GetControllerTypes(string controllerName)
        {
            if (String.IsNullOrEmpty(controllerName))
            {
                throw Error.ArgumentNullOrEmpty("controllerName");
            }

            HashSet<Type> matchingTypes = new HashSet<Type>();

            ILookup<string, Type> namespaceLookup;
            if (_cache.Value.TryGetValue(controllerName, out namespaceLookup))
            {
                foreach (var namespaceGroup in namespaceLookup)
                {
                    matchingTypes.UnionWith(namespaceGroup);
                }
            }

            return matchingTypes;
        }

        private Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);
            var groupedByName = controllerTypes.GroupBy(
                t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length),
                StringComparer.OrdinalIgnoreCase);

            return groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
