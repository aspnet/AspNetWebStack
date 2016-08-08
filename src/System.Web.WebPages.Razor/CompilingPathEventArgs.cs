// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages.Razor
{
    public class CompilingPathEventArgs : EventArgs
    {
        public CompilingPathEventArgs(string virtualPath, WebPageRazorHost host)
        {
            VirtualPath = virtualPath;
            Host = host;
        }

        public string VirtualPath { get; private set; }
        public WebPageRazorHost Host { get; set; }
    }
}
