// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ActionExecutedContextTest
    {
        [Fact]
        public void ConstructorThrowsIfActionDescriptorIsNull()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionDescriptor actionDescriptor = null;
            bool canceled = true;
            Exception exception = null;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ActionExecutedContext(controllerContext, actionDescriptor, canceled, exception); }, "actionDescriptor");
        }

        [Fact]
        public void PropertiesAreSetByConstructor()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionDescriptor actionDescriptor = new Mock<ActionDescriptor>().Object;
            bool canceled = true;
            Exception exception = new Exception();

            // Act
            ActionExecutedContext actionExecutedContext = new ActionExecutedContext(controllerContext, actionDescriptor, canceled, exception);

            // Assert
            Assert.Equal(actionDescriptor, actionExecutedContext.ActionDescriptor);
            Assert.Equal(canceled, actionExecutedContext.Canceled);
            Assert.Equal(exception, actionExecutedContext.Exception);
        }

        [Fact]
        public void ResultProperty()
        {
            // Arrange
            ActionExecutedContext actionExecutedContext = new Mock<ActionExecutedContext>().Object;

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(actionExecutedContext, "Result", new ViewResult(), EmptyResult.Instance);
        }
    }
}
