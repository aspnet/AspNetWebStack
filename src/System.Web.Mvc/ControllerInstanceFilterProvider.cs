// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public class ControllerInstanceFilterProvider : IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (controllerContext.Controller != null)
            {
                // Use FilterScope.First and Order of Int32.MinValue to ensure controller instance methods always run first
                yield return new Filter(controllerContext.Controller, FilterScope.First, Int32.MinValue);
            }
        }
    }
}
