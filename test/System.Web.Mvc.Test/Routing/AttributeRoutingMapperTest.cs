// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Routing
{
    public class AttributeRoutingMapperTest
    {
        [Fact]
        public void MapMvcAttributeRoutes_DoesNotTryToInferRouteNames()
        {
            var controllerType = typeof(MyController);

            var routeEntries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            var routeEntry = Assert.Single(routeEntries);
            Assert.Null(routeEntry.Name);
        }

        [Fact]
        public void MapMvcAttributeRoutes_RespectsActionNameAttribute()
        {
            // Arrange
            var controllerType = typeof(MyController);

            // Act
            var routeEntries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            // Assert
            var routeEntry = Assert.Single(routeEntries);
            Assert.Equal("ActionName", routeEntry.Route.Defaults["action"]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute()
        {
            // Arrange
            var controllerType = typeof(AnotherController);

            // Act
            var entries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            // Assert
            var controllerEntry = Assert.Single(entries, r => !r.Route.Defaults.ContainsKey("action"));
            Assert.Same(controllerType, controllerEntry.Route.GetTargetControllerDescriptor().ControllerType);

            var actionMethods = controllerEntry.Route.GetTargetActionDescriptors().ToArray();
            Assert.Equal(2, actionMethods.Length);
            Assert.Single(actionMethods, a => a.ActionName == "RegularAction");
            Assert.Single(actionMethods, a => a.ActionName == "AnotherAction");
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute_AndNoReachableActions()
        {
            // Arrange
            var controllerType = typeof(NoActionsController);

            // Act
            var entries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            // Assert
            Assert.Empty(entries);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithControllerRoute_ExcludesAttributeRoute()
        {
            // Arrange
            var controllerType = typeof(MixedRoutingController);

            // Act
            var entries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            // Assert
            var controllerEntry = Assert.Single(entries, r => !r.Route.Defaults.ContainsKey("action"));
            Assert.Same(controllerType, controllerEntry.Route.GetTargetControllerDescriptor().ControllerType);

            var actionMethods = controllerEntry.Route.GetTargetActionDescriptors().ToArray();
            Assert.Single(actionMethods);
            Assert.Single(actionMethods, a => a.ActionName == "GoodAction");

            var actionEntry = Assert.Single(entries, r => r.Route.Defaults.ContainsKey("action"));
            Assert.Equal("DirectRouteAction", Assert.Single(actionEntry.Route.GetTargetActionDescriptors()).ActionName);
        }

        [Fact]
        public void MapMvcAttributeRoutes_SetsTargetIsAction()
        {
            // Arrange
            var controllerType = typeof(MixedRoutingController);

            // Act
            var entries = AttributeRoutingMapper.GetAttributeRoutes(controllerType);

            // Assert
            var controllerEntry = Assert.Single(entries, r => !r.Route.Defaults.ContainsKey("action"));
            Assert.False(controllerEntry.Route.GetTargetIsAction());

            var actionEntry = Assert.Single(entries, r => r.Route.Defaults.ContainsKey("action"));
            Assert.True(actionEntry.Route.GetTargetIsAction());
        }

        [Fact]
        public void MapMvcAttributeRoutes_ValidatesConstraints()
        {
            // Arrange
            var controllerType = typeof(InvalidConstraintController);

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template 'invalidconstraint/{action}' " +
                "must have a string value or be of a type which implements 'System.Web.Routing.IRouteConstraint'.";


            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.GetAttributeRoutes(controllerType), expectedMessage);
        }

        [InvalidConstraintRoute("invalidconstraint/{action}")]
        public class InvalidConstraintController : Controller
        {
            public void A1()
            {
            }
        }

        public class InvalidConstraintRouteAttribute : RouteFactoryAttribute
        {
            public InvalidConstraintRouteAttribute(string template)
                : base(template)
            {
            }

            public override RouteValueDictionary Constraints
            {
                get
                {
                    var result = new RouteValueDictionary();
                    result.Add("custom", new Uri("http://localhost"));
                    return result;
                }
            }
        }

        public class MyController : Controller
        {
            [HttpGet]
            [Route("")]
            [ActionName("ActionName")]
            public void MethodName()
            {
            }
        }

        [Route("controller/{action}")]
        public class AnotherController : Controller
        {
            public void RegularAction()
            {
            }

            public void AnotherAction()
            {
            }
        }

        [Route("controller/{action}")]
        public class NoActionsController : Controller
        {
        }

        [Route("controller/{action}")]
        public class MixedRoutingController : Controller
        {
            [Route("Yep")]
            public void DirectRouteAction()
            {
            }

            public void GoodAction()
            {
            }
        }
    }
}
