// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.WebPages.Scope;

namespace System.Web.Mvc
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        private static bool _startWasCalled;

        public static void Start()
        {
            // Guard against multiple calls. All Start calls are made on same thread, so no lock needed here
            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            WebPages.Razor.PreApplicationStartCode.Start();
            WebPages.PreApplicationStartCode.Start();

            ViewContext.GlobalScopeThunk = () => ScopeStorage.CurrentScope;
        }
    }
}
