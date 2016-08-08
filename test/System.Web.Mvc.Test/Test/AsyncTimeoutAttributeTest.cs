// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class AsyncTimeoutAttributeTest
    {
        [Fact]
        public void ConstructorThrowsIfDurationIsOutOfRange()
        {
            // Act & assert
            Assert.ThrowsArgumentOutOfRange(() => new AsyncTimeoutAttribute(-1000), "duration",
                @"The timeout value must be non-negative or Timeout.Infinite.");
        }

        [Fact]
        public void DurationProperty()
        {
            // Act
            AsyncTimeoutAttribute attr = new AsyncTimeoutAttribute(45);

            // Assert
            Assert.Equal(45, attr.Duration);
        }

        [Fact]
        public void OnActionExecutingSetsTimeoutPropertyOnController()
        {
            // Arrange
            AsyncTimeoutAttribute attr = new AsyncTimeoutAttribute(45);

            MyAsyncController controller = new MyAsyncController();
            controller.AsyncManager.Timeout = 0;

            ActionExecutingContext filterContext = new ActionExecutingContext()
            {
                Controller = controller
            };

            // Act
            attr.OnActionExecuting(filterContext);

            // Assert
            Assert.Equal(45, controller.AsyncManager.Timeout);
        }

        [Fact]
        public void OnActionExecutingThrowsIfControllerIsNotAsyncManagerContainer()
        {
            // Arrange
            AsyncTimeoutAttribute attr = new AsyncTimeoutAttribute(45);

            ActionExecutingContext filterContext = new ActionExecutingContext()
            {
                Controller = new MyController()
            };

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { attr.OnActionExecuting(filterContext); },
                @"The controller of type 'System.Web.Mvc.Test.AsyncTimeoutAttributeTest+MyController' must subclass AsyncController or implement the IAsyncManagerContainer interface.");
        }

        [Fact]
        public void OnActionExecutingThrowsIfFilterContextIsNull()
        {
            // Arrange
            AsyncTimeoutAttribute attr = new AsyncTimeoutAttribute(45);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnActionExecuting(null); }, "filterContext");
        }

        private class MyController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }

        private class MyAsyncController : AsyncController
        {
        }
    }
}
