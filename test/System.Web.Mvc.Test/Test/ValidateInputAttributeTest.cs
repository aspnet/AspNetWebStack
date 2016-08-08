// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ValidateInputAttributeTest
    {
        [Fact]
        public void EnableValidationProperty()
        {
            // Act
            ValidateInputAttribute attrTrue = new ValidateInputAttribute(true);
            ValidateInputAttribute attrFalse = new ValidateInputAttribute(false);

            // Assert
            Assert.True(attrTrue.EnableValidation);
            Assert.False(attrFalse.EnableValidation);
        }

        [Fact]
        public void OnAuthorizationSetsControllerValidateRequestToFalse()
        {
            // Arrange
            Controller controller = new EmptyController() { ValidateRequest = true };
            ValidateInputAttribute attr = new ValidateInputAttribute(enableValidation: false);
            AuthorizationContext authContext = GetAuthorizationContext(controller);

            // Act
            attr.OnAuthorization(authContext);

            // Assert
            Assert.False(controller.ValidateRequest);
        }

        [Fact]
        public void OnAuthorizationSetsControllerValidateRequestToTrue()
        {
            // Arrange
            Controller controller = new EmptyController() { ValidateRequest = false };
            ValidateInputAttribute attr = new ValidateInputAttribute(enableValidation: true);
            AuthorizationContext authContext = GetAuthorizationContext(controller);

            // Act
            attr.OnAuthorization(authContext);

            // Assert
            Assert.True(controller.ValidateRequest);
        }

        [Fact]
        public void OnAuthorizationThrowsIfFilterContextIsNull()
        {
            // Arrange
            ValidateInputAttribute attr = new ValidateInputAttribute(true);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnAuthorization(null); }, "filterContext");
        }

        private static AuthorizationContext GetAuthorizationContext(ControllerBase controller)
        {
            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.Controller).Returns(controller);
            return mockAuthContext.Object;
        }

        private class EmptyController : Controller
        {
        }
    }
}
