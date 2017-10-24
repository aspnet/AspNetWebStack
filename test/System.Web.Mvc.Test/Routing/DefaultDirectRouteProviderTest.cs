// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Routing
{
    public class DefaultDirectRouteProviderTest
    {
        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsNull_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            IDirectRouteFactory factory = CreateStubRouteFactory(null);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDirectRouteProvider.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false),
                "IDirectRouteFactory.CreateRoute must not return null.");
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithoutActionDescriptors_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            Route route = new Route(url: null, routeHandler: null);
            Assert.Null(route.GetTargetActionDescriptors()); // Guard
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => DefaultDirectRouteProvider.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithEmptyActionDescriptors_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            Route route = new Route(url: null, routeHandler: null);
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add(RouteDataTokenKeys.Actions, new ActionDescriptor[0]);
            ActionDescriptor[] originalActions = route.GetTargetActionDescriptors();
            Assert.NotNull(originalActions); // Guard
            Assert.Empty(originalActions); // Guard
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "The route does not have any associated action descriptors. Routing requires " +
                "that each direct route map to a non-empty set of actions.";
            Assert.Throws<InvalidOperationException>(() => DefaultDirectRouteProvider.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        [Fact]
        public void CreateRouteEntry_IfDirectRouteProviderReturnsRouteWithHandler_Throws()
        {
            // Arrange
            string areaPrefix = null;
            string controllerPrefix = null;
            ControllerDescriptor controllerDescriptor = CreateStubControllerDescriptor("IgnoreController");
            ActionDescriptor actionDescriptor = CreateStubActionDescriptor(controllerDescriptor, "IgnoreAction");
            Route route = new Route(url: null, routeHandler: null);
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add(RouteDataTokenKeys.Actions, new ActionDescriptor[] { actionDescriptor });
            route.RouteHandler = new Mock<IRouteHandler>(MockBehavior.Strict).Object;
            ActionDescriptor[] originalActions = route.GetTargetActionDescriptors();
            RouteEntry entry = new RouteEntry(name: null, route: route);
            IDirectRouteFactory factory = CreateStubRouteFactory(entry);
            IReadOnlyCollection<ActionDescriptor> actions = new ActionDescriptor[] { actionDescriptor };
            IInlineConstraintResolver constraintResolver =
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object;

            // Act & Assert
            string expectedMessage = "Direct routing does not support per-route route handlers.";
            Assert.Throws<InvalidOperationException>(() => DefaultDirectRouteProvider.CreateRouteEntry(areaPrefix,
                controllerPrefix, factory, actions, constraintResolver, targetIsAction: false), expectedMessage);
        }

        private static ActionDescriptor CreateStubActionDescriptor(ControllerDescriptor controllerDescriptor, string actionName)
        {
            Mock<ActionDescriptor> mock = new Mock<ActionDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ControllerDescriptor).Returns(controllerDescriptor);
            mock.SetupGet(d => d.ActionName).Returns(actionName);
            return mock.Object;
        }

        private static ControllerDescriptor CreateStubControllerDescriptor(string controllerName)
        {
            Mock<ControllerDescriptor> mock = new Mock<ControllerDescriptor>(MockBehavior.Strict);
            mock.SetupGet(d => d.ControllerName).Returns(controllerName);
            return mock.Object;
        }

        private static IDirectRouteFactory CreateStubRouteFactory(RouteEntry entry)
        {
            Mock<IDirectRouteFactory> mock = new Mock<IDirectRouteFactory>(MockBehavior.Strict);
            mock.Setup(p => p.CreateRoute(It.IsAny<DirectRouteFactoryContext>())).Returns(entry);
            return mock.Object;
        }
    }
}
