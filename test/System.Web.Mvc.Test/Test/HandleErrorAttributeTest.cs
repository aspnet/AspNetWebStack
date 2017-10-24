// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Routing;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HandleErrorAttributeTest
    {
        [Fact]
        public void HandleErrorAttributeReturnsUniqueTypeIDs()
        {
            // Arrange
            HandleErrorAttribute attr1 = new HandleErrorAttribute();
            HandleErrorAttribute attr2 = new HandleErrorAttribute();

            // Assert
            Assert.NotEqual(attr1.TypeId, attr2.TypeId);
        }

        [HandleError(View = "foo")]
        [HandleError(View = "bar")]
        private class ClassWithMultipleHandleErrorAttributes
        {
        }

        [Fact]
        public void CanRetrieveMultipleAuthorizeAttributesFromOneClass()
        {
            // Arrange
            ClassWithMultipleHandleErrorAttributes @class = new ClassWithMultipleHandleErrorAttributes();

            // Act
            IEnumerable<HandleErrorAttribute> attributes = TypeDescriptor.GetAttributes(@class).OfType<HandleErrorAttribute>();

            // Assert
            Assert.Equal(2, attributes.Count());
            Assert.Contains(attributes, a => a.View == "foo");
            Assert.Contains(attributes, a => a.View == "bar");
        }

        [Fact]
        public void ExceptionTypeProperty()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act
            Type origType = attr.ExceptionType;
            attr.ExceptionType = typeof(SystemException);
            Type newType = attr.ExceptionType;

            // Assert
            Assert.Equal(typeof(Exception), origType);
            Assert.Equal(typeof(SystemException), attr.ExceptionType);
        }

        [Fact]
        public void ExceptionTypePropertyWithNonExceptionTypeThrows()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                delegate { attr.ExceptionType = typeof(string); },
                "The type 'System.String' does not inherit from Exception.");
        }

        [Fact]
        public void ExceptionTypePropertyWithNullValueThrows()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { attr.ExceptionType = null; }, "value");
        }

        [Fact]
        public void MasterProperty()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act & Assert
            MemberHelper.TestStringProperty(attr, "Master", String.Empty);
        }

        [Fact]
        public void OnException()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute()
            {
                View = "SomeView",
                Master = "SomeMaster",
                ExceptionType = typeof(ArgumentException)
            };
            Exception exception = new ArgumentNullException();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            mockHttpContext.Setup(c => c.IsCustomErrorEnabled).Returns(true);
            mockHttpContext.Setup(c => c.Session).Returns((HttpSessionStateBase)null);
            mockHttpContext.Setup(c => c.Response.Clear()).Verifiable();
            mockHttpContext.SetupSet(c => c.Response.StatusCode = 500).Verifiable();
            mockHttpContext.SetupSet(c => c.Response.TrySkipIisCustomErrors = true).Verifiable();

            TempDataDictionary tempData = new TempDataDictionary();
            IViewEngine viewEngine = new Mock<IViewEngine>().Object;
            Controller controller = new Mock<Controller>().Object;
            controller.TempData = tempData;

            ExceptionContext context = GetExceptionContext(mockHttpContext.Object, controller, exception);

            // Exception
            attr.OnException(context);

            // Assert
            mockHttpContext.Verify();
            ViewResult viewResult = context.Result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(tempData, viewResult.TempData);
            Assert.Equal("SomeView", viewResult.ViewName);
            Assert.Equal("SomeMaster", viewResult.MasterName);

            HandleErrorInfo viewData = viewResult.ViewData.Model as HandleErrorInfo;
            Assert.NotNull(viewData);
            Assert.Same(exception, viewData.Exception);
            Assert.Equal("SomeController", viewData.ControllerName);
            Assert.Equal("SomeAction", viewData.ActionName);
        }

        [Fact]
        public void OnExceptionWithCustomErrorsDisabledDoesNothing()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();
            ActionResult result = new EmptyResult();
            ExceptionContext context = GetExceptionContext(GetHttpContext(false), null, new Exception());
            context.Result = result;

            // Exception
            attr.OnException(context);

            // Assert
            Assert.Same(result, context.Result);
        }

        [Fact]
        public void OnExceptionWithExceptionHandledDoesNothing()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();
            ActionResult result = new EmptyResult();
            ExceptionContext context = GetExceptionContext(GetHttpContext(), null, new Exception());
            context.Result = result;
            context.ExceptionHandled = true;

            // Exception
            attr.OnException(context);

            // Assert
            Assert.Same(result, context.Result);
        }

        [Fact]
        public void OnExceptionWithNon500ExceptionDoesNothing()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();
            ActionResult result = new EmptyResult();
            ExceptionContext context = GetExceptionContext(GetHttpContext(), null, new HttpException(404, "Some Exception"));
            context.Result = result;

            // Exception
            attr.OnException(context);

            // Assert
            Assert.Same(result, context.Result);
        }

        [Fact]
        public void OnExceptionWithNullFilterContextThrows()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnException(null /* filterContext */); }, "filterContext");
        }

        [Fact]
        public void OnExceptionWithWrongExceptionTypeDoesNothing()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute() { ExceptionType = typeof(ArgumentException) };
            ActionResult result = new EmptyResult();
            ExceptionContext context = GetExceptionContext(GetHttpContext(), null, new InvalidCastException());
            context.Result = result;

            // Exception
            attr.OnException(context);

            // Assert
            Assert.Same(result, context.Result);
        }

        [Fact]
        public void ViewProperty()
        {
            // Arrange
            HandleErrorAttribute attr = new HandleErrorAttribute();

            // Act & Assert
            MemberHelper.TestStringProperty(attr, "View", "Error", nullAndEmptyReturnValue: "Error");
        }

        private static ExceptionContext GetExceptionContext(HttpContextBase httpContext, ControllerBase controller, Exception exception)
        {
            RouteData rd = new RouteData();
            rd.Values["controller"] = "SomeController";
            rd.Values["action"] = "SomeAction";

            Mock<ExceptionContext> mockExceptionContext = new Mock<ExceptionContext>();
            mockExceptionContext.Setup(c => c.Controller).Returns(controller);
            mockExceptionContext.Setup(c => c.Exception).Returns(exception);
            mockExceptionContext.Setup(c => c.RouteData).Returns(rd);
            mockExceptionContext.Setup(c => c.HttpContext).Returns(httpContext);
            return mockExceptionContext.Object;
        }

        private static HttpContextBase GetHttpContext()
        {
            return GetHttpContext(true);
        }

        private static HttpContextBase GetHttpContext(bool isCustomErrorEnabled)
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            mockContext.Setup(c => c.IsCustomErrorEnabled).Returns(isCustomErrorEnabled);
            return mockContext.Object;
        }
    }
}
