// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.WebPages.Razor;

namespace Microsoft.Web.Helpers
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        private static bool _startWasCalled;

        public static void Start()
        {
            // Even though ASP.NET will only call each PreAppStart once, we sometimes internally call one PreAppStart from 
            // another PreAppStart to ensure that things get initialized in the right order. ASP.NET does not guarantee the 
            // order so we have to guard against multiple calls.
            // All Start calls are made on same thread, so no lock needed here.

            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            // Auto import the Microsoft.Web.Helpers namespace to all apps that are executing.
            WebPageRazorHost.AddGlobalImport(typeof(PreApplicationStartCode).Namespace);
        }
    }
}
