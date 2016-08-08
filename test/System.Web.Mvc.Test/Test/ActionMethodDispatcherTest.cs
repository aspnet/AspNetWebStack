// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ActionMethodDispatcherTest
    {
        [Fact]
        public void ExecuteWithNormalActionMethod()
        {
            // Arrange
            DispatcherController controller = new DispatcherController();
            object[] parameters = new object[] { 5, "some string", new DateTime(2001, 1, 1) };
            MethodInfo methodInfo = typeof(DispatcherController).GetMethod("NormalAction");
            ActionMethodDispatcher dispatcher = new ActionMethodDispatcher(methodInfo);

            // Act
            object returnValue = dispatcher.Execute(controller, parameters);

            // Assert
            var stringResult = Assert.IsType<string>(returnValue);
            Assert.Equal("Hello from NormalAction!", stringResult);

            Assert.Equal(5, controller._i);
            Assert.Equal("some string", controller._s);
            Assert.Equal(new DateTime(2001, 1, 1), controller._dt);
        }

        [Fact]
        public void ExecuteWithParameterlessActionMethod()
        {
            // Arrange
            DispatcherController controller = new DispatcherController();
            object[] parameters = new object[0];
            MethodInfo methodInfo = typeof(DispatcherController).GetMethod("ParameterlessAction");
            ActionMethodDispatcher dispatcher = new ActionMethodDispatcher(methodInfo);

            // Act
            object returnValue = dispatcher.Execute(controller, parameters);

            // Assert
            var intResult = Assert.IsType<int>(returnValue);
            Assert.Equal(53, intResult);
        }

        [Fact]
        public void ExecuteWithStaticActionMethod()
        {
            // Arrange
            DispatcherController controller = new DispatcherController();
            object[] parameters = new object[0];
            MethodInfo methodInfo = typeof(DispatcherController).GetMethod("StaticAction");
            ActionMethodDispatcher dispatcher = new ActionMethodDispatcher(methodInfo);

            // Act
            object returnValue = dispatcher.Execute(controller, parameters);

            // Assert
            var intResult = Assert.IsType<int>(returnValue);
            Assert.Equal(89, intResult);
        }

        [Fact]
        public void ExecuteWithVoidActionMethod()
        {
            // Arrange
            DispatcherController controller = new DispatcherController();
            object[] parameters = new object[] { 5, "some string", new DateTime(2001, 1, 1) };
            MethodInfo methodInfo = typeof(DispatcherController).GetMethod("VoidAction");
            ActionMethodDispatcher dispatcher = new ActionMethodDispatcher(methodInfo);

            // Act
            object returnValue = dispatcher.Execute(controller, parameters);

            // Assert
            Assert.Null(returnValue);
            Assert.Equal(5, controller._i);
            Assert.Equal("some string", controller._s);
            Assert.Equal(new DateTime(2001, 1, 1), controller._dt);
        }

        [Fact]
        public void MethodInfoProperty()
        {
            // Arrange
            MethodInfo original = typeof(object).GetMethod("ToString");
            ActionMethodDispatcher dispatcher = new ActionMethodDispatcher(original);

            // Act
            MethodInfo returned = dispatcher.MethodInfo;

            // Assert
            Assert.Same(original, returned);
        }

        private class DispatcherController : Controller
        {
            public int _i;
            public string _s;
            public DateTime _dt;

            public object NormalAction(int i, string s, DateTime dt)
            {
                VoidAction(i, s, dt);
                return "Hello from NormalAction!";
            }

            public int ParameterlessAction()
            {
                return 53;
            }

            public void VoidAction(int i, string s, DateTime dt)
            {
                _i = i;
                _s = s;
                _dt = dt;
            }

            public static int StaticAction()
            {
                return 89;
            }
        }
    }
}
