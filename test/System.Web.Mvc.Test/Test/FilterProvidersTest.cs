// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class FilterProvidersTest
    {
        [Fact]
        public void DefaultFilterProviders()
        {
            // Assert
            Assert.NotNull(FilterProviders.Providers.Single(fp => fp is GlobalFilterCollection));
            Assert.NotNull(FilterProviders.Providers.Single(fp => fp is FilterAttributeFilterProvider));
            Assert.NotNull(FilterProviders.Providers.Single(fp => fp is ControllerInstanceFilterProvider));
        }
    }
}
