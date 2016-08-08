// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class HelpPageAreaRegistrationTest
    {
        [Fact]
        public void AreaName()
        {
            HelpPageAreaRegistration area = new HelpPageAreaRegistration();
            Assert.Equal("HelpPage", area.AreaName);
        }

        [Fact]
        public void RegisterArea()
        {
            HelpPageAreaRegistration area = new HelpPageAreaRegistration();
            AreaRegistrationContext context = new AreaRegistrationContext("HelpPage", RouteTable.Routes);
            area.RegisterArea(context);
            Assert.NotEmpty(context.Routes);
            Route route = Assert.IsType<Route>(context.Routes[0]);
            Assert.Equal("Help/{action}/{apiId}", route.Url);
            Assert.Equal("Help", route.Defaults["controller"]);
            Assert.Equal("Index", route.Defaults["action"]);
        }
    }
}
