// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using ExpressionHelper = Microsoft.Web.Mvc.Internal.ExpressionHelper;

namespace Microsoft.Web.Mvc.Test
{
    public class ExpressionHelperTest
    {
        [Fact]
        public void BuildRouteValueDictionary_TargetsAsynchronousAsyncMethod_StripsSuffix()
        {
            // Arrange
            Expression<Action<TestAsyncController>> expr = (c => c.AsynchronousAsync());

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(expr);

            // Assert
            Assert.Equal("Asynchronous", rvd["action"]);
            Assert.Equal("TestAsync", rvd["controller"]);
            Assert.False(rvd.ContainsKey("area"));
        }

        [Fact]
        public void BuildRouteValueDictionary_TargetsAsynchronousCompletedMethod_Throws()
        {
            // Arrange
            Expression<Action<TestAsyncController>> expr = (c => c.AsynchronousCompleted());

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { ExpressionHelper.GetRouteValuesFromExpression(expr); },
                @"The method 'AsynchronousCompleted' is an asynchronous completion method and cannot be called directly.");
        }

        [Fact]
        public void BuildRouteValueDictionary_TargetsControllerWithAreaAttribute_AddsAreaName()
        {
            // Arrange
            Expression<Action<ControllerWithAreaController>> expr = c => c.Index();

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(expr);

            // Assert
            Assert.Equal("Index", rvd["action"]);
            Assert.Equal("ControllerWithArea", rvd["controller"]);
            Assert.Equal("the area name", rvd["area"]);
        }

        [Fact]
        public void BuildRouteValueDictionary_TargetsNonActionMethod_Throws()
        {
            // Arrange
            Expression<Action<TestController>> expr = (c => c.NotAnAction());

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { ExpressionHelper.GetRouteValuesFromExpression(expr); },
                @"The method 'NotAnAction' is marked [NonAction] and cannot be called directly.");
        }

        [Fact]
        public void BuildRouteValueDictionary_TargetsRenamedMethod_UsesNewName()
        {
            // Arrange
            Expression<Action<TestController>> expr = (c => c.Renamed());

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(expr);

            // Assert
            Assert.Equal("NewName", rvd["action"]);
            Assert.Equal("Test", rvd["controller"]);
            Assert.False(rvd.ContainsKey("area"));
        }

        [Fact]
        public void BuildRouteValueDictionary_TargetsSynchronousMethodOnAsyncController_ReturnsOriginalName()
        {
            // Arrange
            Expression<Action<TestAsyncController>> expr = (c => c.Synchronous());

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(expr);

            // Assert
            Assert.Equal("Synchronous", rvd["action"]);
            Assert.Equal("TestAsync", rvd["controller"]);
            Assert.False(rvd.ContainsKey("area"));
        }

        [Fact]
        public void BuildRouteValueDictionaryWithNullExpressionThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(
                () => ExpressionHelper.GetRouteValuesFromExpression<TestController>(null),
                "action");
        }

        [Fact]
        public void BuildRouteValueDictionaryWithNonMethodExpressionThrowsInvalidOperationException()
        {
            // Arrange
            Expression<Action<TestController>> expression = c => new TestController();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => ExpressionHelper.GetRouteValuesFromExpression(expression),
                "Expression must be a method call." + Environment.NewLine
              + "Parameter name: action");
        }

        [Fact]
        public void BuildRouteValueDictionaryWithoutControllerSuffixThrowsInvalidOperationException()
        {
            // Arrange
            Expression<Action<TestControllerNot>> index = (c => c.Index(123));

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => ExpressionHelper.GetRouteValuesFromExpression(index),
                "Controller name must end in 'Controller'." + Environment.NewLine
              + "Parameter name: action");
        }

        [Fact]
        public void BuildRouteValueDictionaryWithControllerBaseClassThrowsInvalidOperationException()
        {
            // Arrange
            Expression<Action<Controller>> index = (c => c.Dispose());

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => ExpressionHelper.GetRouteValuesFromExpression(index),
                "Cannot route to class named 'Controller'." + Environment.NewLine
              + "Parameter name: action");
        }

        [Fact]
        public void BuildRouteValueDictionaryAddsControllerNameToDictionary()
        {
            // Arrange
            Expression<Action<TestController>> index = (c => c.Index(123));

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(index);

            // Assert
            Assert.Equal("Test", rvd["Controller"]);
        }

        [Fact]
        public void BuildRouteValueDictionaryFromExpressionReturnsCorrectDictionary()
        {
            // Arrange
            Expression<Action<TestController>> index = (c => c.Index(123));

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(index);

            // Assert
            Assert.Equal("Test", rvd["Controller"]);
            Assert.Equal("Index", rvd["Action"]);
            Assert.Equal(123, rvd["page"]);
        }

        [Fact]
        public void BuildRouteValueDictionaryFromNonConstantExpressionReturnsCorrectDictionary()
        {
            // Arrange
            Expression<Action<TestController>> index = (c => c.About(Foo));

            // Act
            RouteValueDictionary rvd = ExpressionHelper.GetRouteValuesFromExpression(index);

            // Assert
            Assert.Equal("Test", rvd["Controller"]);
            Assert.Equal("About", rvd["Action"]);
            Assert.Equal("FooValue", rvd["s"]);
        }

        [Fact]
        public void GetInputNameFromPropertyExpressionReturnsPropertyName()
        {
            // Arrange
            Expression<Func<TestModel, int>> expression = m => m.IntProperty;

            // Act
            string name = ExpressionHelper.GetInputName(expression);

            // Assert
            Assert.Equal("IntProperty", name);
        }

        [Fact]
        public void GetInputNameFromPropertyWithMethodCallExpressionReturnsPropertyName()
        {
            // Arrange
            Expression<Func<TestModel, string>> expression = m => m.IntProperty.ToString();

            // Act
            string name = ExpressionHelper.GetInputName(expression);

            // Assert
            Assert.Equal("IntProperty", name);
        }

        [Fact]
        public void GetInputNameFromPropertyWithTwoMethodCallExpressionReturnsPropertyName()
        {
            // Arrange
            Expression<Func<TestModel, string>> expression = m => m.IntProperty.ToString().ToUpper();

            // Act
            string name = ExpressionHelper.GetInputName(expression);

            // Assert
            Assert.Equal("IntProperty", name);
        }

        [Fact]
        public void GetInputNameFromExpressionWithTwoPropertiesUsesWholeExpression()
        {
            // Arrange
            Expression<Func<TestModel, int>> expression = m => m.StringProperty.Length;

            // Act
            string name = ExpressionHelper.GetInputName(expression);

            // Assert
            Assert.Equal("StringProperty.Length", name);
        }

        public class TestController : Controller
        {
            public ActionResult Index(int page)
            {
                return null;
            }

            public string About(string s)
            {
                return "The value is " + s;
            }

            [ActionName("NewName")]
            public void Renamed()
            {
            }

            [NonAction]
            public void NotAnAction()
            {
            }
        }

        public class TestAsyncController : AsyncController
        {
            public void Synchronous()
            {
            }

            public void AsynchronousAsync()
            {
            }

            public void AsynchronousCompleted()
            {
            }
        }

        public string Foo
        {
            get { return "FooValue"; }
        }

        public class TestControllerNot : Controller
        {
            public ActionResult Index(int page)
            {
                return null;
            }
        }

        [ActionLinkArea("the area name")]
        public class ControllerWithAreaController : Controller
        {
            public ActionResult Index()
            {
                return null;
            }
        }

        public class TestModel
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
        }
    }
}
