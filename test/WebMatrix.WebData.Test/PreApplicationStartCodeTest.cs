// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Security;
using System.Web.WebPages.Razor;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace WebMatrix.WebData.Test
{
    public class PreApplicationStartCodeTest
    {
        [Fact]
        public void StartRegistersRazorNamespaces()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();
                // Call a second time to ensure multiple calls do not cause issues
                PreApplicationStartCode.Start();

                // Verify namespaces
                var imports = WebPageRazorHost.GetGlobalImports();
                Assert.Contains(imports, ns => ns.Equals("WebMatrix.Data"));
                Assert.Contains(imports, ns => ns.Equals("WebMatrix.WebData"));
            });
        }

        [Fact]
        public void StartInitializesFormsAuthByDefault()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();

                string formsAuthLoginUrl = (string)typeof(FormsAuthentication).GetField("_LoginUrl", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.Equal(FormsAuthenticationSettings.DefaultLoginUrl, formsAuthLoginUrl);
            });
        }

        [Fact]
        public void StartDoesNotInitializeFormsAuthWhenDisabled()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                ConfigurationManager.AppSettings[WebSecurity.EnableSimpleMembershipKey] = "False";
                PreApplicationStartCode.Start();

                string formsAuthLoginUrl = (string)typeof(FormsAuthentication).GetField("_LoginUrl", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Assert.Null(formsAuthLoginUrl);
            });
        }

        [Fact]
        public void StartInitializesSimpleMembershipByDefault()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();

                // Verify simple membership
                var providers = Membership.Providers;
                Assert.NotEmpty(providers.OfType<SimpleMembershipProvider>());
                Assert.True(Roles.Enabled);
            });
        }

        [Fact]
        public void StartDoesNotInitializeSimpleMembershipWhenDisabled()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                AppDomainUtils.SetPreAppStartStage();
                ConfigurationManager.AppSettings[WebSecurity.EnableSimpleMembershipKey] = "False";
                PreApplicationStartCode.Start();

                // Verify simple membership
                var providers = Membership.Providers;
                Assert.Empty(providers.OfType<SimpleMembershipProvider>());
                Assert.False(Roles.Enabled);
            });
        }

        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
