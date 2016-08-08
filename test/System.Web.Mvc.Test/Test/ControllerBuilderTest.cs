// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ControllerBuilderTest
    {
        [Fact]
        public void ControllerBuilderReturnsDefaultControllerBuilderByDefault()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();

            // Act
            IControllerFactory cf = cb.GetControllerFactory();

            // Assert
            Assert.IsType<DefaultControllerFactory>(cf);
        }

        [Fact]
        public void CreateControllerWithFactoryThatCannotBeCreatedThrows()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();
            cb.SetControllerFactory(typeof(ControllerFactoryThrowsFromConstructor));

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    RequestContext reqContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
                    reqContext.RouteData.Values["controller"] = "foo";
                    MvcHandlerWithNoVersionHeader handler = new MvcHandlerWithNoVersionHeader(reqContext)
                    {
                        ControllerBuilder = cb
                    };
                    handler.ProcessRequest(reqContext.HttpContext);
                },
                "An error occurred when trying to create the IControllerFactory 'System.Web.Mvc.Test.ControllerBuilderTest+ControllerFactoryThrowsFromConstructor'. Make sure that the controller factory has a public parameterless constructor.");
        }

        [Fact]
        public void CreateControllerWithFactoryThatReturnsNullThrows()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();
            cb.SetControllerFactory(typeof(ControllerFactoryReturnsNull));

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    RequestContext reqContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
                    reqContext.RouteData.Values["controller"] = "boo";
                    MvcHandlerWithNoVersionHeader handler = new MvcHandlerWithNoVersionHeader(reqContext)
                    {
                        ControllerBuilder = cb
                    };
                    handler.ProcessRequest(reqContext.HttpContext);
                },
                "The IControllerFactory 'System.Web.Mvc.Test.ControllerBuilderTest+ControllerFactoryReturnsNull' did not return a controller for the name 'boo'.");
        }

        [Fact]
        public void CreateControllerWithFactoryThatThrowsDoesNothingSpecial()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();
            cb.SetControllerFactory(typeof(ControllerFactoryThrows));

            // Act
            Assert.Throws<Exception>(
                delegate
                {
                    RequestContext reqContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
                    reqContext.RouteData.Values["controller"] = "foo";
                    MvcHandlerWithNoVersionHeader handler = new MvcHandlerWithNoVersionHeader(reqContext)
                    {
                        ControllerBuilder = cb
                    };
                    handler.ProcessRequest(reqContext.HttpContext);
                },
                "ControllerFactoryThrows");
        }

        [Fact]
        public void CreateControllerWithFactoryInstanceReturnsInstance()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();
            DefaultControllerFactory factory = new DefaultControllerFactory();
            cb.SetControllerFactory(factory);

            // Act
            IControllerFactory cf = cb.GetControllerFactory();

            // Assert
            Assert.Same(factory, cf);
        }

        [Fact]
        public void CreateControllerWithFactoryTypeReturnsValidType()
        {
            // Arrange
            ControllerBuilder cb = new ControllerBuilder();
            cb.SetControllerFactory(typeof(MockControllerFactory));

            // Act
            IControllerFactory cf = cb.GetControllerFactory();

            // Assert
            Assert.IsType<MockControllerFactory>(cf);
        }

        [Fact]
        public void SetControllerFactoryInstanceWithNullThrows()
        {
            ControllerBuilder cb = new ControllerBuilder();
            Assert.ThrowsArgumentNull(
                delegate { cb.SetControllerFactory((IControllerFactory)null); },
                "controllerFactory");
        }

        [Fact]
        public void SetControllerFactoryTypeWithNullThrows()
        {
            ControllerBuilder cb = new ControllerBuilder();
            Assert.ThrowsArgumentNull(
                delegate { cb.SetControllerFactory((Type)null); },
                "controllerFactoryType");
        }

        [Fact]
        public void SetControllerFactoryTypeWithNonFactoryTypeThrows()
        {
            ControllerBuilder cb = new ControllerBuilder();
            Assert.Throws<ArgumentException>(
                delegate { cb.SetControllerFactory(typeof(int)); },
                "The controller factory type 'System.Int32' must implement the IControllerFactory interface.\r\nParameter name: controllerFactoryType");
        }

        [Fact]
        public void DefaultControllerFactoryIsDefaultControllerFactory()
        {
            // Arrange
            ControllerBuilder builder = new ControllerBuilder();

            // Act
            IControllerFactory returnedControllerFactory = builder.GetControllerFactory();

            //Assert
            Assert.Equal(typeof(DefaultControllerFactory), returnedControllerFactory.GetType());
        }

        [Fact]
        public void SettingControllerFactoryReturnsSetFactory()
        {
            // Arrange
            ControllerBuilder builder = new ControllerBuilder();
            Mock<IControllerFactory> setFactory = new Mock<IControllerFactory>();

            // Act
            builder.SetControllerFactory(setFactory.Object);

            // Assert
            Assert.Same(setFactory.Object, builder.GetControllerFactory());
        }

        [Fact]
        public void ControllerBuilderGetControllerFactoryDelegatesToResolver()
        {
            //Arrange
            Mock<IControllerFactory> factory = new Mock<IControllerFactory>();
            Resolver<IControllerFactory> resolver = new Resolver<IControllerFactory> { Current = factory.Object };
            ControllerBuilder builder = new ControllerBuilder(resolver);

            //Act
            IControllerFactory result = builder.GetControllerFactory();

            //Assert
            Assert.Same(factory.Object, result);
        }

        public class ControllerFactoryThrowsFromConstructor : IControllerFactory
        {
            public ControllerFactoryThrowsFromConstructor()
            {
                throw new Exception("ControllerFactoryThrowsFromConstructor");
            }

            public IController CreateController(RequestContext context, string controllerName)
            {
                return null;
            }

            public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
            {
                return SessionStateBehavior.Default;
            }

            public void ReleaseController(IController controller)
            {
            }
        }

        public class ControllerFactoryReturnsNull : IControllerFactory
        {
            public IController CreateController(RequestContext context, string controllerName)
            {
                return null;
            }

            public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
            {
                return SessionStateBehavior.Default;
            }

            public void ReleaseController(IController controller)
            {
            }
        }

        public class ControllerFactoryThrows : IControllerFactory
        {
            public IController CreateController(RequestContext context, string controllerName)
            {
                throw new Exception("ControllerFactoryThrows");
            }

            public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
            {
                return SessionStateBehavior.Default;
            }

            public void ReleaseController(IController controller)
            {
            }
        }

        public class MockControllerFactory : IControllerFactory
        {
            public IController CreateController(RequestContext context, string controllerName)
            {
                throw new NotImplementedException();
            }

            public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
            {
                return SessionStateBehavior.Default;
            }

            public void ReleaseController(IController controller)
            {
            }
        }

        private sealed class MvcHandlerWithNoVersionHeader : MvcHandler
        {
            public MvcHandlerWithNoVersionHeader(RequestContext requestContext)
                : base(requestContext)
            {
            }

            protected internal override void AddVersionHeader(HttpContextBase httpContext)
            {
                // Don't try to set the version header for the unit tests
            }
        }
    }
}
