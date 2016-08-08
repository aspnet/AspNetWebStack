// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.WebPages.Scope
{
    public class StaticScopeStorageProvider : IScopeStorageProvider
    {
        private static readonly IDictionary<object, object> _defaultContext =
            new ScopeStorageDictionary(null, new ConcurrentDictionary<object, object>(ScopeStorageComparer.Instance));

        private IDictionary<object, object> _currentContext;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The state storage API is designed to allow contexts to be set")]
        public IDictionary<object, object> CurrentScope
        {
            get { return _currentContext ?? _defaultContext; }
            set { _currentContext = value; }
        }

        public IDictionary<object, object> GlobalScope
        {
            get { return _defaultContext; }
        }
    }
}
