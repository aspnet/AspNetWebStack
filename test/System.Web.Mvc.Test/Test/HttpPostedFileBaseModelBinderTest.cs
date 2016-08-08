// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpPostedFileBaseModelBinderTest
    {
        [Fact]
        public void BindModelReturnsEmptyResultIfEmptyFileInputElementInPost()
        {
            // Arrange
            Mock<HttpPostedFileBase> mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(0);
            mockFile.Setup(f => f.FileName).Returns(String.Empty);
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.HttpContext.Request.Files["fileName"]).Returns(mockFile.Object);

            HttpPostedFileBaseModelBinder binder = new HttpPostedFileBaseModelBinder();
            ModelBindingContext bindingContext = new ModelBindingContext() { ModelName = "fileName" };

            // Act
            object result = binder.BindModel(mockControllerContext.Object, bindingContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BindModelReturnsNullIfNoFileInputElementInPost()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Files["fileName"]).Returns((HttpPostedFileBase)null);

            HttpPostedFileBaseModelBinder binder = new HttpPostedFileBaseModelBinder();
            ModelBindingContext bindingContext = new ModelBindingContext() { ModelName = "fileName" };

            // Act
            object result = binder.BindModel(mockControllerContext.Object, bindingContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BindModelReturnsResultIfFileFound()
        {
            // Arrange
            Mock<HttpPostedFileBase> mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(1234);
            mockFile.Setup(f => f.FileName).Returns("somefile");
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.HttpContext.Request.Files["fileName"]).Returns(mockFile.Object);

            HttpPostedFileBaseModelBinder binder = new HttpPostedFileBaseModelBinder();
            ModelBindingContext bindingContext = new ModelBindingContext() { ModelName = "fileName" };

            // Act
            object result = binder.BindModel(mockControllerContext.Object, bindingContext);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockFile.Object, result);
        }

        [Fact]
        public void BindModelThrowsIfBindingContextIsNull()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            HttpPostedFileBaseModelBinder binder = new HttpPostedFileBaseModelBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(controllerContext, null); }, "bindingContext");
        }

        [Fact]
        public void BindModelThrowsIfControllerContextIsNull()
        {
            // Arrange
            HttpPostedFileBaseModelBinder binder = new HttpPostedFileBaseModelBinder();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { binder.BindModel(null, null); }, "controllerContext");
        }
    }
}
