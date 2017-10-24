// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class TaskAsyncActionDescriptorTest
    {
        private readonly MethodInfo _taskMethod = typeof(ExecuteController).GetMethod("SimpleTask");

        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            string actionName = "SomeAction";
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act
            TaskAsyncActionDescriptor ad = new TaskAsyncActionDescriptor(_taskMethod, actionName, cd);

            // Assert
            Assert.Equal(_taskMethod, ad.TaskMethodInfo);
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
                delegate { new TaskAsyncActionDescriptor(_taskMethod, "", cd); }, "actionName");
        }

        [Fact]
        public void Constructor_ThrowsIfActionNameIsNull()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new TaskAsyncActionDescriptor(_taskMethod, null, cd); }, "actionName");
        }

        [Fact]
        public void Constructor_ThrowsIfTaskMethodInfoIsInvalid()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            MethodInfo getHashCodeMethod = typeof(object).GetMethod("GetHashCode");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new TaskAsyncActionDescriptor(getHashCodeMethod, "SomeAction", cd); },
                "Cannot create a descriptor for instance method 'Int32 GetHashCode()' on type 'System.Object' because the type does not derive from ControllerBase." + Environment.NewLine
              + "Parameter name: taskMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfTaskMethodInfoIsNull()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new TaskAsyncActionDescriptor(null, "SomeAction", cd); }, "taskMethodInfo");
        }

        [Fact]
        public void Constructor_ThrowsIfControllerDescriptorIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new TaskAsyncActionDescriptor(_taskMethod, "SomeAction", null); }, "controllerDescriptor");
        }

        [Fact]
        public void ExecuteTask()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("SimpleTask"));

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "doWork", true }
            };

            ControllerContext controllerContext = GetControllerContext();

            // Act
            object retVal = ExecuteHelper(actionDescriptor, parameters, controllerContext);

            // Assert
            Assert.Null(retVal);
            Assert.True((controllerContext.Controller as ExecuteController).WorkDone);
        }

        [Fact]
        public void ExecuteTaskGeneric()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("GenericTask"));

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "taskId", "foo" }
            };

            // Act
            object retVal = ExecuteHelper(actionDescriptor, parameters);

            // Assert
            Assert.Equal("foo", retVal);
        }

        [Fact]
        public void ExecuteTaskPreservesStackTraceOnException()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("SimpleTaskException"));

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "doWork", true }
            };

            // Act
            IAsyncResult result = actionDescriptor.BeginExecute(GetControllerContext(), parameters, null, null);

            // Assert
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => actionDescriptor.EndExecute(result),
                "Test exception from action"
                );

            Assert.Contains("System.Web.Mvc.Async.Test.TaskAsyncActionDescriptorTest.ExecuteController.", ex.StackTrace);
        }

        [Fact]
        public void ExecuteTaskGenericPreservesStackTraceOnException()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("GenericTaskException"));

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "taskId", "foo" },
                { "throwException", true }
            };

            // Act
            IAsyncResult result = actionDescriptor.BeginExecute(GetControllerContext(), parameters, null, null);

            // Assert
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => actionDescriptor.EndExecute(result),
                "Test exception from action"
                );

            Assert.Contains("System.Web.Mvc.Async.Test.TaskAsyncActionDescriptorTest.ExecuteController.", ex.StackTrace);
        }

        [Fact]
        public void ExecuteTaskOfPrivateT()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("TaskOfPrivateT"));
            ControllerContext controllerContext = GetControllerContext();

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Act
            object retVal = ExecuteHelper(actionDescriptor, parameters, controllerContext);

            // Assert
            Assert.Null(retVal);
            Assert.True((controllerContext.Controller as ExecuteController).WorkDone);
        }

        [Fact]
        public void ExecuteTaskPreservesState()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("SimpleTask"));

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "doWork", true }
            };

            ControllerContext controllerContext = GetControllerContext();

            // Act
            TaskWrapperAsyncResult result = (TaskWrapperAsyncResult)actionDescriptor.BeginExecute(GetControllerContext(), parameters, callback: null, state: "state");

            // Assert
            Assert.Equal("state", result.AsyncState);
        }

        [Fact]
        public void ExecuteTaskWithNullParameterAndTimeout()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("TaskTimeoutWithNullParam"));

            Dictionary<string, object> token = new Dictionary<string, object>()
            {
                { "nullParam", null },
                { "cancellationToken", new CancellationToken() }
            };

            // Act & assert
            Assert.Throws<TimeoutException>(
                () => actionDescriptor.EndExecute(actionDescriptor.BeginExecute(GetControllerContext(0), parameters: token, callback: null, state: null)),
                "The operation has timed out."
                );
        }

        [Fact]
        public void ExecuteWithInfiniteTimeout()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("TaskWithInfiniteTimeout"));
            ControllerContext controllerContext = GetControllerContext(Timeout.Infinite);

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "cancellationToken", new CancellationToken() }
            };

            // Act
            object retVal = ExecuteHelper(actionDescriptor, parameters);

            // Assert
            Assert.Equal("Task Completed", retVal);
        }

        [Fact]
        public void ExecuteTaskWithImmediateTimeout()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("TaskTimeout"));

            Dictionary<string, object> token = new Dictionary<string, object>()
            {
                { "cancellationToken", new CancellationToken() }
            };

            // Act & assert
            Assert.Throws<TimeoutException>(
                () => actionDescriptor.EndExecute(actionDescriptor.BeginExecute(GetControllerContext(0), parameters: token, callback: null, state: null)),
                "The operation has timed out."
                );
        }

        [Fact]
        public void ExecuteTaskWithTimeout()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("TaskTimeout"));

            Dictionary<string, object> token = new Dictionary<string, object>()
            {
                { "cancellationToken", new CancellationToken() }
            };

            // Act & assert
            Assert.Throws<TimeoutException>(
                () => actionDescriptor.EndExecute(actionDescriptor.BeginExecute(GetControllerContext(2000), parameters: token, callback: null, state: null)),
                "The operation has timed out."
                );
        }

        [Fact]
        public void SynchronousExecuteThrows()
        {
            // Arrange
            TaskAsyncActionDescriptor actionDescriptor = GetActionDescriptor(GetExecuteControllerMethodInfo("SimpleTask"));

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { actionDescriptor.Execute(new ControllerContext(), new Dictionary<string, object>()); }, "The asynchronous action method 'someName' returns a Task, which cannot be executed synchronously.");
        }

        [Fact]
        public void Execute_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            TaskAsyncActionDescriptor ad = GetActionDescriptor(_taskMethod);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.BeginExecute(null, new Dictionary<string, object>(), null, null); }, "controllerContext");
        }

        [Fact]
        public void Execute_ThrowsIfControllerIsNotAsyncManagerContainer()
        {
            // Arrange
            TaskAsyncActionDescriptor ad = GetActionDescriptor(_taskMethod);
            ControllerContext controllerContext = new ControllerContext()
            {
                Controller = new RegularSyncController()
            };

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "doWork", true }
            };

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { ad.BeginExecute(controllerContext, parameters, null, null); },
                @"The controller of type 'System.Web.Mvc.Async.Test.TaskAsyncActionDescriptorTest+RegularSyncController' must subclass AsyncController or implement the IAsyncManagerContainer interface.");
        }

        [Fact]
        public void Execute_ThrowsIfParametersIsNull()
        {
            // Arrange
            TaskAsyncActionDescriptor ad = GetActionDescriptor(_taskMethod);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.BeginExecute(new ControllerContext(), null, null, null); }, "parameters");
        }

        [Fact]
        public void GetCustomAttributesCallsMethodInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();
            mockMethod.Setup(mi => mi.GetCustomAttributes(true)).Returns(expected);
            TaskAsyncActionDescriptor ad = new TaskAsyncActionDescriptor(mockMethod.Object, "someName", new Mock<ControllerDescriptor>().Object, validateMethod: false)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };

            // Act
            object[] returned = ad.GetCustomAttributes(true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetCustomAttributesWithAttributeTypeCallsMethodInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();
            mockMethod.Setup(mi => mi.GetCustomAttributes(typeof(ObsoleteAttribute), true)).Returns(expected);
            TaskAsyncActionDescriptor ad = new TaskAsyncActionDescriptor(mockMethod.Object, "someName", new Mock<ControllerDescriptor>().Object, validateMethod: false)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };

            // Act
            object[] returned = ad.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetParameters()
        {
            // Arrange
            ParameterInfo pInfo = _taskMethod.GetParameters()[0];
            TaskAsyncActionDescriptor ad = GetActionDescriptor(_taskMethod);

            // Act
            ParameterDescriptor[] pDescsFirstCall = ad.GetParameters();
            ParameterDescriptor[] pDescsSecondCall = ad.GetParameters();

            // Assert
            Assert.NotSame(pDescsFirstCall, pDescsSecondCall); // Should get a new array every time
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

            TaskAsyncActionDescriptor ad = new TaskAsyncActionDescriptor(mockMethod.Object, "someName", new Mock<ControllerDescriptor>().Object, validateMethod: false)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };

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
            TaskAsyncActionDescriptor ad = GetActionDescriptor(_taskMethod);

            // Act
            bool isDefined = ad.IsDefined(typeof(AuthorizeAttribute), inherit: true);

            // Assert
            Assert.True(isDefined);
        }

        public static object ExecuteHelper(TaskAsyncActionDescriptor actionDescriptor, Dictionary<string, object> parameters, ControllerContext controllerContext = null)
        {
            using (SignalContainer<object> resultContainer = new SignalContainer<object>())
            {
                AsyncCallback callback = ar =>
                {
                    object o = actionDescriptor.EndExecute(ar);
                    resultContainer.Signal(o);
                };

                actionDescriptor.BeginExecute(controllerContext ?? GetControllerContext(), parameters, callback, state: null);
                return resultContainer.Wait();
            }
        }

        private static TaskAsyncActionDescriptor GetActionDescriptor(MethodInfo taskMethod)
        {
            return new TaskAsyncActionDescriptor(taskMethod, "someName", new Mock<ControllerDescriptor>().Object)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };
        }

        private static ControllerContext GetControllerContext(int timeout = 45 * 1000)
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            ExecuteController controller = new ExecuteController();
            controller.AsyncManager.Timeout = timeout;
            mockControllerContext.Setup(c => c.Controller).Returns(controller);
            return mockControllerContext.Object;
        }

        private static MethodInfo GetExecuteControllerMethodInfo(string methodName)
        {
            return typeof(ExecuteController).GetMethod(methodName);
        }

        private class ExecuteController : AsyncController
        {
            public bool WorkDone { get; set; }

            public Task<ActionResult> ReturnedTask { get; set; }

            public Task<string> GenericTask(string taskId)
            {
                return Task.Factory.StartNew(() => taskId);
            }

            public Task<string> GenericTaskException(string taskId, bool throwException)
            {
                return Task.Factory.StartNew(() =>
                {
                    if (throwException)
                    {
                        ThrowException();
                    }
                    ;
                    return taskId;
                });
            }

            private void ThrowException()
            {
                throw new InvalidOperationException("Test exception from action");
            }

            [Authorize]
            public Task SimpleTask(bool doWork)
            {
                return Task.Factory.StartNew(() => { WorkDone = doWork; });
            }

            public Task SimpleTaskException(bool doWork)
            {
                return Task.Factory.StartNew(() => { ThrowException(); });
            }

            public Task<ActionResult> TaskTimeoutWithNullParam(Object nullParam, CancellationToken cancellationToken)
            {
                return TaskTimeout(cancellationToken);
            }

            public Task<string> TaskWithInfiniteTimeout(CancellationToken cancellationToken)
            {
                return Task.Factory.StartNew(() => "Task Completed");
            }

            public Task<ActionResult> TaskTimeout(CancellationToken cancellationToken)
            {
                TaskCompletionSource<ActionResult> completionSource = new TaskCompletionSource<ActionResult>();
                cancellationToken.Register(() => completionSource.TrySetCanceled());
                ReturnedTask = completionSource.Task;
                return ReturnedTask;
            }

            public Task TaskOfPrivateT()
            {
                var completionSource = new TaskCompletionSource<PrivateObject>();
                completionSource.SetResult(new PrivateObject());
                WorkDone = true;
                return completionSource.Task;
            }

            private class PrivateObject
            {
                public override string ToString()
                {
                    return "Private Object";
                }
            }
        }

        // Controller is async, so derive from ControllerBase to get sync behavior.
        private class RegularSyncController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }
    }
}
