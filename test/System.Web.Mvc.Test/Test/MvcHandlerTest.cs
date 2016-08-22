// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Async;
using System.Web.Mvc.Async.Test;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Mvc.Test
{
    public class MvcHandlerTest
    {
        [Fact]
        public void ConstructorWithNullRequestContextThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new MvcHandler(null); },
                "requestContext");
        }

        [Fact]
        public void ProcessRequestWithRouteWithoutControllerThrows()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.ExpectMvcVersionResponseHeader().Verifiable();
            RouteData rd = new RouteData();
            MvcHandler mvcHandler = new MvcHandler(new RequestContext(contextMock.Object, rd));

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { mvcHandler.ProcessRequest(contextMock.Object); },
                "The RouteData must contain an item named 'controller' with a non-empty string value.");

            // Assert
            contextMock.Verify();
        }

        [Fact]
        public void ProcessRequestAddsServerHeaderCallsExecute()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.ExpectMvcVersionResponseHeader().Verifiable();

            RouteData rd = new RouteData();
            rd.Values.Add("controller", "foo");
            RequestContext requestContext = new RequestContext(contextMock.Object, rd);
            MvcHandler mvcHandler = new MvcHandler(requestContext);

            Mock<ControllerBase> controllerMock = new Mock<ControllerBase>();
            controllerMock.As<IController>().CallBase = true;
            controllerMock.Protected().Setup("Execute", requestContext).Verifiable();

            ControllerBuilder cb = new ControllerBuilder();
            Mock<IControllerFactory> controllerFactoryMock = new Mock<IControllerFactory>();
            controllerFactoryMock.Setup(o => o.CreateController(requestContext, "foo")).Returns(controllerMock.Object);
            controllerFactoryMock.Setup(o => o.ReleaseController(controllerMock.Object));
            cb.SetControllerFactory(controllerFactoryMock.Object);
            mvcHandler.ControllerBuilder = cb;

            // Act
            mvcHandler.ProcessRequest(contextMock.Object);

            // Assert
            contextMock.Verify();
            controllerMock.Verify();
        }

        [Fact]
        public void ProcessRequestRemovesOptionalParametersFromRouteValueDictionary()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.ExpectMvcVersionResponseHeader();

            RouteData rd = new RouteData();
            rd.Values.Add("controller", "foo");
            rd.Values.Add("optional", UrlParameter.Optional);
            RequestContext requestContext = new RequestContext(contextMock.Object, rd);
            MvcHandler mvcHandler = new MvcHandler(requestContext);

            Mock<ControllerBase> controllerMock = new Mock<ControllerBase>();
            controllerMock.As<IController>().CallBase = true;
            controllerMock.Protected().Setup("Execute", requestContext).Verifiable();

            ControllerBuilder cb = new ControllerBuilder();
            Mock<IControllerFactory> controllerFactoryMock = new Mock<IControllerFactory>();
            controllerFactoryMock.Setup(o => o.CreateController(requestContext, "foo")).Returns(controllerMock.Object);
            controllerFactoryMock.Setup(o => o.ReleaseController(controllerMock.Object));
            cb.SetControllerFactory(controllerFactoryMock.Object);
            mvcHandler.ControllerBuilder = cb;

            // Act
            mvcHandler.ProcessRequest(contextMock.Object);

            // Assert
            controllerMock.Verify();
            Assert.False(rd.Values.ContainsKey("optional"));
        }

        [Fact]
        public void ProcessRequestWithDisabledServerHeaderOnlyCallsExecute()
        {
            bool oldResponseHeaderValue = MvcHandler.DisableMvcResponseHeader;
            try
            {
                // Arrange
                MvcHandler.DisableMvcResponseHeader = true;
                Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();

                RouteData rd = new RouteData();
                rd.Values.Add("controller", "foo");
                RequestContext requestContext = new RequestContext(contextMock.Object, rd);
                MvcHandler mvcHandler = new MvcHandler(requestContext);

                Mock<ControllerBase> controllerMock = new Mock<ControllerBase>();
                controllerMock.As<IController>().CallBase = true;
                controllerMock.Protected().Setup("Execute", requestContext).Verifiable();

                ControllerBuilder cb = new ControllerBuilder();
                Mock<IControllerFactory> controllerFactoryMock = new Mock<IControllerFactory>();
                controllerFactoryMock.Setup(o => o.CreateController(requestContext, "foo")).Returns(controllerMock.Object);
                controllerFactoryMock.Setup(o => o.ReleaseController(controllerMock.Object));
                cb.SetControllerFactory(controllerFactoryMock.Object);
                mvcHandler.ControllerBuilder = cb;

                // Act
                mvcHandler.ProcessRequest(contextMock.Object);

                // Assert
                controllerMock.Verify();
            }
            finally
            {
                MvcHandler.DisableMvcResponseHeader = oldResponseHeaderValue;
            }
        }

        [Fact]
        public void ProcessRequestDisposesControllerIfExecuteDoesNotThrowException()
        {
            // Arrange
            Mock<ControllerBase> mockController = new Mock<ControllerBase>();
            mockController.As<IDisposable>().Setup(c => c.Dispose()); // so that Verify can be called on Dispose later
            mockController.As<IController>().CallBase = true;
            mockController.Protected().Setup("Execute", ItExpr.IsAny<RequestContext>()).Verifiable();

            ControllerBuilder builder = new ControllerBuilder();
            builder.SetControllerFactory(new SimpleControllerFactory(mockController.Object));

            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.ExpectMvcVersionResponseHeader().Verifiable();
            RequestContext requestContext = new RequestContext(contextMock.Object, new RouteData());
            requestContext.RouteData.Values["controller"] = "fooController";
            MvcHandler handler = new MvcHandler(requestContext)
            {
                ControllerBuilder = builder
            };

            // Act
            handler.ProcessRequest(requestContext.HttpContext);

            // Assert
            mockController.Verify();
            contextMock.Verify();
            mockController.As<IDisposable>().Verify(d => d.Dispose(), Times.AtMostOnce());
        }

        [Fact]
        public void ProcessRequestDisposesControllerIfExecuteThrowsException()
        {
            // Arrange
            Mock<ControllerBase> mockController = new Mock<ControllerBase>();
            mockController.As<IDisposable>().Setup(d => d.Dispose()); // so that Verify can be called on Dispose later
            mockController.As<IController>().CallBase = true;
            mockController.Protected().Setup("Execute", ItExpr.IsAny<RequestContext>()).Throws(new Exception("some exception"));

            ControllerBuilder builder = new ControllerBuilder();
            builder.SetControllerFactory(new SimpleControllerFactory(mockController.Object));

            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.ExpectMvcVersionResponseHeader().Verifiable();
            RequestContext requestContext = new RequestContext(contextMock.Object, new RouteData());
            requestContext.RouteData.Values["controller"] = "fooController";
            MvcHandler handler = new MvcHandler(requestContext)
            {
                ControllerBuilder = builder
            };

            // Act
            Assert.Throws<Exception>(
                delegate { handler.ProcessRequest(requestContext.HttpContext); },
                "some exception");

            // Assert
            mockController.Verify();
            contextMock.Verify();
            mockController.As<IDisposable>().Verify(d => d.Dispose(), Times.AtMostOnce());
        }

        [Fact]
        public void ProcessRequestAsync_AsyncController_DisposesControllerOnException()
        {
            // Arrange
            Mock<IAsyncController> mockController = new Mock<IAsyncController>();
            mockController.Setup(o => o.BeginExecute(It.IsAny<RequestContext>(), It.IsAny<AsyncCallback>(), It.IsAny<object>())).Throws(new Exception("Some exception text."));
            mockController.As<IDisposable>().Setup(o => o.Dispose()).Verifiable();

            MvcHandler handler = GetMvcHandler(mockController.Object);

            // Act & assert
            Assert.Throws<Exception>(
                delegate { handler.BeginProcessRequest(handler.RequestContext.HttpContext, null, null); },
                @"Some exception text.");

            mockController.Verify();
        }

        [Fact]
        public void ProcessRequestAsync_AsyncController_NormalExecution()
        {
            // Arrange
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                bool disposeWasCalled = false;

                Mock<IAsyncController> mockController = new Mock<IAsyncController>();
                mockController.Setup(o => o.BeginExecute(It.IsAny<RequestContext>(), It.IsAny<AsyncCallback>(), It.IsAny<object>())).Returns(innerAsyncResult);
                mockController.As<IDisposable>().Setup(o => o.Dispose()).Callback(delegate { disposeWasCalled = true; });

                MvcHandler handler = GetMvcHandler(mockController.Object);

                // Act & assert
                IAsyncResult outerAsyncResult = handler.BeginProcessRequest(handler.RequestContext.HttpContext, null, null);
                Assert.False(disposeWasCalled);

                handler.EndProcessRequest(outerAsyncResult);
                Assert.True(disposeWasCalled);
                mockController.Verify(o => o.EndExecute(innerAsyncResult), Times.AtMostOnce());
            }
        }

        [Fact]
        public void ProcessRequestAsync_SyncController_NormalExecution()
        {
            // Arrange
            bool executeWasCalled = false;
            bool disposeWasCalled = false;

            Mock<IController> mockController = new Mock<IController>();
            mockController.Setup(o => o.Execute(It.IsAny<RequestContext>())).Callback(delegate { executeWasCalled = true; });
            mockController.As<IDisposable>().Setup(o => o.Dispose()).Callback(delegate { disposeWasCalled = true; });

            MvcHandler handler = GetMvcHandler(mockController.Object);

            // Act & assert
            IAsyncResult outerAsyncResult = handler.BeginProcessRequest(handler.RequestContext.HttpContext, null, null);
            Assert.False(executeWasCalled);
            Assert.False(disposeWasCalled);

            handler.EndProcessRequest(outerAsyncResult);
            Assert.True(executeWasCalled);
            Assert.True(disposeWasCalled);
        }

        // Test that execute is called on a  user controller that derives from Controller
        [Fact]
        public void ProcessRequestAsync_SyncController_NormalExecution2()
        {
            // Arrange
            MyCustomerController controller = new MyCustomerController();

            MvcHandler handler = GetMvcHandler(controller);
            handler.RequestContext.RouteData.Values["action"] = "Widget";

            // Act
            IAsyncResult outerAsyncResult = handler.BeginProcessRequest(handler.RequestContext.HttpContext, null, null);
            handler.EndProcessRequest(outerAsyncResult);

            // Assert
            Assert.Equal(1, controller.Called);
        }

        private class EmptyActionInvoker : IActionInvoker
        {
            public bool InvokeAction(ControllerContext controllerContext, string actionName)
            {
                return true; // action handled.
            }
        }

        // Class that captures how an end-user would override and use a Controller
        private class MyCustomerController : Controller
        {
            public int Called { get; set; }

            protected override void ExecuteCore()
            {
                this.Called++;
            }

            protected override IActionInvoker CreateActionInvoker()
            {
                // The default action invoker relies on too much state (like having an HttpRequestContext), so stub it out.
                return new EmptyActionInvoker();
            }

            // Workaround.
            protected override bool DisableAsyncSupport
            {
                get
                {
                    return true; // ensure ExecuteCore still gets called.
                }
            }
        }

        private static MvcHandler GetMvcHandler(IController controller)
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Response.AddHeader("X-AspNetMvc-Version", "2.0"));

            RouteData routeData = new RouteData();
            routeData.Values["controller"] = "SomeController";
            RequestContext requestContext = new RequestContext(mockHttpContext.Object, routeData);

            ControllerBuilder controllerBuilder = new ControllerBuilder();
            controllerBuilder.SetControllerFactory(new SimpleControllerFactory(controller));

            return new MvcHandler(requestContext)
            {
                ControllerBuilder = controllerBuilder
            };
        }

        private class SimpleControllerFactory : IControllerFactory
        {
            private IController _instance;

            public SimpleControllerFactory(IController instance)
            {
                _instance = instance;
            }

            public IController CreateController(RequestContext context, string controllerName)
            {
                return _instance;
            }

            public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
            {
                return SessionStateBehavior.Default;
            }

            public void ReleaseController(IController controller)
            {
                IDisposable disposable = controller as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
