// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Web.Routing.Test;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ActionMethodSelectorTest
    {
        [Fact]
        public void AliasedMethodsProperty()
        {
            // Arrange
            Type controllerType = typeof(MethodLocatorController);

            // Act
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Assert
            Assert.Equal(2, selector.AliasedMethods.Length);

            List<MethodInfo> sortedAliasedMethods = selector.AliasedMethods.OrderBy(methodInfo => methodInfo.Name).ToList();
            Assert.Equal("Bar", sortedAliasedMethods[0].Name);
            Assert.Equal("FooRenamed", sortedAliasedMethods[1].Name);
        }

        [Fact]
        public void ControllerTypeProperty()
        {
            // Arrange
            Type controllerType = typeof(MethodLocatorController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act & Assert
            Assert.Same(controllerType, selector.ControllerType);
        }

        [Fact]
        public void FindActionMethodReturnsMatchingMethodIfOneMethodMatches()
        {
            // Arrange
            Type controllerType = typeof(SelectionAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "OneMatch");

            // Assert
            Assert.Equal("OneMatch", matchedMethod.Name);
            Assert.Equal(typeof(string), matchedMethod.GetParameters()[0].ParameterType);
        }

        [Fact]
        public void FindActionMethodReturnsMethodWithActionSelectionAttributeIfMultipleMethodsMatchRequest()
        {
            // DevDiv Bugs 212062: If multiple action methods match a request, we should match only the methods with an
            // [ActionMethod] attribute since we assume those methods are more specific.

            // Arrange
            Type controllerType = typeof(SelectionAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "ShouldMatchMethodWithSelectionAttribute");

            // Assert
            Assert.Equal("MethodHasSelectionAttribute1", matchedMethod.Name);
        }

        [Fact]
        public void FindActionMethodReturnsMethodWithoutAttributeWhenOthersOptOut()
        {
            // Arrange
            Type controllerType = typeof(ControllerMatchMiddleNoAttribute);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "MiddleMatch");

            // Assert
            Assert.Equal("MiddleMatch", matchedMethod.Name);
        }

        [Fact]
        public void FindActionMethodReturnsMethodWithAttributeWhenOthersOptOutAndNoAttribute()
        {
            // Arrange
            Type controllerType = typeof(ControllerMatchMiddleWithAttribute);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "MiddleMatch");

            // Assert
            Assert.Equal("MiddleMatch", matchedMethod.Name);
        }

        [Fact]
        public void FindActionMethodReturnsMethodMultipleMatchesOverOneOptOut()
        {
            // Arrange
            Type controllerType = typeof(ControllerMatchMultipleAttributes);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "Match");

            // Assert
            Assert.Equal("Match", matchedMethod.Name);
        }

        [Fact]
        public void FindActionMethodReturnsNullIfNoMethodMatches()
        {
            // Arrange
            Type controllerType = typeof(SelectionAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "ZeroMatch");

            // Assert
            Assert.Null(matchedMethod);
        }

        [Fact]
        public void FindActionMethod_IgnoresDirectRouteAttributedMethods()
        {
            // Arrange
            Type controllerType = typeof(WithRoutingAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // This simulates what AttributeRoutingMapper will do
            selector.StandardRouteMethods.Remove(controllerType.GetMethod("ActionWithoutRoute"));
            selector.StandardRouteMethods.Remove(controllerType.GetMethod("DirectRouteOnly"));

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "Action");

            // Assert
            Assert.NotNull(matchedMethod);
            Assert.Equal("Action", matchedMethod.Name);
        }

        [Fact]
        public void FindActionMethod_IgnoresDirectRouteAttributedMethods_NoMatch()
        {
            // Arrange
            Type controllerType = typeof(WithRoutingAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // This simulates what AttributeRoutingMapper will do
            selector.StandardRouteMethods.Remove(controllerType.GetMethod("ActionWithoutRoute"));
            selector.StandardRouteMethods.Remove(controllerType.GetMethod("DirectRouteOnly"));

            // Act
            MethodInfo matchedMethod = selector.FindActionMethod(new ControllerContext(), "DirectRouteOnly");

            // Assert
            Assert.Null(matchedMethod);
        }

        [Fact]
        public void FindActionMethodThrowsIfMultipleMethodsMatch()
        {
            // Arrange
            Type controllerType = typeof(SelectionAttributeController);
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Act & veriy
            Assert.Throws<AmbiguousMatchException>(
                delegate { selector.FindActionMethod(new ControllerContext(), "TwoMatch"); },
                "The current request for action 'TwoMatch' on controller type 'SelectionAttributeController' is ambiguous between the following action methods:" + Environment.NewLine
              + "Void TwoMatch2() on type System.Web.Mvc.Test.ActionMethodSelectorTest+SelectionAttributeController" + Environment.NewLine
              + "Void TwoMatch() on type System.Web.Mvc.Test.ActionMethodSelectorTest+SelectionAttributeController");
        }

        [Fact]
        public void NonAliasedMethodsProperty()
        {
            // Arrange
            Type controllerType = typeof(MethodLocatorController);

            // Act
            ActionMethodSelector selector = new ActionMethodSelector(controllerType);

            // Assert
            Assert.Single(selector.NonAliasedMethods);

            List<MethodInfo> sortedMethods = selector.NonAliasedMethods["foo"].OrderBy(methodInfo => methodInfo.GetParameters().Length).ToList();
            Assert.Equal("Foo", sortedMethods[0].Name);
            Assert.Empty(sortedMethods[0].GetParameters());
            Assert.Equal("Foo", sortedMethods[1].Name);
            Assert.Equal(typeof(string), sortedMethods[1].GetParameters()[0].ParameterType);
        }

        private class MethodLocatorController : Controller
        {
            public void Foo()
            {
            }

            public void Foo(string s)
            {
            }

            [ActionName("Foo")]
            public void FooRenamed()
            {
            }

            [ActionName("Bar")]
            public void Bar()
            {
            }

            [ActionName("PrivateVoid")]
            private void PrivateVoid()
            {
            }

            protected void ProtectedVoidAction()
            {
            }

            public static void StaticMethod()
            {
            }

            // ensure that methods inheriting from Controller or a base class are not matched
            [ActionName("Blah")]
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }

            public string StringProperty { get; set; }

#pragma warning disable 0067
            public event EventHandler<EventArgs> SomeEvent;
#pragma warning restore 0067
        }

        private class ControllerMatchMiddleNoAttribute : Controller
        {
            [Match(false)]
            [ActionName("MiddleMatch")]
            public void SkipMatchBefore()
            {
            }

            public void MiddleMatch()
            {
            }

            [Match(false)]
            [ActionName("MiddleMatch")]
            public void SkipMatchAfter()
            {
            }
        }

        private class ControllerMatchMultipleAttributes : Controller
        {
            [Match(false)]
            [Match(true)]
            [ActionName("Match")]
            public void SkipMatchBeforeOneOptOut()
            {
            }

            [Match(true)]
            [Match(true)]
            public void Match()
            {
            }

            [ActionName("Match")]
            public void SkipMatchAfterNoAttribute()
            {
            }
        }

        private class ControllerMatchMiddleWithAttribute : Controller
        {
            [Match(false)]
            [ActionName("MiddleMatch")]
            public void SkipMatchBeforeNonMatch()
            {
            }

            [ActionName("MiddleMatch")]
            public void SkipMatchBeforeNoSelection()
            {
            }

            [Match(true)]
            public void MiddleMatch()
            {
            }

            [ActionName("MiddleMatch")]
            public void SkipMatchAfterNoSelection()
            {
            }

            [Match(false)]
            [ActionName("MiddleMatch")]
            public void SkipMatchAfterNonMatch()
            {
            }
        }

        private class SelectionAttributeController : Controller
        {
            [Match(false)]
            public void OneMatch()
            {
            }

            public void OneMatch(string s)
            {
            }

            public void TwoMatch()
            {
            }

            [ActionName("TwoMatch")]
            public void TwoMatch2()
            {
            }

            [Match(true), ActionName("ShouldMatchMethodWithSelectionAttribute")]
            public void MethodHasSelectionAttribute1()
            {
            }

            [ActionName("ShouldMatchMethodWithSelectionAttribute")]
            public void MethodDoesNotHaveSelectionAttribute1()
            {
            }
        }

        private class WithRoutingAttributeController : Controller
        {
            [Route("route")]
            [ActionName("Action")] // to make things confusing
            public void ActionWithoutRoute()
            {
            }

            public void Action()
            {
            }

            [Route("routeonly")]
            public void DirectRouteOnly()
            {
            }
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class MatchAttribute : ActionMethodSelectorAttribute
        {
            private bool _match;

            public MatchAttribute(bool match)
            {
                _match = match;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
            {
                return _match;
            }
        }
    }
}
