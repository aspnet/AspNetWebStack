// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc.Async;
using System.Web.Mvc.Async.Test;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AsyncControllerTest
    {
        [Fact]
        public void ActionInvokerProperty()
        {
            // Arrange
            EmptyController controller = new EmptyController();

            // Act
            IActionInvoker invoker = controller.ActionInvoker;

            // Assert
            Assert.IsType<AsyncControllerActionInvoker>(invoker);
        }

        [Fact]
        public void AsyncManagerProperty()
        {
            // Arrange
            EmptyController controller = new EmptyController();

            // Act
            AsyncManager asyncManager = controller.AsyncManager;

            // Assert
            Assert.NotNull(asyncManager);
        }

        [Fact]
        public void Execute_ThrowsIfCalledMoreThanOnce()
        {
            // Arrange
            IAsyncController controller = new EmptyController();
            RequestContext requestContext = GetRequestContext("SomeAction");

            // Act & assert
            controller.BeginExecute(requestContext, null, null);
            Assert.Throws<InvalidOperationException>(
                delegate { controller.BeginExecute(requestContext, null, null); },
                @"A single instance of controller 'System.Web.Mvc.Test.AsyncControllerTest+EmptyController' cannot be used to handle multiple requests. If a custom controller factory is in use, make sure that it creates a new instance of the controller for each request.");
        }

        [Fact]
        public void Execute_ThrowsIfRequestContextIsNull()
        {
            // Arrange
            IAsyncController controller = new EmptyController();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { controller.BeginExecute(null, null, null); }, "requestContext");
        }

        [Fact]
        public void ExecuteCore_Asynchronous_ActionFound()
        {
            // Arrange
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                Mock<IAsyncActionInvoker> mockActionInvoker = new Mock<IAsyncActionInvoker>();
                mockActionInvoker.Setup(o => o.BeginInvokeAction(It.IsAny<ControllerContext>(), "SomeAction", It.IsAny<AsyncCallback>(), It.IsAny<object>())).Returns(innerAsyncResult);
                mockActionInvoker.Setup(o => o.EndInvokeAction(innerAsyncResult)).Returns(true);

                RequestContext requestContext = GetRequestContext("SomeAction");
                EmptyController controller = new EmptyController()
                {
                    ActionInvoker = mockActionInvoker.Object
                };

                // Act & assert
                IAsyncResult outerAsyncResult = ((IAsyncController)controller).BeginExecute(requestContext, null, null);
                Assert.False(controller.TempDataSaved);

                ((IAsyncController)controller).EndExecute(outerAsyncResult);
                Assert.True(controller.TempDataSaved);
                Assert.False(controller.HandleUnknownActionCalled);
            }
        }

        [Fact]
        public void ExecuteCore_Asynchronous_ActionNotFound()
        {
            // Arrange
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                Mock<IAsyncActionInvoker> mockActionInvoker = new Mock<IAsyncActionInvoker>();
                mockActionInvoker.Setup(o => o.BeginInvokeAction(It.IsAny<ControllerContext>(), "SomeAction", It.IsAny<AsyncCallback>(), It.IsAny<object>())).Returns(innerAsyncResult);
                mockActionInvoker.Setup(o => o.EndInvokeAction(innerAsyncResult)).Returns(false);

                RequestContext requestContext = GetRequestContext("SomeAction");
                EmptyController controller = new EmptyController()
                {
                    ActionInvoker = mockActionInvoker.Object
                };

                // Act & assert
                IAsyncResult outerAsyncResult = ((IAsyncController)controller).BeginExecute(requestContext, null, null);
                Assert.False(controller.TempDataSaved);

                ((IAsyncController)controller).EndExecute(outerAsyncResult);
                Assert.True(controller.TempDataSaved);
                Assert.True(controller.HandleUnknownActionCalled);
            }
        }

        [Fact]
        public void ExecuteCore_Synchronous_ActionFound()
        {
            // Arrange
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                Mock<IActionInvoker> mockActionInvoker = new Mock<IActionInvoker>();
                mockActionInvoker.Setup(o => o.InvokeAction(It.IsAny<ControllerContext>(), "SomeAction")).Returns(true);

                RequestContext requestContext = GetRequestContext("SomeAction");
                EmptyController controller = new EmptyController()
                {
                    ActionInvoker = mockActionInvoker.Object
                };

                // Act & assert
                IAsyncResult outerAsyncResult = ((IAsyncController)controller).BeginExecute(requestContext, null, null);
                Assert.False(controller.TempDataSaved);

                ((IAsyncController)controller).EndExecute(outerAsyncResult);
                Assert.True(controller.TempDataSaved);
                Assert.False(controller.HandleUnknownActionCalled);
            }
        }

        [Fact]
        public void ExecuteCore_Synchronous_ActionNotFound()
        {
            // Arrange
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                Mock<IActionInvoker> mockActionInvoker = new Mock<IActionInvoker>();
                mockActionInvoker.Setup(o => o.InvokeAction(It.IsAny<ControllerContext>(), "SomeAction")).Returns(false);

                RequestContext requestContext = GetRequestContext("SomeAction");
                EmptyController controller = new EmptyController()
                {
                    ActionInvoker = mockActionInvoker.Object
                };

                // Act & assert
                IAsyncResult outerAsyncResult = ((IAsyncController)controller).BeginExecute(requestContext, null, null);
                Assert.False(controller.TempDataSaved);

                ((IAsyncController)controller).EndExecute(outerAsyncResult);
                Assert.True(controller.TempDataSaved);
                Assert.True(controller.HandleUnknownActionCalled);
            }
        }

        [Fact]
        public void ExecuteCore_SavesTempDataOnException()
        {
            // Arrange
            Mock<IAsyncActionInvoker> mockActionInvoker = new Mock<IAsyncActionInvoker>();
            mockActionInvoker
                .Setup(o => o.BeginInvokeAction(It.IsAny<ControllerContext>(), "SomeAction", It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Throws(new Exception("Some exception text."));

            RequestContext requestContext = GetRequestContext("SomeAction");
            EmptyController controller = new EmptyController()
            {
                ActionInvoker = mockActionInvoker.Object
            };

            // Act & assert
            Assert.Throws<Exception>(
                delegate { ((IAsyncController)controller).BeginExecute(requestContext, null, null); },
                @"Some exception text.");
            Assert.True(controller.TempDataSaved);
        }

        [Fact]
        public void CreateActionInvokerCallsIntoResolverInstance()
        {
            // Controller uses an IDependencyResolver to create an IActionInvoker.
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IAsyncActionInvoker> actionInvokerMock = new Mock<IAsyncActionInvoker>();
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvoker))).Returns(actionInvokerMock.Object);
            controller.Resolver = resolverMock.Object;

            var ai = controller.CreateActionInvoker();

            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvoker)), Times.Once());
            Assert.Same(actionInvokerMock.Object, ai);
        }

        [Fact]
        public void CreateActionInvokerCallsIntoResolverInstanceAndCreatesANewOneIfNecessary()
        {
            // If IDependencyResolver is set, but empty, falls back and still creates.
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvoker))).Returns(null);
            resolverMock.Setup(r => r.GetService(typeof(IActionInvoker))).Returns(null);
            controller.Resolver = resolverMock.Object;

            var ai = controller.CreateActionInvoker();

            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvoker)), Times.Once());
            resolverMock.Verify(r => r.GetService(typeof(IActionInvoker)), Times.Once());
            Assert.NotNull(ai);
        }


        private static RequestContext GetRequestContext(string actionName)
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            RouteData routeData = new RouteData();
            routeData.Values["action"] = actionName;

            return new RequestContext(mockHttpContext.Object, routeData);
        }

        private class EmptyController : AsyncController
        {
            public bool TempDataSaved;
            public bool HandleUnknownActionCalled;

            protected override ITempDataProvider CreateTempDataProvider()
            {
                return new DummyTempDataProvider();
            }

            protected override void HandleUnknownAction(string actionName)
            {
                HandleUnknownActionCalled = true;
            }

            // Test can expose protected method as public.
            public new IActionInvoker CreateActionInvoker()
            {
                return base.CreateActionInvoker();
            }


            private class DummyTempDataProvider : ITempDataProvider
            {
                public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
                {
                    return new TempDataDictionary();
                }

                public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
                {
                    ((EmptyController)controllerContext.Controller).TempDataSaved = true;
                }
            }
        }
    }
}
