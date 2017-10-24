// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class ReflectedAsyncActionDescriptorTest
    {
        private readonly MethodInfo _asyncMethod = typeof(ExecuteController).GetMethod("FooAsync");
        private readonly MethodInfo _completedMethod = typeof(ExecuteController).GetMethod("FooCompleted");

        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            string actionName = "SomeAction";
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act
            ReflectedAsyncActionDescriptor ad = new ReflectedAsyncActionDescriptor(_asyncMethod, _completedMethod, actionName, cd);

            // Assert
            Assert.Equal(_asyncMethod, ad.AsyncMethodInfo);
            Assert.Equal(_completedMethod, ad.CompletedMethodInfo);
            Assert.Equal(actionName, ad.ActionName);
            Assert.Equal(cd, ad.ControllerDescriptor);
        }

        [Fact]
        public void Constructor_ThrowsIfActionNameIsEmpty()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new ReflectedAsyncActionDescriptor(_asyncMethod, _completedMethod, "", cd); }, "actionName");
        }

        [Fact]
        public void Constructor_ThrowsIfActionNameIsNull()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new ReflectedAsyncActionDescriptor(_asyncMethod, _completedMethod, null, cd); }, "actionName");
        }

        [Fact]
        public void Constructor_ThrowsIfAsyncMethodInfoIsInvalid()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            MethodInfo getHashCodeMethod = typeof(object).GetMethod("GetHashCode");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedAsyncActionDescriptor(getHashCodeMethod, _completedMethod, "SomeAction", cd); },
                "Cannot create a descriptor for instance method 'Int32 GetHashCode()' on type 'System.Object' because the type does not derive from ControllerBase." + Environment.NewLine
              + "Parameter name: asyncMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfAsyncMethodInfoIsNull()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedAsyncActionDescriptor(null, _completedMethod, "SomeAction", cd); }, "asyncMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfCompletedMethodInfoIsInvalid()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            MethodInfo getHashCodeMethod = typeof(object).GetMethod("GetHashCode");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedAsyncActionDescriptor(_asyncMethod, getHashCodeMethod, "SomeAction", cd); },
                "Cannot create a descriptor for instance method 'Int32 GetHashCode()' on type 'System.Object' because the type does not derive from ControllerBase." + Environment.NewLine
              + "Parameter name: completedMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfCompletedMethodInfoIsNull()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedAsyncActionDescriptor(_asyncMethod, null, "SomeAction", cd); }, "completedMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfControllerDescriptorIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedAsyncActionDescriptor(_asyncMethod, _completedMethod, "SomeAction", null); }, "controllerDescriptor");
        }

        [Fact]
        public void Execute()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.Controller).Returns(new ExecuteController());
            ControllerContext controllerContext = mockControllerContext.Object;

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "id1", 42 }
            };

            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            using (SignalContainer<object> resultContainer = new SignalContainer<object>())
            {
                AsyncCallback callback = ar =>
                {
                    object o = ad.EndExecute(ar);
                    resultContainer.Signal(o);
                };

                // Act
                ad.BeginExecute(controllerContext, parameters, callback, null);
                object retVal = resultContainer.Wait();

                // Assert
                Assert.Equal("Hello world: 42", retVal);
            }
        }

        [Fact]
        public void Execute_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.BeginExecute(null, new Dictionary<string, object>(), null, null); }, "controllerContext");
        }

        [Fact]
        public void Execute_ThrowsIfControllerIsNotAsyncManagerContainer()
        {
            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);
            ControllerContext controllerContext = new ControllerContext()
            {
                Controller = new RegularSyncController()
            };

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { ad.BeginExecute(controllerContext, new Dictionary<string, object>(), null, null); },
                "The controller of type 'System.Web.Mvc.Async.Test.ReflectedAsyncActionDescriptorTest+RegularSyncController' must subclass AsyncController or implement the IAsyncManagerContainer interface.");
        }

        [Fact]
        public void Execute_ThrowsIfParametersIsNull()
        {
            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.BeginExecute(new ControllerContext(), null, null, null); }, "parameters");
        }

        [Fact]
        public void GetCustomAttributes()
        {
            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act
            object[] attributes = ad.GetCustomAttributes(true /* inherit */);

            // Assert
            object attribute = Assert.Single(attributes);
            Assert.IsType<AuthorizeAttribute>(attribute);
        }

        [Fact]
        public void GetCustomAttributes_FilterByType()
        {
            // Shouldn't match attributes on the Completed() method, only the Async() method

            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act
            object[] attributes = ad.GetCustomAttributes(typeof(OutputCacheAttribute), true /* inherit */);

            // Assert
            Assert.Empty(attributes);
        }

        [Fact]
        public void GetParameters()
        {
            // Arrange
            ParameterInfo pInfo = _asyncMethod.GetParameters()[0];
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act
            ParameterDescriptor[] pDescsFirstCall = ad.GetParameters();
            ParameterDescriptor[] pDescsSecondCall = ad.GetParameters();

            // Assert
            Assert.NotSame(pDescsFirstCall, pDescsSecondCall);
            Assert.Equal(pDescsFirstCall, pDescsSecondCall);

            ParameterDescriptor parameterDescriptor = Assert.Single(pDescsFirstCall);
            ReflectedParameterDescriptor pDesc = Assert.IsType<ReflectedParameterDescriptor>(parameterDescriptor);

            Assert.NotNull(pDesc);
            Assert.Same(ad, pDesc.ActionDescriptor);
            Assert.Same(pInfo, pDesc.ParameterInfo);
        }

        [Fact]
        public void GetSelectors()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();

            Mock<ActionMethodSelectorAttribute> mockAttr = new Mock<ActionMethodSelectorAttribute>();
            mockAttr.Setup(attr => attr.IsValidForRequest(controllerContext, mockMethod.Object)).Returns(true).Verifiable();
            mockMethod.Setup(m => m.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), true)).Returns(new ActionMethodSelectorAttribute[] { mockAttr.Object });

            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(mockMethod.Object, _completedMethod);

            // Act
            ICollection<ActionSelector> selectors = ad.GetSelectors();
            bool executedSuccessfully = selectors.All(s => s(controllerContext));

            // Assert
            Assert.Single(selectors);
            Assert.True(executedSuccessfully);
            mockAttr.Verify();
        }

        [Fact]
        public void IsDefined()
        {
            // Arrange
            ReflectedAsyncActionDescriptor ad = GetActionDescriptor(_asyncMethod, _completedMethod);

            // Act
            bool isDefined = ad.IsDefined(typeof(AuthorizeAttribute), true /* inherit */);

            // Assert
            Assert.True(isDefined);
        }

        private static ReflectedAsyncActionDescriptor GetActionDescriptor(MethodInfo asyncMethod, MethodInfo completedMethod)
        {
            return new ReflectedAsyncActionDescriptor(asyncMethod, completedMethod, "someName", new Mock<ControllerDescriptor>().Object, false /* validateMethod */)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };
        }

        private class ExecuteController : AsyncController
        {
            private Func<object, string> _func;

            [Authorize]
            public void FooAsync(int id1)
            {
                _func = o => Convert.ToString(o, CultureInfo.InvariantCulture) + id1.ToString(CultureInfo.InvariantCulture);
                AsyncManager.Parameters["id2"] = "Hello world: ";
                AsyncManager.Finish();
            }

            [OutputCache]
            public string FooCompleted(string id2)
            {
                return _func(id2);
            }

            public string FooWithBool(bool id2)
            {
                return _func(id2);
            }

            public string FooWithException(Exception id2)
            {
                return _func(id2);
            }
        }

        private class RegularSyncController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }
    }
}
