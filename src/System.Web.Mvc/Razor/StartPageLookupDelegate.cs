// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages;

namespace System.Web.Mvc.Razor
{
    internal delegate WebPageRenderingBase StartPageLookupDelegate(WebPageRenderingBase page, string fileName, IEnumerable<string> supportedExtensions);
}
