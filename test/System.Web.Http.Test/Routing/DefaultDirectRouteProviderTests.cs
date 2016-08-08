// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class DefaultDirectRouteProviderTests
    {
        [Fact]
        public void GetActionDirectRoutes_IfDirectRouteProviderReturnsNull_Throws()
        {
            // Arrange
            var factories = new[] { CreateStubRouteFactory(null) };
            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetActionDirectRoutes(action, factories, constraintResolver),
                "IDirectRouteFactory.CreateRoute must not return null.");
        }

        [Fact]
        public void GetControllerDirectRoutes_IfDirectRouteProviderReturnsNull_Throws()
        {
            // Arrange
            var factories = new[] { CreateStubRouteFactory(null) };
            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetControllerDirectRoutes(action.ControllerDescriptor, new[] { action }, factories, constraintResolver),
                "IDirectRouteFactory.CreateRoute must not return null.");
        }

        [Fact]
        public void GetActionDirectRoutes_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            IHttpRoute route = new Mock<IHttpRoute>().Object;
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetActionDirectRoutes(action, factories, constraintResolver),
                expectedMessage);
        }

        [Fact]
        public void GetControllerDirectRoutes_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            IHttpRoute route = new Mock<IHttpRoute>().Object;
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetControllerDirectRoutes(action.ControllerDescriptor, new[] { action }, factories, constraintResolver),
                expectedMessage);
        }

        [Fact]
        public void GetActionDirectRoutes_IfDirectRouteProviderReturnsRouteWithEmptyActionDescriptors_Throws()
        {
            // Arrange
            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[0] }
            };
            HttpRoute route = new HttpRoute(null, null, null, dataTokens);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetActionDirectRoutes(action, factories, constraintResolver),
                expectedMessage);
        }

        [Fact]
        public void GetControllerDirectRoute_IfDirectRouteProviderReturnsRouteWithEmptyActionDescriptors_Throws()
        {
            // Arrange
            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[0] }
            };
            HttpRoute route = new HttpRoute(null, null, null, dataTokens);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var action = CreateStubActionDescriptor("IgnoreAction");
            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetControllerDirectRoutes(action.ControllerDescriptor, new[] { action }, factories, constraintResolver),
                expectedMessage);
        }

        [Fact]
        public void GetActionDirectRoutes_IfDirectRouteProviderReturnsRouteWithHandler_Throws()
        {
            // Arrange
            var action = CreateStubActionDescriptor("IgnoreAction");

            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[] { action } }
            };
            HttpMessageHandler handler = new Mock<HttpMessageHandler>(MockBehavior.Strict).Object;
            HttpRoute route = new HttpRoute(null, null, null, dataTokens, handler);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "Direct routing does not support per-route message handlers.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetActionDirectRoutes(action, factories, constraintResolver), 
                expectedMessage);
        }

        [Fact]
        public void GetControllerDirectRoutes_IfDirectRouteProviderReturnsRouteWithHandler_Throws()
        {
            // Arrange
            var action = CreateStubActionDescriptor("IgnoreAction");

            HttpRouteValueDictionary dataTokens = new HttpRouteValueDictionary
            {
                { RouteDataTokenKeys.Actions, new HttpActionDescriptor[] { action } }
            };
            HttpMessageHandler handler = new Mock<HttpMessageHandler>(MockBehavior.Strict).Object;
            HttpRoute route = new HttpRoute(null, null, null, dataTokens, handler);
            RouteEntry entry = new RouteEntry(name: null, route: route);
            var factories = new[] { CreateStubRouteFactory(entry) };

            var constraintResolver = new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            string expectedMessage = "Direct routing does not support per-route message handlers.";
            Assert.Throws<InvalidOperationException>(
                () => provider.GetControllerDirectRoutes(action.ControllerDescriptor, new[] { action }, factories, constraintResolver),
                expectedMessage);
        }

        [Fact]
        public void GetRoutePrefix_WithMultiRoutePrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new MultiRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetRoutePrefix(httpControllerDescriptor),
                "Only one route prefix attribute is supported. Remove extra attributes from the controller of type 'Namespace.TypeFullName'.");
        }

        [Fact]
        public void GetRoutePrefix_WithNullPrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpControllerDescriptor = new NullRoutePrefixControllerDescripter();
            var typeMock = new Mock<Type>();
            typeMock.SetupGet(t => t.FullName).Returns("Namespace.TypeFullName");
            httpControllerDescriptor.ControllerType = typeMock.Object;

            var provider = new AccessibleDirectRouteProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetRoutePrefix(httpControllerDescriptor),
                "The property 'prefix' from route prefix attribute on controller of type 'Namespace.TypeFullName' cannot be null.");
        }

        private static HttpActionDescriptor CreateStubActionDescriptor(string actionName)
        {
            Mock<HttpActionDescriptor> mock = new Mock<HttpActionDescriptor>(MockBehavior.Strict, new Mock<HttpControllerDescriptor>().Object);
            mock.SetupGet(d => d.ActionName).Returns(actionName);
            return mock.Object;
        }

        private static IDirectRouteFactory CreateStubRouteFactory(RouteEntry entry)
        {
            Mock<IDirectRouteFactory> mock = new Mock<IDirectRouteFactory>(MockBehavior.Strict);
            mock.Setup(p => p.CreateRoute(It.IsAny<DirectRouteFactoryContext>())).Returns(entry);
            return mock.Object;
        }

        private class MultiRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute(), new RoutePrefixAttribute("Prefix") };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class NullRoutePrefixControllerDescripter : HttpControllerDescriptor
        {
            public override Collection<T> GetCustomAttributes<T>(bool inherit)
            {
                object[] attributes = new object[] { new ExtendedRoutePrefixAttribute() };
                return new Collection<T>(TypeHelper.OfType<T>(attributes));
            }
        }

        private class ExtendedRoutePrefixAttribute : RoutePrefixAttribute
        {
        }

        private class AccessibleDirectRouteProvider : DefaultDirectRouteProvider
        {
            public new IReadOnlyCollection<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
            {
                return base.GetActionRouteFactories(actionDescriptor);
            }

            public new IReadOnlyList<RouteEntry> GetControllerDirectRoutes(
                HttpControllerDescriptor controllerDescriptor,
                IReadOnlyList<HttpActionDescriptor> actionDescriptors,
                IReadOnlyList<IDirectRouteFactory> factories,
                IInlineConstraintResolver constraintResolver)
            {
                return base.GetControllerDirectRoutes(controllerDescriptor, actionDescriptors, factories, constraintResolver);
            }

            public new IReadOnlyList<RouteEntry> GetActionDirectRoutes(
                HttpActionDescriptor actionDescriptor,
                IReadOnlyList<IDirectRouteFactory> factories,
                IInlineConstraintResolver constraintResolver)
            {
                return base.GetActionDirectRoutes(actionDescriptor, factories, constraintResolver);
            }

            public new IReadOnlyCollection<IDirectRouteFactory> GetControllerRouteFactories(HttpControllerDescriptor controllerDescriptor)
            {
                return base.GetControllerRouteFactories(controllerDescriptor);
            }

            public new string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
            {
                return base.GetRoutePrefix(controllerDescriptor);
            }
        }
    }
}
