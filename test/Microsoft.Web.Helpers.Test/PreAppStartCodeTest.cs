// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.WebPages.Razor;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace Microsoft.Web.Helpers.Test
{
    public class PreApplicationStartCodeTest
    {
        [Fact]
        public void StartTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Act
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();

                // Assert
                var imports = WebPageRazorHost.GetGlobalImports();
                Assert.Contains(imports, ns => ns.Equals("Microsoft.Web.Helpers"));
            });
        }

        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
