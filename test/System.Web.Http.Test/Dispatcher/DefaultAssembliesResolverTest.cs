// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.Services;
using Microsoft.TestCommon;

namespace System.Web.Http.Dispatcher
{
    public class DefaultAssembliesResolverTest
    {
        [Fact]
        public void GetAssemblies_ContainsCurrentAssembly()
        {
            IAssembliesResolver ar = new DefaultAssembliesResolver();
            Assembly currentAssembly = typeof(DefaultAssembliesResolverTest).Assembly;

            Assert.True(ar.GetAssemblies().Contains(currentAssembly));
        }

        [Fact]
        public void Class_IsDefaultIAssembliesResolver()
        {
            var serviceResolver = new DefaultServices(new HttpConfiguration());

            Assert.IsType<DefaultAssembliesResolver>(serviceResolver.GetService(typeof(IAssembliesResolver)));
        }
    }
}
