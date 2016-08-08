// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Web.WebPages.ApplicationParts
{
    // IVirtualPathFactory that keeps track of a mapping from virtual paths to handler factories
    internal class DictionaryBasedVirtualPathFactory : IVirtualPathFactory
    {
        private Dictionary<string, Func<object>> _factories = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase);

        internal void RegisterPath(string virtualPath, Func<object> factory)
        {
            _factories[virtualPath] = factory;
        }

        public bool Exists(string virtualPath)
        {
            return _factories.ContainsKey(virtualPath);
        }

        public object CreateInstance(string virtualPath)
        {
            // Instantiate an object for the path
            // Note that this fails if it doesn't exist.  PathExists is assumed to have been called first
            Debug.Assert(Exists(virtualPath));
            return _factories[virtualPath]();
        }
    }
}
