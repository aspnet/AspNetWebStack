// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class ConfigurationFilterProviderTest
    {
        private readonly ConfigurationFilterProvider provider = new ConfigurationFilterProvider();

        [Fact]
        public void GetFilters_IfContextParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                provider.GetFilters(configuration: null, actionDescriptor: null);
            }, "configuration");
        }

        [Fact]
        public void GetFilters_ReturnsFiltersFromConfiguration()
        {
            var config = new HttpConfiguration();
            IFilter filter1 = new Mock<IFilter>().Object;
            config.Filters.Add(filter1);

            var result = provider.GetFilters(config, actionDescriptor: null);

            Assert.True(result.All(f => f.Scope == FilterScope.Global));
            Assert.Same(filter1, result.ToArray()[0].Instance);
        }
    }
}
