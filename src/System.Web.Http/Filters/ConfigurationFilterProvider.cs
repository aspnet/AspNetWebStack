// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public class ConfigurationFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration.Filters;
        }
    }
}
