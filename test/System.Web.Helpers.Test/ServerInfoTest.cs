// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class InfoTest
    {
        [Fact]
        public void ConfigurationReturnsExpectedInfo()
        {
            var configInfo = ServerInfo.Configuration();

            // verification
            // checks only subset of values
            Assert.NotNull(configInfo);
            VerifyKey(configInfo, "Machine Name");
            VerifyKey(configInfo, "OS Version");
            VerifyKey(configInfo, "ASP.NET Version");
            VerifyKey(configInfo, "ASP.NET Web Pages Version");
        }

        [Fact]
        public void EnvironmentVariablesReturnsExpectedInfo()
        {
            var envVariables = ServerInfo.EnvironmentVariables();

            // verification
            // checks only subset of values
            Assert.NotNull(envVariables);
            VerifyKey(envVariables, "Path");
            VerifyKey(envVariables, "SystemDrive");
        }

        [Fact]
        public void ServerVariablesReturnsExpectedInfoWithNoContext()
        {
            var serverVariables = ServerInfo.ServerVariables();

            // verification
            // since there is no HttpContext this will be empty
            Assert.NotNull(serverVariables);
        }

        [Fact]
        public void ServerVariablesReturnsExpectedInfoWthContext()
        {
            var serverVariables = new NameValueCollection();
            serverVariables.Add("foo", "bar");

            var request = new Mock<HttpRequestBase>();
            request.Setup(c => c.ServerVariables).Returns(serverVariables);

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request).Returns(request.Object);

            // verification
            Assert.NotNull(serverVariables);

            IDictionary<string, string> returnedValues = ServerInfo.ServerVariables(context.Object);
            Assert.Equal(serverVariables.Count, returnedValues.Count);
            foreach (var item in returnedValues)
            {
                Assert.Equal(serverVariables[item.Key], item.Value);
            }
        }

#if false // Avoid an NRE when accessing HttpRuntime.BinDirectory without initializing the runtime.
        [Fact]
        public void HttpRuntimeInfoReturnsExpectedInfo()
        {
            var httpRuntimeInfo = ServerInfo.HttpRuntimeInfo();

            // verification
            // checks only subset of values
            Assert.NotNull(httpRuntimeInfo);
            VerifyKey(httpRuntimeInfo, "CLR Install Directory");
            VerifyKey(httpRuntimeInfo, "Asp Install Directory");
            VerifyKey(httpRuntimeInfo, "On UNC Share");
        }
#endif

        [Fact]
        public void ServerInfoDoesNotProduceLegacyCasForHomogenousAppDomain()
        {
            // Act and Assert
            Action action = () =>
            {
                IDictionary<string, string> configValue = ServerInfo.LegacyCAS(AppDomain.CurrentDomain);

                Assert.NotNull(configValue);
                Assert.Equal(0, configValue.Count);
            };

            AppDomainUtils.RunInSeparateAppDomain(GetAppDomainSetup(legacyCasEnabled: false), action);
        }

        [Fact]
        public void ServerInfoProducesLegacyCasForNonHomogenousAppDomain()
        {
            // Arrange
            Action action = () =>
            {
                // Act and Assert
                IDictionary<string, string> configValue = ServerInfo.LegacyCAS(AppDomain.CurrentDomain);

                // Assert
                Assert.True(configValue.ContainsKey("Legacy Code Access Security"));
                Assert.Equal("Legacy Code Access Security has been detected on your system. Microsoft WebPage features require the ASP.NET 4 Code Access Security model. For information about how to resolve this, contact your server administrator.", configValue["Legacy Code Access Security"]);
            };

            AppDomainUtils.RunInSeparateAppDomain(GetAppDomainSetup(legacyCasEnabled: true), action);
        }

        //[Fact]
        //public void SqlServerInfoReturnsExpectedInfo() {
        //    var sqlInfo = ServerInfo.SqlServerInfo();

        //    // verification
        //    // just verifies that we don't get any unexpected exceptions
        //    Assert.NotNull(sqlInfo);
        //}

#if false // Avoid NREs when accessing HttpRuntime.BinDirectory without initializing the runtime.
        [Fact]
        public void RenderResultContainsExpectedTags()
        {
            var htmlString = ServerInfo.GetHtml().ToString();

            // just verify that the final HTML produced contains some expected info
            Assert.True(htmlString.Contains("<table class=\"server-info\" dir=\"ltr\">"));
            Assert.True(htmlString.Contains("</style>"));
            Assert.True(htmlString.Contains("Server Configuration"));
        }

        [Fact]
        public void RenderGeneratesValidXhtml()
        {
            // Result does not validate against XHTML 1.1 and HTML5 because ServerInfo generates
            // <style> inside <body>. This is by design however since we only use ServerInfo
            // as debugging aid, not something to be permanently added to a web page.
            XhtmlAssert.Validate1_0(
                ServerInfo.GetHtml(),
                addRoot: true
                );
        }
#endif

        private void VerifyKey(IDictionary<string, string> info, string key)
        {
            Assert.True(info.ContainsKey(key));
            Assert.False(String.IsNullOrEmpty(info[key]));
        }

        private AppDomainSetup GetAppDomainSetup(bool legacyCasEnabled)
        {
            var setup = new AppDomainSetup();
            if (legacyCasEnabled)
            {
                setup.SetCompatibilitySwitches(new[] { "NetFx40_LegacySecurityPolicy" });
            }
            return setup;
        }
    }
}
