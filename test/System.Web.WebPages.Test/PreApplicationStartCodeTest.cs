// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
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
                // Call a second time to ensure multiple calls do not cause issues
                PreApplicationStartCode.Start();

                Assert.False(RouteTable.Routes.RouteExistingFiles, "We should not be setting RouteExistingFiles");
                Assert.Empty(RouteTable.Routes);

                Assert.False(PageParser.EnableLongStringsAsResources);

                string formsAuthLoginUrl = (string)typeof(FormsAuthentication).GetField("_LoginUrl", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.Null(formsAuthLoginUrl);
            });
        }

        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
