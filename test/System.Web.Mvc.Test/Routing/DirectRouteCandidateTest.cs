// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Routing.Test
{
    public class DirectRouteCandidateTest
    {
        [Fact]
        public void SelectBestCandidate_ReturnsNullOnEmptyList()
        {
            // Arrange & Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(new List<DirectRouteCandidate>(), new ControllerContext());

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void SelectBestCandidate_SelectByActionName()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            RouteData routeData = new RouteData();
            routeData.Values["action"] = "Action1";

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action2()),
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_SelectByActionNameSelector_UsesActionName()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            RouteData routeData = new RouteData();
            routeData.Values["action"] = "Action1";

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionNameSelectors = new ActionNameSelector[] { (context, name) => name == "Action1" },
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action2()),
                ActionNameSelectors = new ActionNameSelector[] { (context, name) => name == "Action2" },
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_SelectByActionNameSelector_RouteProvidesName()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            RouteData routeData = new RouteData();
            routeData.Values["action"] = "Action1";

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action2()),
                ActionNameSelectors = new ActionNameSelector[] { (context, name) => name == "Action1" },
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action2()),
                ControllerDescriptor = controllerDescriptor,
                RouteData = routeData,
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ExcludesFailedActionSelectors()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                Order = 1,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => false, },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ExcludesFailedActionSelectors_NoneValid()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => false, },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => false, },
                ControllerDescriptor = controllerDescriptor,
                Order = 1,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByActionSelector()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByOrder()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                Order = 1,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByOrder_WithActionSelectors()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 1,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByOrder_AfterActionSelectors()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 1,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => false },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByPrecedence()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 1,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 2,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_ChoosesByPrecedence_WithActionSelectors()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate better = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 1,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate worse = new DirectRouteCandidate()
            {
                ActionDescriptor = ActionDescriptorFrom<TestController>(c => c.Action1()),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 2,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                better, 
                worse,
            };

            // Act
            DirectRouteCandidate actual = DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext());

            // Assert
            Assert.Same(better, actual);
        }

        [Fact]
        public void SelectBestCandidate_Ambiguity()
        {
            // Arrange
            Type controllerType = typeof(TestController);
            ReflectedControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            DirectRouteCandidate candidate1 = new DirectRouteCandidate()
            {
                ActionDescriptor = new ReflectedActionDescriptor(controllerType.GetMethod("Action1"), "Action1", controllerDescriptor),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 1,
                RouteData = new RouteData(),
            };

            DirectRouteCandidate candidate2 = new DirectRouteCandidate()
            {
                ActionDescriptor = new ReflectedActionDescriptor(controllerType.GetMethod("Action2"), "Action2", controllerDescriptor),
                ActionSelectors = new ActionSelector[] { (context) => true },
                ControllerDescriptor = controllerDescriptor,
                Order = 0,
                Precedence = 1,
                RouteData = new RouteData(),
            };

            List<DirectRouteCandidate> candidates = new List<DirectRouteCandidate>()
            {
                candidate1, 
                candidate2,
            };

            string message = 
                "The current request is ambiguous between the following action methods:" + Environment.NewLine +
                "Void Action1() on type System.Web.Mvc.Routing.Test.DirectRouteCandidateTest+TestController" + Environment.NewLine +
                "Void Action2() on type System.Web.Mvc.Routing.Test.DirectRouteCandidateTest+TestController";

            // Act & Assert
            Assert.Throws<AmbiguousMatchException>(() => DirectRouteCandidate.SelectBestCandidate(candidates, new ControllerContext()), message);
        }

        private static ActionDescriptor ActionDescriptorFrom<T>(Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;

            var controllerType = method.DeclaringType;
            var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            return new ReflectedActionDescriptor(method, method.Name, controllerDescriptor);
        }

        private class TestController : Controller
        {
            [Route("cool")]
            public void Action1()
            {
            }

            [Route("cool")]
            public void Action2()
            {
            }
        }

        [Route("controller/{action")]
        private class ClassLevelTestController : Controller
        {
            public void Action1()
            {
            }

            public void Action2()
            {
            }
        }
    }
}
