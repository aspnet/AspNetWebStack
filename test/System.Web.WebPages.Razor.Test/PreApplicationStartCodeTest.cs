// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Compilation;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Razor.Test
{
    public class PreApplicationStartCodeTest
    {
        [Fact]
        public void StartTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();
                var buildProviders = typeof(BuildProvider).GetField("s_dynamicallyRegisteredProviders", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.Equal(2, buildProviders.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance).GetValue(buildProviders, new object[] { }));
            });
        }

        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
