// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.WebPages.Test
{
    public class HashVirtualPathFactory : IVirtualPathFactory
    {
        private IDictionary<string, object> _pages;

        public HashVirtualPathFactory(params WebPageExecutingBase[] pages)
        {
            _pages = pages.ToDictionary(p => p.VirtualPath, p => (object)p, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string virtualPath)
        {
            return _pages.ContainsKey(virtualPath);
        }

        public object CreateInstance(string virtualPath)
        {
            object value;
            if (_pages.TryGetValue(virtualPath, out value))
            {
                return value;
            }
            return null;
        }
    }
}
