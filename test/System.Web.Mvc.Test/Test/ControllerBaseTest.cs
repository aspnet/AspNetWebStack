// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Routing;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ControllerBaseTest
    {
        [Fact]
        public void ExecuteCallsControllerBaseExecute()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(HttpContextHelpers.GetMockHttpContext().Object, new RouteData());

            Mock<ControllerBaseHelper> mockController = new Mock<ControllerBaseHelper>() { CallBase = true };
            mockController.Setup(c => c.PublicInitialize(requestContext)).Verifiable();
            mockController.Setup(c => c.PublicExecuteCore()).Verifiable();
            IController controller = mockController.Object;

            // Act
            controller.Execute(requestContext);

            // Assert
            mockController.Verify();
        }

        [Fact]
        public void ExecuteThrowsIfCalledTwice()
        {
            // Arrange
            EmptyControllerBase controller = new EmptyControllerBase();
            RequestContext requestContext = new RequestContext(HttpContextHelpers.GetMockHttpContext().Object, new RouteData());

            // Act
            ((IController)controller).Execute(requestContext); // first call
            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    ((IController)controller).Execute(requestContext); // second call
                },
                @"A single instance of controller 'System.Web.Mvc.Test.ControllerBaseTest+EmptyControllerBase' cannot be used to handle multiple requests. If a custom controller factory is in use, make sure that it creates a new instance of the controller for each request.");

            // Assert
            Assert.Equal(1, controller.NumTimesExecuteCoreCalled);
        }

        [Fact]
        public void ExecuteThrowsIfRequestContextIsNull()
        {
            // Arrange
            IController controller = new ControllerBaseHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { controller.Execute(null); }, "requestContext");
        }

        [Fact]
        public void ExecuteThrowsIfRequestContextHttpContextIsNull()
        {
            //Arrange
            IController controller = new ControllerBaseHelper();

            //Act & Assert
            Assert.Throws<ArgumentException>(
                delegate { controller.Execute(new Mock<RequestContext>().Object); }, "Cannot execute Controller with a null HttpContext.\r\nParameter name: requestContext");
        }

        [Fact]
        public void InitializeSetsControllerContext()
        {
            // Arrange
            ControllerBaseHelper helper = new ControllerBaseHelper();
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());

            // Act
            helper.PublicInitialize(requestContext);

            // Assert
            Assert.Same(requestContext.HttpContext, helper.ControllerContext.HttpContext);
            Assert.Same(requestContext.RouteData, helper.ControllerContext.RouteData);
            Assert.Same(helper, helper.ControllerContext.Controller);
        }

        [Fact]
        public void TempDataProperty()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(controller, "TempData", new TempDataDictionary());
        }

        [Fact]
        public void TempDataReturnsParentTempDataWhenInChildRequest()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();
            ViewContext viewContext = new ViewContext { TempData = tempData };
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.ParentActionViewContextToken] = viewContext;
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, routeData);
            ControllerBaseHelper controller = new ControllerBaseHelper();
            controller.PublicInitialize(requestContext);

            // Act
            TempDataDictionary result = controller.TempData;

            // Assert
            Assert.Same(result, tempData);
        }

        [Fact]
        public void ValidateRequestProperty()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();

            // Act & assert
            MemberHelper.TestBooleanProperty(controller, "ValidateRequest", true /* initialValue */, false /* testDefaultValue */);
        }

        [Fact]
        public void ValueProviderProperty()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();
            IValueProvider valueProvider = new SimpleValueProvider();

            // Act & assert
            ValueProviderFactory[] originalFactories = ValueProviderFactories.Factories.ToArray();
            try
            {
                ValueProviderFactories.Factories.Clear();
                MemberHelper.TestPropertyWithDefaultInstance(controller, "ValueProvider", valueProvider);
            }
            finally
            {
                foreach (ValueProviderFactory factory in originalFactories)
                {
                    ValueProviderFactories.Factories.Add(factory);
                }
            }
        }

        [Fact]
        public void ViewDataProperty()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(controller, "ViewData", new ViewDataDictionary());
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();
            controller.ViewData["A"] = 1;

            // Act & Assert
            Assert.NotNull(controller.ViewBag);
            Assert.Equal(1, controller.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataInstance()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();
            controller.ViewData["A"] = 1;
            controller.ViewData = new ViewDataDictionary() { { "A", "bar" } };

            // Act & Assert
            Assert.Equal("bar", controller.ViewBag.A);
        }

        [Fact]
        public void ViewBag_PropagatesChangesToViewData()
        {
            // Arrange
            ControllerBase controller = new ControllerBaseHelper();
            controller.ViewData["A"] = 1;

            // Act
            controller.ViewBag.A = "foo";
            controller.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", controller.ViewData["A"]);
            Assert.Equal(2, controller.ViewData["B"]);
        }

        public class ControllerBaseHelper : ControllerBase
        {
            protected override void Initialize(RequestContext requestContext)
            {
                PublicInitialize(requestContext);
            }

            public virtual void PublicInitialize(RequestContext requestContext)
            {
                base.Initialize(requestContext);
            }

            protected override void ExecuteCore()
            {
                PublicExecuteCore();
            }

            public virtual void PublicExecuteCore()
            {
                throw new NotImplementedException();
            }
        }

        private class EmptyControllerBase : ControllerBase
        {
            public int NumTimesExecuteCoreCalled = 0;

            protected override void ExecuteCore()
            {
                NumTimesExecuteCoreCalled++;
            }
        }
    }
}
