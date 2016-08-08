// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    public class BrowserOverrideStores
    {
        private static BrowserOverrideStores _instance = new BrowserOverrideStores();
        private BrowserOverrideStore _currentOverrideStore = new CookieBrowserOverrideStore();

        /// <summary>
        /// The current BrowserOverrideStore
        /// </summary>
        public static BrowserOverrideStore Current
        {
            get { return _instance.CurrentInternal; }
            set { _instance.CurrentInternal = value; }
        }

        internal BrowserOverrideStore CurrentInternal
        {
            get { return _currentOverrideStore; }
            set { _currentOverrideStore = value ?? new RequestBrowserOverrideStore(); }
        }
    }
}
