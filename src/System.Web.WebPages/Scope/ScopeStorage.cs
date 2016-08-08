// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.WebPages.Scope
{
    public static class ScopeStorage
    {
        private static readonly IScopeStorageProvider _defaultStorageProvider = new StaticScopeStorageProvider();
        private static IScopeStorageProvider _stateStorageProvider;

        public static IScopeStorageProvider CurrentProvider
        {
            get { return _stateStorageProvider ?? _defaultStorageProvider; }
            set { _stateStorageProvider = value; }
        }

        public static IDictionary<object, object> CurrentScope
        {
            get { return CurrentProvider.CurrentScope; }
        }

        public static IDictionary<object, object> GlobalScope
        {
            get { return CurrentProvider.GlobalScope; }
        }

        public static IDisposable CreateTransientScope(IDictionary<object, object> context)
        {
            var currentContext = CurrentScope;
            CurrentProvider.CurrentScope = context;
            return new DisposableAction(() => CurrentProvider.CurrentScope = currentContext); // Return an IDisposable that pops the item back off
        }

        public static IDisposable CreateTransientScope()
        {
            return CreateTransientScope(new ScopeStorageDictionary(baseScope: CurrentScope));
        }
    }
}
