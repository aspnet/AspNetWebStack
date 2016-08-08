// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    internal sealed class WebPageMatch
    {
        public WebPageMatch(string matchedPath, string pathInfo)
        {
            MatchedPath = matchedPath;
            PathInfo = pathInfo;
        }

        public string MatchedPath { get; private set; }

        public string PathInfo { get; private set; }
    }
}
