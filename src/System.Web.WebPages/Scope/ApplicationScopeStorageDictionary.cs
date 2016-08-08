// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Web.WebPages.Scope
{
    /// <summary>
    /// The application level storage context that uses a static dictionary as a backing store.
    /// </summary>
    internal class ApplicationScopeStorageDictionary : ScopeStorageDictionary
    {
        private static readonly IDictionary<object, object> _innerDictionary =
            new ConcurrentDictionary<object, object>(ScopeStorageComparer.Instance);

        public ApplicationScopeStorageDictionary()
            : this(new WebConfigScopeDictionary())
        {
        }

        public ApplicationScopeStorageDictionary(WebConfigScopeDictionary webConfigState)
            : base(baseScope: webConfigState, backingStore: _innerDictionary)
        {
        }
    }
}
