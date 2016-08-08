// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class WebConfigTest
    {
        [Fact]
        public void WebConfig_HelpPageReferencesCorrectMvcHostVersion()
        {
            // Arrange
            Version mvcVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Mvc", typeof(Controller));
            string expectedFactoryType = "System.Web.Mvc.MvcWebRazorHostFactory, System.Web.Mvc, Version=" + mvcVersion
                + ", Culture=neutral, PublicKeyToken=31BF3856AD364E35";
            using (Stream webConfigStream = typeof(WebConfigTest).Assembly
                .GetManifestResourceStream(@"WebApiHelpPage.TestFiles.Web.config"))
            {
                XDocument document = XDocument.Load(webConfigStream);

                // Act
                string actualFactoryType = document.Root
                    .Element("system.web.webPages.razor")
                    .Element("host")
                    .Attribute("factoryType")
                    .Value;

                // Assert
                Assert.Equal(expectedFactoryType, actualFactoryType);
            }
        }
    }
}
