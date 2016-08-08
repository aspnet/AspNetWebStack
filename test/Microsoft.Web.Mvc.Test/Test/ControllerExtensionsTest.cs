// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.Test
{
    public class ControllerExtensionsTest
    {
        private const string AppPathModifier = MvcHelper.AppPathModifier;

        [Fact]
        public void RedirectToAction_DifferentController()
        {
            // Act
            RedirectToRouteResult result = new SampleController().RedirectToAction<DifferentController>(x => x.SomeOtherMethod(84));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.RouteName);
            Assert.Equal(3, result.RouteValues.Count);
            Assert.Equal("Different", result.RouteValues["controller"]);
            Assert.Equal("SomeOtherMethod", result.RouteValues["action"]);
            Assert.Equal(84, result.RouteValues["someOtherParameter"]);
        }

        [Fact]
        public void RedirectToAction_SameController()
        {
            // Act
            RedirectToRouteResult result = new SampleController().RedirectToAction(x => x.SomeMethod(42));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.RouteName);
            Assert.Equal(3, result.RouteValues.Count);
            Assert.Equal("Sample", result.RouteValues["controller"]);
            Assert.Equal("SomeMethod", result.RouteValues["action"]);
            Assert.Equal(42, result.RouteValues["someParameter"]);
        }

        [Fact]
        public void RedirectToAction_ThrowsIfControllerIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ((SampleController)null).RedirectToAction(x => x.SomeMethod(42)); }, "controller");
        }

        private class SampleController : Controller
        {
            public ActionResult SomeMethod(int someParameter)
            {
                throw new NotImplementedException();
            }
        }

        private class DifferentController : Controller
        {
            public ActionResult SomeOtherMethod(int someOtherParameter)
            {
                throw new NotImplementedException();
            }
        }
    }
}
