// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    internal interface IViewStartPageChild
    {
        HtmlHelper<object> Html { get; }
        UrlHelper Url { get; }
        ViewContext ViewContext { get; }
    }
}
