// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Web.Mvc
{
    internal static class TagBuilderExtensions
    {
        internal static MvcHtmlString ToMvcHtmlString(this TagBuilder tagBuilder, TagRenderMode renderMode)
        {
            Debug.Assert(tagBuilder != null);
            return new MvcHtmlString(tagBuilder.ToString(renderMode));
        }
    }
}
