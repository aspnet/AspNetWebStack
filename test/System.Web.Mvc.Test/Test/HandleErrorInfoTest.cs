// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HandleErrorInfoTest
    {
        [Fact]
        public void ConstructorSetsProperties()
        {
            // Arrange
            Exception exception = new Exception();
            string controller = "SomeController";
            string action = "SomeAction";

            // Act
            HandleErrorInfo viewData = new HandleErrorInfo(exception, controller, action);

            // Assert
            Assert.Same(exception, viewData.Exception);
            Assert.Equal(controller, viewData.ControllerName);
            Assert.Equal(action, viewData.ActionName);
        }

        [Fact]
        public void ConstructorWithEmptyActionThrows()
        {
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new HandleErrorInfo(new Exception(), "SomeController", String.Empty); }, "actionName");
        }

        [Fact]
        public void ConstructorWithEmptyControllerThrows()
        {
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new HandleErrorInfo(new Exception(), String.Empty, "SomeAction"); }, "controllerName");
        }

        [Fact]
        public void ConstructorWithNullActionThrows()
        {
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new HandleErrorInfo(new Exception(), "SomeController", null /* action */); }, "actionName");
        }

        [Fact]
        public void ConstructorWithNullControllerThrows()
        {
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new HandleErrorInfo(new Exception(), null /* controller */, "SomeAction"); }, "controllerName");
        }

        [Fact]
        public void ConstructorWithNullExceptionThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new HandleErrorInfo(null /* exception */, "SomeController", "SomeAction"); }, "exception");
        }

        [Fact]
        public void ErrorHandlingDoesNotFireIfCalledInChildAction()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();
            Mock<ExceptionContext> context = new Mock<ExceptionContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act
            attr.OnException(context.Object);

            // Assert
            Assert.IsType<EmptyResult>(context.Object.Result);
        }
    }
}
