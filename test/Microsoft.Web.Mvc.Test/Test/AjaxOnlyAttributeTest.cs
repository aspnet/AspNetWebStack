// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class AjaxOnlyAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHeaderNotPresent()
        {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();
            ControllerContext controllerContext = GetControllerContext(containsHeader: false);

            // Act
            bool isValid = attr.IsValidForRequest(controllerContext, null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHeaderIsPresent()
        {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();
            ControllerContext controllerContext = GetControllerContext(containsHeader: true);

            // Act
            bool isValid = attr.IsValidForRequest(controllerContext, null);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            // Arrange
            AjaxOnlyAttribute attr = new AjaxOnlyAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.IsValidForRequest(null, null); }, "controllerContext");
        }

        private static ControllerContext GetControllerContext(bool containsHeader)
        {
            Mock<ControllerContext> mockContext = new Mock<ControllerContext>();

            NameValueCollection nvc = new NameValueCollection();
            if (containsHeader)
            {
                nvc["X-Requested-With"] = "XMLHttpRequest";
            }

            mockContext.Setup(o => o.HttpContext.Request.Headers).Returns(nvc);
            mockContext.Setup(o => o.HttpContext.Request["X-Requested-With"]).Returns("XMLHttpRequest"); // always assume the request contains this, e.g. as a form value

            return mockContext.Object;
        }
    }
}
