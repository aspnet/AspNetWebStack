// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class CopyAsyncParametersAttributeTest
    {
        [Fact]
        public void OnActionExecuting_CopiesParametersIfControllerIsAsync()
        {
            // Arrange
            CopyAsyncParametersAttribute attr = new CopyAsyncParametersAttribute();
            SampleAsyncController controller = new SampleAsyncController();

            ActionExecutingContext filterContext = new ActionExecutingContext
            {
                ActionParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                Controller = controller
            };
            filterContext.ActionParameters["foo"] = "fooAction";
            filterContext.ActionParameters["bar"] = "barAction";
            controller.AsyncManager.Parameters["bar"] = "barAsync";
            controller.AsyncManager.Parameters["baz"] = "bazAsync";

            // Act
            attr.OnActionExecuting(filterContext);

            // Assert
            Assert.Equal("fooAction", controller.AsyncManager.Parameters["foo"]);
            Assert.Equal("barAction", controller.AsyncManager.Parameters["bar"]);
            Assert.Equal("bazAsync", controller.AsyncManager.Parameters["baz"]);
        }

        [Fact]
        public void OnActionExecuting_DoesNothingIfControllerNotAsync()
        {
            // Arrange
            CopyAsyncParametersAttribute attr = new CopyAsyncParametersAttribute();
            SampleSyncController controller = new SampleSyncController();

            ActionExecutingContext filterContext = new ActionExecutingContext
            {
                ActionParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                Controller = controller
            };
            filterContext.ActionParameters["foo"] = "originalFoo";
            filterContext.ActionParameters["bar"] = "originalBar";

            // Act
            attr.OnActionExecuting(filterContext);

            // Assert
            // If we got this far without crashing, life is good :)
        }

        [Fact]
        public void OnActionExecuting_ThrowsIfFilterContextIsNull()
        {
            // Arrange
            CopyAsyncParametersAttribute attr = new CopyAsyncParametersAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnActionExecuting(null); }, "filterContext");
        }

        private class SampleSyncController : Controller
        {
        }

        private class SampleAsyncController : AsyncController
        {
        }
    }
}
