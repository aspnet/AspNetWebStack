// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Web.Routing.Test;
using System.Web.SessionState;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class DefaultControllerFactoryTest
    {
        static DefaultControllerFactoryTest()
        {
            MvcTestHelper.CreateMvcAssemblies();
        }

        [Fact]
        public void CreateAmbiguousControllerException_RouteWithoutUrl()
        {
            // Arrange
            RouteBase route = new Mock<RouteBase>().Object;

            Type[] matchingTypes = new Type[]
            {
                typeof(object),
                typeof(string)
            };

            // Act
            InvalidOperationException exception = DefaultControllerFactory.CreateAmbiguousControllerException(route, "Foo", matchingTypes);

            // Assert
            Assert.Equal(
                "Multiple types were found that match the controller named 'Foo'. This can happen if the route that services this request does not specify namespaces to search for a controller that matches the request. If this is the case, register this route by calling an overload of the 'MapRoute' method that takes a 'namespaces' parameter." + Environment.NewLine
              + Environment.NewLine
              + "The request for 'Foo' has found the following matching controllers:" + Environment.NewLine
              + "System.Object" + Environment.NewLine
              + "System.String",
                exception.Message);
        }

        [Fact]
        public void CreateAmbiguousControllerException_RouteWithUrl()
        {
            // Arrange
            RouteBase route = new Route("{controller}/blah", new Mock<IRouteHandler>().Object);

            Type[] matchingTypes = new Type[]
            {
                typeof(object),
                typeof(string)
            };

            // Act
            InvalidOperationException exception = DefaultControllerFactory.CreateAmbiguousControllerException(route, "Foo", matchingTypes);

            // Assert
            Assert.Equal(
                "Multiple types were found that match the controller named 'Foo'. This can happen if the route that services this request ('{controller}/blah') does not specify namespaces to search for a controller that matches the request. If this is the case, register this route by calling an overload of the 'MapRoute' method that takes a 'namespaces' parameter." + Environment.NewLine
              + Environment.NewLine
              + "The request for 'Foo' has found the following matching controllers:" + Environment.NewLine
              + "System.Object" + Environment.NewLine
              + "System.String",
                exception.Message);
        }

        [Fact]
        public void CreateControllerWithNullContextThrows()
        {
            // Arrange
            DefaultControllerFactory factory = new DefaultControllerFactory();

            // Act
            Assert.ThrowsArgumentNull(
                delegate
                {
                    ((IControllerFactory)factory).CreateController(
                        null,
                        "foo");
                },
                "requestContext");
        }

        [Fact]
        public void CreateControllerWithEmptyControllerNameThrows()
        {
            // Arrange
            DefaultControllerFactory factory = new DefaultControllerFactory();

            // Act
            Assert.Throws<ArgumentException>(
                delegate
                {
                    ((IControllerFactory)factory).CreateController(
                        new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()),
                        String.Empty);
                },
                "Value cannot be null or empty.\r\nParameter name: controllerName");
        }

        [Fact]
        public void CreateControllerReturnsControllerInstance()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            Mock<DefaultControllerFactory> factoryMock = new Mock<DefaultControllerFactory>();
            factoryMock.CallBase = true;
            factoryMock.Setup(o => o.GetControllerType(requestContext, "moo")).Returns(typeof(DummyController));

            // Act
            IController controller = ((IControllerFactory)factoryMock.Object).CreateController(requestContext, "moo");

            // Assert
            Assert.IsType<DummyController>(controller);
        }

        [Fact]
        public void CreateControllerCanReturnNull()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            Mock<DefaultControllerFactory> factoryMock = new Mock<DefaultControllerFactory>();
            factoryMock.Setup(o => o.GetControllerType(requestContext, "moo")).Returns(typeof(DummyController));
            factoryMock.Setup(o => o.GetControllerInstance(requestContext, typeof(DummyController))).Returns((ControllerBase)null);

            // Act
            IController controller = ((IControllerFactory)factoryMock.Object).CreateController(requestContext, "moo");

            // Assert
            Assert.Null(controller);
        }

        [Fact]
        public void DisposeControllerFactoryWithDisposableController()
        {
            // Arrange
            IControllerFactory factory = new DefaultControllerFactory();
            Mock<ControllerBase> mockController = new Mock<ControllerBase>();
            Mock<IDisposable> mockDisposable = mockController.As<IDisposable>();
            mockDisposable.Setup(d => d.Dispose()).Verifiable();

            // Act
            factory.ReleaseController(mockController.Object);

            // Assert
            mockDisposable.Verify();
        }

        [Fact]
        public void GetControllerInstanceThrowsIfControllerTypeIsNull()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            contextMock.Setup(o => o.Request).Returns(requestMock.Object);
            requestMock.Setup(o => o.Path).Returns("somepath");
            RequestContext requestContext = new RequestContext(contextMock.Object, new RouteData());
            Mock<DefaultControllerFactory> factoryMock = new Mock<DefaultControllerFactory> { CallBase = true };
            factoryMock.Setup(o => o.GetControllerType(requestContext, "moo")).Returns((Type)null);

            // Act
            Assert.ThrowsHttpException(
                delegate { ((IControllerFactory)factoryMock.Object).CreateController(requestContext, "moo"); },
                "The controller for path 'somepath' was not found or does not implement IController.",
                404);
        }

        [Fact]
        public void GetControllerInstanceThrowsIfControllerTypeIsNotControllerBase()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = new DefaultControllerFactory();

            // Act
            Assert.Throws<ArgumentException>(
                delegate { factory.GetControllerInstance(requestContext, typeof(int)); },
                "The controller type 'System.Int32' must implement IController.\r\nParameter name: controllerType");
        }

        [Fact]
        public void GetControllerInstanceWithBadConstructorThrows()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            RequestContext requestContext = new RequestContext(contextMock.Object, new RouteData());
            Mock<DefaultControllerFactory> factoryMock = new Mock<DefaultControllerFactory>();
            factoryMock.CallBase = true;
            factoryMock.Setup(o => o.GetControllerType(requestContext, "moo")).Returns(typeof(DummyControllerThrows));

            // Act
            Exception ex = Assert.Throws<InvalidOperationException>(
                delegate { ((IControllerFactory)factoryMock.Object).CreateController(requestContext, "moo"); },
                "An error occurred when trying to create a controller of type 'System.Web.Mvc.Test.DefaultControllerFactoryTest+DummyControllerThrows'. Make sure that the controller has a parameterless public constructor.");

            Assert.Equal("constructor", ex.InnerException.InnerException.Message);
        }

        [Fact]
        public void GetControllerSessionBehaviorGuardClauses()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            IControllerFactory factory = new DefaultControllerFactory();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => factory.GetControllerSessionBehavior(null, "controllerName"),
                "requestContext"
                );
            Assert.ThrowsArgumentNullOrEmpty(
                () => factory.GetControllerSessionBehavior(requestContext, null),
                "controllerName"
                );
            Assert.ThrowsArgumentNullOrEmpty(
                () => factory.GetControllerSessionBehavior(requestContext, ""),
                "controllerName"
                );
        }

        [Fact]
        public void GetControllerSessionBehaviorReturnsDefaultForNullControllerType()
        {
            // Arrange
            var factory = new DefaultControllerFactory();

            // Act
            SessionStateBehavior result = factory.GetControllerSessionBehavior(null, null);

            // Assert
            Assert.Equal(SessionStateBehavior.Default, result);
        }

        [Fact]
        public void GetControllerSessionBehaviorReturnsDefaultForControllerWithoutAttribute()
        {
            // Arrange
            var factory = new DefaultControllerFactory();

            // Act
            SessionStateBehavior result = factory.GetControllerSessionBehavior(null, typeof(object));

            // Assert
            Assert.Equal(SessionStateBehavior.Default, result);
        }

        [Fact]
        public void GetControllerSessionBehaviorReturnsAttributeValueFromController()
        {
            // Arrange
            var factory = new DefaultControllerFactory();

            // Act
            SessionStateBehavior result = factory.GetControllerSessionBehavior(null, typeof(MyReadOnlyController));

            // Assert
            Assert.Equal(SessionStateBehavior.ReadOnly, result);
        }

        [SessionState(SessionStateBehavior.ReadOnly)]
        class MyReadOnlyController
        {
        }

        [Fact]
        public void GetControllerTypeWithEmptyControllerNameThrows()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = new DefaultControllerFactory();

            // Act
            Assert.Throws<ArgumentException>(
                delegate { factory.GetControllerType(requestContext, String.Empty); },
                "Value cannot be null or empty.\r\nParameter name: controllerName");
        }

        [Fact]
        public void GetControllerTypeForNoAssemblies()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = new DefaultControllerFactory();
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type controllerType = factory.GetControllerType(requestContext, "sometype");

            // Assert
            Assert.Null(controllerType);
            Assert.Equal(0, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeForOneAssembly()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");
            Type c2Type = factory.GetControllerType(requestContext, "c2");

            // Assert
            Assembly asm1 = Assembly.Load("MvcAssembly1");
            Type verifiedC1 = asm1.GetType("NS1a.NS1b.C1Controller");
            Type verifiedC2 = asm1.GetType("NS2a.NS2b.C2Controller");
            Assert.Equal(verifiedC1, c1Type);
            Assert.Equal(verifiedC2, c2Type);
            Assert.Equal(2, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeForManyAssemblies()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b", "ns3a.ns3b", "ns4a.ns4b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly2") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");
            Type c2Type = factory.GetControllerType(requestContext, "C2");
            Type c3Type = factory.GetControllerType(requestContext, "c3"); // lower case
            Type c4Type = factory.GetControllerType(requestContext, "c4"); // lower case

            // Assert
            Assembly asm1 = Assembly.Load("MvcAssembly1");
            Type verifiedC1 = asm1.GetType("NS1a.NS1b.C1Controller");
            Type verifiedC2 = asm1.GetType("NS2a.NS2b.C2Controller");
            Assembly asm2 = Assembly.Load("MvcAssembly2");
            Type verifiedC3 = asm2.GetType("NS3a.NS3b.C3Controller");
            Type verifiedC4 = asm2.GetType("NS4a.NS4b.C4Controller");
            Assert.NotNull(verifiedC1);
            Assert.NotNull(verifiedC2);
            Assert.NotNull(verifiedC3);
            Assert.NotNull(verifiedC4);
            Assert.Equal(verifiedC1, c1Type);
            Assert.Equal(verifiedC2, c2Type);
            Assert.Equal(verifiedC3, c3Type);
            Assert.Equal(verifiedC4, c4Type);
            Assert.Equal(4, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeDoesNotThrowIfSameControllerMatchedMultipleNamespaces()
        {
            // both namespaces "ns3a" and "ns3a.ns3b" will match a controller type, but it's actually
            // the same type. in this case, we shouldn't throw.

            // Arrange
            RequestContext requestContext = GetRequestContextWithNamespaces("ns3a", "ns3a.ns3b");
            requestContext.RouteData.DataTokens["UseNamespaceFallback"] = false;
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly3") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");

            // Assert
            Assembly asm3 = Assembly.Load("MvcAssembly3");
            Type verifiedC1 = asm3.GetType("NS3a.NS3b.C1Controller");
            Assert.NotNull(verifiedC1);
            Assert.Equal(verifiedC1, c1Type);
        }

        [Fact]
        public void GetControllerTypeForAssembliesWithSameTypeNamesInDifferentNamespaces()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly3") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");
            Type c2Type = factory.GetControllerType(requestContext, "C2");

            // Assert
            Assembly asm1 = Assembly.Load("MvcAssembly1");
            Type verifiedC1 = asm1.GetType("NS1a.NS1b.C1Controller");
            Type verifiedC2 = asm1.GetType("NS2a.NS2b.C2Controller");
            Assert.NotNull(verifiedC1);
            Assert.NotNull(verifiedC2);
            Assert.Equal(verifiedC1, c1Type);
            Assert.Equal(verifiedC2, c2Type);
            Assert.Equal(4, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeForAssembliesWithSameTypeNamesInDifferentNamespacesThrowsIfAmbiguous()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns3a.ns3b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly3") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { factory.GetControllerType(requestContext, "C1"); },
                "Multiple types were found that match the controller named 'C1'. This can happen if the route that services this request does not specify namespaces to search for a controller that matches the request. If this is the case, register this route by calling an overload of the 'MapRoute' method that takes a 'namespaces' parameter." + Environment.NewLine
              + Environment.NewLine
              + "The request for 'C1' has found the following matching controllers:" + Environment.NewLine
              + "NS1a.NS1b.C1Controller" + Environment.NewLine
              + "NS3a.NS3b.C1Controller");

            // Assert
            Assert.Equal(4, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeForAssembliesWithSameTypeNamesInSameNamespaceThrows()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly4") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { factory.GetControllerType(requestContext, "C1"); },
                "Multiple types were found that match the controller named 'C1'. This can happen if the route that services this request does not specify namespaces to search for a controller that matches the request. If this is the case, register this route by calling an overload of the 'MapRoute' method that takes a 'namespaces' parameter." + Environment.NewLine
              + Environment.NewLine
              + "The request for 'C1' has found the following matching controllers:" + Environment.NewLine
              + "NS1a.NS1b.C1Controller" + Environment.NewLine
              + "NS1a.NS1b.C1Controller");

            // Assert
            Assert.Equal(4, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeSearchesAllNamespacesAsLastResort()
        {
            // Arrange
            RequestContext requestContext = GetRequestContextWithNamespaces("ns3a.ns3b");
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c2Type = factory.GetControllerType(requestContext, "C2");

            // Assert
            Assembly asm1 = Assembly.Load("MvcAssembly1");
            Type verifiedC2 = asm1.GetType("NS2a.NS2b.C2Controller");
            Assert.NotNull(verifiedC2);
            Assert.Equal(verifiedC2, c2Type);
            Assert.Equal(2, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeSearchesOnlyRouteDefinedNamespacesIfRequested()
        {
            // Arrange
            RequestContext requestContext = GetRequestContextWithNamespaces("ns3a.ns3b");
            requestContext.RouteData.DataTokens["UseNamespaceFallback"] = false;
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly3") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");
            Type c2Type = factory.GetControllerType(requestContext, "C2");

            // Assert
            Assembly asm3 = Assembly.Load("MvcAssembly3");
            Type verifiedC1 = asm3.GetType("NS3a.NS3b.C1Controller");
            Assert.NotNull(verifiedC1);
            Assert.Equal(verifiedC1, c1Type);
            Assert.Null(c2Type);
        }

        [Fact]
        public void GetControllerTypeSearchesRouteDefinedNamespacesBeforeApplicationDefinedNamespaces()
        {
            // Arrange
            RequestContext requestContext = GetRequestContextWithNamespaces("ns3a.ns3b");
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly3") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type c1Type = factory.GetControllerType(requestContext, "C1");
            Type c2Type = factory.GetControllerType(requestContext, "C2");

            // Assert
            Assembly asm1 = Assembly.Load("MvcAssembly1");
            Type verifiedC2 = asm1.GetType("NS2a.NS2b.C2Controller");
            Assembly asm3 = Assembly.Load("MvcAssembly3");
            Type verifiedC1 = asm3.GetType("NS3a.NS3b.C1Controller");
            Assert.NotNull(verifiedC1);
            Assert.NotNull(verifiedC2);
            Assert.Equal(verifiedC1, c1Type);
            Assert.Equal(verifiedC2, c2Type);
            Assert.Equal(4, controllerTypeCache.Count);
        }

        [Fact]
        public void GetControllerTypeThatDoesntExist()
        {
            // Arrange
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            DefaultControllerFactory factory = GetDefaultControllerFactory("ns1a.ns1b", "ns2a.ns2b", "ns3a.ns3b", "ns4a.ns4b");
            MockBuildManager buildManagerMock = new MockBuildManager(new Assembly[] { Assembly.Load("MvcAssembly1"), Assembly.Load("MvcAssembly2"), Assembly.Load("MvcAssembly3"), Assembly.Load("MvcAssembly4") });
            ControllerTypeCache controllerTypeCache = new ControllerTypeCache();

            factory.BuildManager = buildManagerMock;
            factory.ControllerTypeCache = controllerTypeCache;

            // Act
            Type randomType1 = factory.GetControllerType(requestContext, "Cx");
            Type randomType2 = factory.GetControllerType(requestContext, "Cy");
            Type randomType3 = factory.GetControllerType(requestContext, "Foo.Bar");
            Type randomType4 = factory.GetControllerType(requestContext, "C1Controller");

            // Assert
            Assert.Null(randomType1);
            Assert.Null(randomType2);
            Assert.Null(randomType3);
            Assert.Null(randomType4);
            Assert.Equal(8, controllerTypeCache.Count);
        }

        [Fact]
        public void IsControllerType()
        {
            // Act
            bool isController1 = ControllerTypeCache.IsControllerType(null);
            bool isController2 = ControllerTypeCache.IsControllerType(typeof(NonPublicController));
            bool isController3 = ControllerTypeCache.IsControllerType(typeof(MisspelledKontroller));
            bool isController4 = ControllerTypeCache.IsControllerType(typeof(AbstractController));
            bool isController5 = ControllerTypeCache.IsControllerType(typeof(NonIControllerController));
            bool isController6 = ControllerTypeCache.IsControllerType(typeof(Goodcontroller));

            // Assert
            Assert.False(isController1);
            Assert.False(isController2);
            Assert.False(isController3);
            Assert.False(isController4);
            Assert.False(isController5);
            Assert.True(isController6);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", true)]
        [InlineData("Dummy", false)]
        [InlineData("Dummy.*", true)]
        [InlineData("Dummy.Controller.*", false)]
        [InlineData("Dummy.Controllers", true)]
        [InlineData("Dummy.Controllers.*", true)]
        [InlineData("Dummy.Controllers*", false)]
        public void IsNamespaceMatch(string testNamespace, bool expectedResult)
        {
            // Act & Assert
            Assert.Equal(expectedResult, ControllerTypeCache.IsNamespaceMatch(testNamespace, "Dummy.Controllers"));
        }

        [Fact]
        public void GetControllerInstanceConsultsSetControllerActivator()
        {
            //Arrange
            Mock<IControllerActivator> activator = new Mock<IControllerActivator>();
            DefaultControllerFactory factory = new DefaultControllerFactory(activator.Object);
            RequestContext context = new RequestContext();

            //Act
            factory.GetControllerInstance(context, typeof(Goodcontroller));

            //Assert
            activator.Verify(l => l.Create(context, typeof(Goodcontroller)));
        }

        [Fact]
        public void GetControllerDelegatesToActivatorResolver()
        {
            //Arrange
            var context = new RequestContext();
            var expectedController = new Goodcontroller();
            var resolverActivator = new Mock<IControllerActivator>();
            resolverActivator.Setup(a => a.Create(context, typeof(Goodcontroller))).Returns(expectedController);
            var activatorResolver = new Resolver<IControllerActivator> { Current = resolverActivator.Object };
            var factory = new DefaultControllerFactory(null, activatorResolver, null);

            //Act
            IController returnedController = factory.GetControllerInstance(context, typeof(Goodcontroller));

            //Assert
            Assert.Same(returnedController, expectedController);
        }

        [Fact]
        public void GetControllerDelegatesToDependencyResolveWhenActivatorResolverIsNull()
        {
            // Arrange
            var context = new RequestContext();
            var expectedController = new Goodcontroller();
            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            dependencyResolver.Setup(dr => dr.GetService(typeof(Goodcontroller))).Returns(expectedController);
            var factory = new DefaultControllerFactory(null, null, dependencyResolver.Object);

            // Act
            IController returnedController = factory.GetControllerInstance(context, typeof(Goodcontroller));

            // Assert
            Assert.Same(returnedController, expectedController);
        }

        [Fact]
        public void GetControllerDelegatesToActivatorCreateInstanceWhenDependencyResolverReturnsNull()
        {
            // Arrange
            var context = new RequestContext();
            var dependencyResolver = new Mock<IDependencyResolver>();
            var factory = new DefaultControllerFactory(null, null, dependencyResolver.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => factory.GetControllerInstance(context, typeof(NoParameterlessCtor)),
                "An error occurred when trying to create a controller of type 'System.Web.Mvc.Test.DefaultControllerFactoryTest+NoParameterlessCtor'. Make sure that the controller has a parameterless public constructor."
                );
        }

        [Fact]
        public void ActivatorResolverAndDependencyResolverAreNeverCalledWhenControllerActivatorIsPassedInConstructor()
        {
            // Arrange
            var context = new RequestContext();
            var expectedController = new Goodcontroller();

            Mock<IControllerActivator> activator = new Mock<IControllerActivator>();
            activator.Setup(a => a.Create(context, typeof(Goodcontroller))).Returns(expectedController);

            var resolverActivator = new Mock<IControllerActivator>(MockBehavior.Strict);
            var activatorResolver = new Resolver<IControllerActivator> { Current = resolverActivator.Object };

            var dependencyResolver = new Mock<IDependencyResolver>(MockBehavior.Strict);

            //Act
            var factory = new DefaultControllerFactory(activator.Object, activatorResolver, dependencyResolver.Object);
            IController returnedController = factory.GetControllerInstance(context, typeof(Goodcontroller));

            //Assert
            Assert.Same(returnedController, expectedController);
        }

        [Fact]
        public void GetControllerType_WithMatchedDirectRoute_UseTargetMethodType()
        {
            // Arrange
            var requestContext = new RequestContext();
            requestContext.RouteData = new RouteData();
            requestContext.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromMethod<WithRoutingAttributeController>(c => c.Action());
            requestContext.RouteData.AddDirectRouteMatches();

            var expectedControllerType = typeof(WithRoutingAttributeController);

            var controllerActivator = new Mock<IControllerActivator>(MockBehavior.Strict).Object;
            var activatorResolver = new Resolver<IControllerActivator>();

            var factory = new DefaultControllerFactory(controllerActivator, activatorResolver, null);

            // Act
            // if it would not get the controller type from the DirectRoute, then it would not 
            // be able to find any controller
            var type = factory.GetControllerType(requestContext, controllerName: null);

            // Assert
            Assert.Equal(expectedControllerType, type);
        }

        [Fact]
        public void GetControllerType_WithMatchedDirectRoute_UseControllerDescriptorType()
        {
            // Arrange
            var requestContext = new RequestContext();
            requestContext.RouteData = new RouteData();
            requestContext.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromController<AttributeRouteAtControllerLevelController>();
            requestContext.RouteData.AddDirectRouteMatches();

            var expectedControllerType = typeof(AttributeRouteAtControllerLevelController);

            var controllerActivator = new Mock<IControllerActivator>(MockBehavior.Strict).Object;
            var activatorResolver = new Resolver<IControllerActivator>();

            var factory = new DefaultControllerFactory(controllerActivator, activatorResolver, null);

            // Act
            // if it would not get the controller type from the DirectRoute, then it would not 
            // be able to find any controller
            var type = factory.GetControllerType(requestContext, controllerName: null);

            // Assert
            Assert.Equal(expectedControllerType, type);
        }

        [Fact]
        public void GetControllerType_WithMultipleDirectRouteControllers_ThrowsInvalidOperationException()
        {
            // Arrange
            var requestContext = new RequestContext();
            requestContext.RouteData = new RouteData();
            SubRouteCollection subRoutes = new SubRouteCollection();
            DirectRouteTestHelpers.AddDirectRouteFromController<AttributeRouteAtControllerLevelController>(subRoutes);
            DirectRouteTestHelpers.AddDirectRouteFromMethod<WithRoutingAttributeController>(subRoutes, c => c.Action());
            requestContext.RouteData.Route = new RouteCollectionRoute(subRoutes);
            requestContext.RouteData.AddDirectRouteMatches();

            var controllerActivator = new Mock<IControllerActivator>(MockBehavior.Strict).Object;
            var activatorResolver = new Resolver<IControllerActivator>();

            var factory = new DefaultControllerFactory(controllerActivator, activatorResolver, null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => factory.GetControllerType(requestContext, controllerName: null));
        }

        class NoParameterlessCtor : IController
        {
            public NoParameterlessCtor(int x)
            {
            }

            public void Execute(RequestContext requestContext)
            {
            }
        }

        private static DefaultControllerFactory GetDefaultControllerFactory(params string[] namespaces)
        {
            ControllerBuilder builder = new ControllerBuilder();
            builder.DefaultNamespaces.UnionWith(namespaces);
            return new DefaultControllerFactory() { ControllerBuilder = builder };
        }

        private static RequestContext GetRequestContextWithNamespaces(params string[] namespaces)
        {
            RouteData routeData = new RouteData();
            routeData.DataTokens["namespaces"] = namespaces;
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            RequestContext requestContext = new RequestContext(mockHttpContext.Object, routeData);
            return requestContext;
        }

        private sealed class DummyController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class DummyControllerThrows : IController
        {
            public DummyControllerThrows()
            {
                throw new Exception("constructor");
            }

            #region IController Members

            void IController.Execute(RequestContext requestContext)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public interface IDisposableController : IController, IDisposable
        {
        }
    }

    // BAD: type isn't public
    internal class NonPublicController : Controller
    {
    }

    // BAD: type doesn't end with 'Controller'
    public class MisspelledKontroller : Controller
    {
    }

    // BAD: type is abstract
    public abstract class AbstractController : Controller
    {
    }

    // BAD: type doesn't implement IController
    public class NonIControllerController
    {
    }

    // GOOD: 'Controller' suffix should be case-insensitive
    public class Goodcontroller : Controller
    {
    }

    public class WithRoutingAttributeController : Controller
    {
        [Route("route")]
        public void Action()
        {
        }
    }

    [Route("cool/{action}")]
    public class AttributeRouteAtControllerLevelController : Controller
    {
        public void Action()
        {
        }
    }
}
