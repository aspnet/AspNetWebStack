// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public static class FilterProviders
    {
        static FilterProviders()
        {
            Providers = new FilterProviderCollection();
            Providers.Add(GlobalFilters.Filters);
            Providers.Add(new FilterAttributeFilterProvider());
            Providers.Add(new ControllerInstanceFilterProvider());
        }

        public static FilterProviderCollection Providers { get; private set; }
    }
}
