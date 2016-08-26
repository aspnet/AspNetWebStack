// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncControllerActionInvokerTest
    {
        [Fact]
        public void InvokeAction_ActionNotFound()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "ActionNotFound", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void InvokeAction_ActionThrowsException_Handled()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "ActionThrowsExceptionAndIsHandled", null, null);
            Assert.Null(((TestController)controllerContext.Controller).Log); // Result filter shouldn't have executed yet

            bool retVal = invoker.EndInvokeAction(asyncResult);
            Assert.True(retVal);
            Assert.Equal("From exception filter", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_ActionThrowsException_NotHandled()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.Throws<Exception>(
                delegate { invoker.BeginInvokeAction(controllerContext, "ActionThrowsExceptionAndIsNotHandled", null, null); },
                @"Some exception text.");
        }

        [Fact]
        public void InvokeAction_ActionThrowsException_ThreadAbort()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.Throws<ThreadAbortException>(
                delegate { invoker.BeginInvokeAction(controllerContext, "ActionCallsThreadAbort", null, null); });
        }

        [Fact]
        public void InvokeAction_AuthenticationFilterShortCircuits()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "AuthenticationFilterShortCircuits", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From authentication filter", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_AuthenticationFilterChallenges()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "AuthenticationFilterChallenges", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From authentication filter challenge", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_AuthenticationFilterShortCircuitsAndChallenges()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "AuthenticationFilterShortCircuitsAndChallenges", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From authentication filter challenge", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_AuthorizationFilterShortCircuits()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "AuthorizationFilterShortCircuits", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From authorization filter", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_AuthorizationFilterShortCircuitsAndChallenges()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "AuthorizationFilterShortCircuitsAndChallenges", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From authentication filter challenge", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_NormalAction()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "NormalAction", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From action", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_OverrideFindAction()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvokerWithCustomFindAction();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, actionName: "Non-ExistantAction", callback: null, state: null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            // Assert
            Assert.True(retVal);
            Assert.Equal("From action", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_RequestValidationFails()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext(passesRequestValidation: false);
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.Throws<HttpRequestValidationException>(
                delegate { invoker.BeginInvokeAction(controllerContext, "NormalAction", null, null); });
        }

        [Fact]
        public void InvokeAction_ResultThrowsException_Handled()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "ResultThrowsExceptionAndIsHandled", null, null);
            bool retVal = invoker.EndInvokeAction(asyncResult);

            Assert.True(retVal);
            Assert.Equal("From exception filter", ((TestController)controllerContext.Controller).Log);
        }

        [Fact]
        public void InvokeAction_ResultThrowsException_NotHandled()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "ResultThrowsExceptionAndIsNotHandled", null, null);
            Assert.Throws<Exception>(
                delegate { invoker.EndInvokeAction(asyncResult); },
                @"Some exception text.");
        }

        [Fact]
        public void InvokeAction_ResultThrowsException_ThreadAbort()
        {
            // Arrange
            ControllerContext controllerContext = GetControllerContext();
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            IAsyncResult asyncResult = invoker.BeginInvokeAction(controllerContext, "ResultCallsThreadAbort", null, null);
            Assert.Throws<ThreadAbortException>(
                delegate { invoker.EndInvokeAction(asyncResult); });
        }

        [Fact]
        public void InvokeAction_ThrowsIfActionNameIsEmpty()
        {
            // Arrange
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { invoker.BeginInvokeAction(new ControllerContext(), "", null, null); }, "actionName");
        }

        [Fact]
        public void InvokeAction_ThrowsIfActionNameIsNull()
        {
            // Arrange
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { invoker.BeginInvokeAction(new ControllerContext(), null, null, null); }, "actionName");
        }

        [Fact]
        public void InvokeAction_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { invoker.BeginInvokeAction(null, "someAction", null, null); }, "controllerContext");
        }

        [Fact]
        public void InvokeActionMethod_AsynchronousDescriptor()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                ActionResult expectedResult = new ViewResult();

                Mock<AsyncActionDescriptor> mockActionDescriptor = new Mock<AsyncActionDescriptor>();
                mockActionDescriptor.Setup(d => d.BeginExecute(controllerContext, parameters, It.IsAny<AsyncCallback>(), It.IsAny<object>())).Returns(innerAsyncResult);
                mockActionDescriptor.Setup(d => d.EndExecute(innerAsyncResult)).Returns(expectedResult);

                AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

                // Act
                IAsyncResult asyncResult = invoker.BeginInvokeActionMethod(controllerContext, mockActionDescriptor.Object, parameters, null, null);
                ActionResult returnedResult = invoker.EndInvokeActionMethod(asyncResult);

                // Assert
                Assert.Equal(expectedResult, returnedResult);
            }
        }

        [Fact]
        public void InvokeActionMethod_SynchronousDescriptor()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionResult expectedResult = new ViewResult();

            Mock<ActionDescriptor> mockActionDescriptor = new Mock<ActionDescriptor>();
            mockActionDescriptor.Setup(d => d.Execute(controllerContext, parameters)).Returns(expectedResult);

            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();

            // Act
            IAsyncResult asyncResult = invoker.BeginInvokeActionMethod(controllerContext, mockActionDescriptor.Object, parameters, null, null);
            ActionResult returnedResult = invoker.EndInvokeActionMethod(asyncResult);

            // Assert
            Assert.Equal(expectedResult, returnedResult);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_BeginExecuteThrowsOnActionExecutingException_Handled()
        {
            // Arrange
            ActionResult expectedResult = new ViewResult();
            Exception expectedException = new Exception("Some exception text.");
            bool onActionExecutingWasCalled = false;
            bool onActionExecutedWasCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = filterContext => { onActionExecutingWasCalled = true; },
                OnActionExecutedImpl = filterContext =>
                {
                    onActionExecutedWasCalled = true;
                    Assert.Same(expectedException, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };
            Func<IAsyncResult> beginExecute = delegate
            {
                throw expectedException;
            };

            // Act
            ActionResult result = BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, actionFilter);

            // Assert
            Assert.True(onActionExecutingWasCalled);
            Assert.True(onActionExecutedWasCalled);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_BeginExecuteThrowsOnActionExecutingException_HandledByEarlier()
        {
            // Arrange
            ActionResult expectedResult = new ViewResult();
            List<string> actionLog = new List<string>();
            Exception exception = new Exception("Some exception text.");
            Func<IAsyncResult> beginExecute = delegate
            {
                actionLog.Add("BeginExecute");
                throw exception;
            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    actionLog.Add("OnActionExecuted1");
                    Assert.Same(exception, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted2"); }
            };

            // Act
            ActionResult result = BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, filter1, filter2);

            // Assert
            Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "BeginExecute", "OnActionExecuted2", "OnActionExecuted1" }, actionLog.ToArray());
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_BeginExecuteThrowsOnActionExecutingException_HandledByLater()
        {
            // Arrange
            List<string> actionLog = new List<string>();
            Exception exception = new Exception("Some exception text.");
            ActionResult expectedResult = new ViewResult();
            Func<IAsyncResult> beginExecute = delegate
            {
                actionLog.Add("BeginExecute");
                throw exception;
            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    actionLog.Add("OnActionExecuted2");
                    Assert.Same(exception, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };

            // Act
            ActionResult result = BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, filter1, filter2);

            // Assert
            Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "BeginExecute", "OnActionExecuted2", "OnActionExecuted1" }, actionLog.ToArray());
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_BeginExecuteThrowsOnActionExecutingException_NotHandled()
        {
            // Arrange
            string expectedExceptionText = "Some exception text.";
            Exception expectedException = new Exception(expectedExceptionText);
            bool onActionExecutingWasCalled = false;
            bool onActionExecutedWasCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = filterContext => { onActionExecutingWasCalled = true; },
                OnActionExecutedImpl = filterContext => { onActionExecutedWasCalled = true; }
            };
            Func<IAsyncResult> beginExecute = delegate
            {
                throw expectedException;
            };

            // Act & assert
            Assert.Throws<Exception>(
                delegate
                {
                    BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, actionFilter);
                },
                expectedExceptionText);

            // Assert
            Assert.True(onActionExecutingWasCalled);
            Assert.True(onActionExecutedWasCalled);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_BeginExecuteThrowsOnActionExecutingException_ThreadAbort()
        {
            // Arrange
            bool onActionExecutingWasCalled = false;
            bool onActionExecutedWasCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = filterContext => { onActionExecutingWasCalled = true; },
                OnActionExecutedImpl = filterContext =>
                {
                    onActionExecutedWasCalled = true;
                    Thread.ResetAbort();
                }
            };
            Func<IAsyncResult> beginExecute = delegate
            {
                Thread.CurrentThread.Abort();
                return null;
            };

            // Act & assert
            Assert.Throws<ThreadAbortException>(
                delegate
                {
                    BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, actionFilter);
                });

            // Assert
            Assert.True(onActionExecutingWasCalled);
            Assert.True(onActionExecutedWasCalled);
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_EndExecuteThrowsOnActionExecutedException_Handled()
        {
            // Arrange
            ViewResult expectedResult = new ViewResult();
            Exception exepctedException = new Exception("Some exception message.");
            bool actionCalled = false;
            bool onActionExecutedCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutedImpl = (filterContext) =>
                {
                    onActionExecutedCalled = true;
                    Assert.Same(exepctedException, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };
            Func<ActionResult> action = () =>
            {
                actionCalled = true;
                throw exepctedException;
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersEndTester(mockResult, action, actionFilter);

                // Assert
                Assert.True(actionCalled);
                Assert.True(onActionExecutedCalled);
                Assert.Equal(expectedResult, result);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_EndExecuteThrowsOnActionExecutedException_HandledByEarlier()
        {
            // Arrange
            List<string> actionLog = new List<string>();
            Exception exception = new Exception("Some exception message.");
            ViewResult expectedResult = new ViewResult();
            Func<ActionResult> action = delegate
            {
                actionLog.Add("EndExecute");
                throw exception;

            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    actionLog.Add("OnActionExecuted1");
                    Assert.Same(exception, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted2"); }
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersEndTester(mockResult, action, filter1, filter2);

                // Assert
                Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "EndExecute", "OnActionExecuted2", "OnActionExecuted1" }, actionLog.ToArray());
                Assert.Equal(expectedResult, result);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_EndExecuteThrowsOnActionExecutedException_HandledByLater()
        {
            // Arrange
            List<string> actionLog = new List<string>();
            Exception exception = new Exception("Some exception message.");
            ActionResult expectedResult = new ViewResult();
            Func<ActionResult> action = delegate
            {
                actionLog.Add("EndExecute");
                throw exception;

            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    actionLog.Add("OnActionExecuted2");
                    Assert.Same(exception, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = expectedResult;
                }
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersEndTester(mockResult, action, filter1, filter2);

                // Assert
                Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "EndExecute", "OnActionExecuted2", "OnActionExecuted1" }, actionLog.ToArray());
                Assert.Equal(expectedResult, result);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_EndExecuteThrowsOnActionExecutedException_NotHandled()
        {
            // Arrange
            bool onActionExecutedWasCalled = false;
            string expectedExceptionText = "Some exception text.";
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutedImpl = filterContext => { onActionExecutedWasCalled = true; }
            };
            Func<ActionResult> action = delegate
            {
                throw new Exception(expectedExceptionText);
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act & assert
                Assert.Throws<Exception>(
                    () => BeginInvokeActionMethodWithFiltersEndTester(mockResult, action, actionFilter),
                    expectedExceptionText);

                // Assert
                Assert.True(onActionExecutedWasCalled);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_EndExecuteThrowsOnActionExecutedException_ThreadAbort()
        {
            // Arrange
            bool onActionExecutedWasCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutedImpl = filterContext =>
                {
                    onActionExecutedWasCalled = true;
                    Thread.ResetAbort();
                }
            };
            Func<ActionResult> action = delegate
            {
                Thread.CurrentThread.Abort();
                return null;
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act & assert
                Assert.Throws<ThreadAbortException>(
                    () => BeginInvokeActionMethodWithFiltersEndTester(mockResult, action, actionFilter));

                // Assert
                Assert.True(onActionExecutedWasCalled);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_NormalExecutionNotCanceled()
        {
            // Arrange
            bool onActionExecutingWasCalled = false;
            bool onActionExecutedWasCalled = false;
            using (MockAsyncResult innerAsyncResult = new MockAsyncResult())
            {
                ActionFilterImpl actionFilter = new ActionFilterImpl()
                {
                    OnActionExecutingImpl = _ => { onActionExecutingWasCalled = true; },
                    OnActionExecutedImpl = _ => { onActionExecutedWasCalled = true; }
                };
                Func<IAsyncResult> beginExecute = delegate
                {
                    return innerAsyncResult;
                };

                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersBeginTester(beginExecute, actionFilter);

                // Assert
                Assert.True(onActionExecutingWasCalled);
                Assert.True(onActionExecutedWasCalled);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_OnActionExecutingSetsResult()
        {
            // Arrange
            ActionResult expectedResult = new ViewResult();
            ActionResult overriddenResult = new ViewResult();
            bool onActionExecutingWasCalled = false;
            bool onActionExecutedWasCalled = false;
            ActionFilterImpl actionFilter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = filterContext =>
                {
                    onActionExecutingWasCalled = true;
                    filterContext.Result = expectedResult;
                },
                OnActionExecutedImpl = _ => { onActionExecutedWasCalled = true; }
            };
            Func<ActionResult> endExecute = delegate
            {
                return overriddenResult;
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersTester(() => mockResult, endExecute, checkBegin: false, checkEnd: false, filters: new IActionFilter[] { actionFilter });

                // Assert
                Assert.True(onActionExecutingWasCalled);
                Assert.False(onActionExecutedWasCalled);
                Assert.Equal(expectedResult, result);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_FiltersOrderedCorrectly()
        {
            // Arrange
            List<string> actionLog = new List<string>();
            ActionResult actionResult = new ViewResult();
            Func<ActionResult> continuation = delegate
            {
                actionLog.Add("Continuation");
                return actionResult;

            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted2"); }
            };

            using (var mockResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersEndTester(mockResult, continuation, filter1, filter2);

                // Assert
                Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "Continuation", "OnActionExecuted2", "OnActionExecuted1" }, actionLog.ToArray());
                Assert.Equal(actionResult, result);
            }
        }

        [Fact]
        public void BeginInvokeActionMethodWithFilters_ShortCircuited()
        {
            // Arrange
            List<string> actionLog = new List<string>();
            ActionResult shortCircuitResult = new ViewResult();
            ActionResult executeResult = new ViewResult();
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actionLog.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext)
                {
                    actionLog.Add("OnActionExecuting2");
                    filterContext.Result = shortCircuitResult;
                },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actionLog.Add("OnActionExecuted2"); }
            };
            Func<ActionResult> endExecute = () =>
            {
                actionLog.Add("ExecuteCalled");
                return executeResult;
            };

            using (var asyncResult = new MockAsyncResult())
            {
                // Act
                ActionResult result = BeginInvokeActionMethodWithFiltersTester(() => asyncResult, endExecute, checkBegin: false, checkEnd: false, filters: new IActionFilter[] { filter1, filter2 });

                // Assert
                Assert.Equal(new[] { "OnActionExecuting1", "OnActionExecuting2", "OnActionExecuted1" }, actionLog.ToArray());
                Assert.Equal(shortCircuitResult, result);
            }
        }

        private ActionResult BeginInvokeActionMethodWithFiltersBeginTester(Func<IAsyncResult> beginFunction, params IActionFilter[] filters)
        {
            return BeginInvokeActionMethodWithFiltersTester(beginFunction, () => new Mock<ActionResult>().Object, checkBegin: true, checkEnd: false, filters: filters);
        }

        private ActionResult BeginInvokeActionMethodWithFiltersEndTester(IAsyncResult asyncResult, Func<ActionResult> endFunction, params IActionFilter[] filters)
        {
            return BeginInvokeActionMethodWithFiltersTester(() => asyncResult, endFunction, checkBegin: true, checkEnd: true, filters: filters);
        }

        private ActionResult BeginInvokeActionMethodWithFiltersTester(Func<IAsyncResult> beginFunction, Func<ActionResult> endFunction, bool checkBegin, bool checkEnd, IActionFilter[] filters)
        {
            AsyncControllerActionInvoker invoker = new AsyncControllerActionInvoker();
            ControllerContext controllerContext = new ControllerContext();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            Mock<AsyncActionDescriptor> mockActionDescriptor = new Mock<AsyncActionDescriptor>();
            bool endExecuteCalled = false;
            bool beginExecuteCalled = false;
            Func<ActionResult> endExecute = () =>
            {
                endExecuteCalled = true;
                return endFunction();
            };
            Func<IAsyncResult> beingExecute = () =>
            {
                beginExecuteCalled = true;
                return beginFunction();
            };

            mockActionDescriptor.Setup(d => d.BeginExecute(controllerContext, parameters, It.IsAny<AsyncCallback>(), It.IsAny<object>())).Returns(beingExecute);
            mockActionDescriptor.Setup(d => d.EndExecute(It.IsAny<IAsyncResult>())).Returns(endExecute);

            IAsyncResult outerAsyncResult = null;
            try
            {
                outerAsyncResult = invoker.BeginInvokeActionMethodWithFilters(controllerContext, filters, mockActionDescriptor.Object, parameters, null, null);
            }
            catch (Exception ex)
            {
                if (checkEnd)
                {
                    // Testing end, so not expecting exception thrown from begin
                    Assert.NotNull(ex);
                }
                else
                {
                    throw ex;
                }
            }

            Assert.NotNull(outerAsyncResult);
            Assert.Equal(checkBegin, beginExecuteCalled);
            Assert.False(endExecuteCalled);

            ActionExecutedContext postContext = invoker.EndInvokeActionMethodWithFilters(outerAsyncResult);

            Assert.NotNull(postContext);
            if (checkEnd)
            {
                Assert.True(endExecuteCalled);
            }
            return postContext.Result;
        }

        private static ActionExecutingContext GetActionExecutingContext()
        {
            return new ActionExecutingContext(new ControllerContext(), new Mock<ActionDescriptor>().Object, new Dictionary<string, object>());
        }

        private static ControllerContext GetControllerContext(bool passesRequestValidation = true)
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            if (passesRequestValidation)
            {
#pragma warning disable 618
                mockHttpContext.Setup(o => o.Request.ValidateInput()).AtMostOnce();
#pragma warning restore 618
            }
            else
            {
                mockHttpContext.Setup(o => o.Request.ValidateInput()).Throws(new HttpRequestValidationException());
            }

            return new ControllerContext()
            {
                Controller = new TestController(),
                HttpContext = mockHttpContext.Object
            };
        }

        private class ActionFilterImpl : IActionFilter, IResultFilter
        {
            public Action<ActionExecutingContext> OnActionExecutingImpl { get; set; }

            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (OnActionExecutingImpl != null)
                {
                    OnActionExecutingImpl(filterContext);
                }
            }

            public Action<ActionExecutedContext> OnActionExecutedImpl { get; set; }

            public void OnActionExecuted(ActionExecutedContext filterContext)
            {
                if (OnActionExecutedImpl != null)
                {
                    OnActionExecutedImpl(filterContext);
                }
            }

            public Action<ResultExecutingContext> OnResultExecutingImpl { get; set; }

            public void OnResultExecuting(ResultExecutingContext filterContext)
            {
                if (OnResultExecutingImpl != null)
                {
                    OnResultExecutingImpl(filterContext);
                }
            }

            public Action<ResultExecutedContext> OnResultExecutedImpl { get; set; }

            public void OnResultExecuted(ResultExecutedContext filterContext)
            {
                if (OnResultExecutedImpl != null)
                {
                    OnResultExecutedImpl(filterContext);
                }
            }
        }

        public class AsyncControllerActionInvokerHelper : AsyncControllerActionInvoker
        {
            public AsyncControllerActionInvokerHelper()
            {
                DescriptorCache = new ControllerDescriptorCache();
            }

            protected override ControllerDescriptor GetControllerDescriptor(ControllerContext controllerContext)
            {
                return PublicGetControllerDescriptor(controllerContext);
            }

            public virtual ControllerDescriptor PublicGetControllerDescriptor(ControllerContext controllerContext)
            {
                return base.GetControllerDescriptor(controllerContext);
            }

            protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
            {
                return PublicInvokeExceptionFilters(controllerContext, filters, exception);
            }

            public virtual ExceptionContext PublicInvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
            {
                return base.InvokeExceptionFilters(controllerContext, filters, exception);
            }

            protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
            {
                PublicInvokeActionResult(controllerContext, actionResult);
            }

            public virtual void PublicInvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
            {
                base.InvokeActionResult(controllerContext, actionResult);
            }

            protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return PublicGetFilters(controllerContext, actionDescriptor);
            }

            public virtual FilterInfo PublicGetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return base.GetFilters(controllerContext, actionDescriptor);
            }

            public virtual AuthenticationContext PublicInvokeAuthenticationFilters(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return base.InvokeAuthenticationFilters(controllerContext, filters, actionDescriptor);
            }

            protected override AuthenticationContext InvokeAuthenticationFilters(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return PublicInvokeAuthenticationFilters(controllerContext, filters, actionDescriptor);
            }

            public virtual AuthenticationChallengeContext PublicInvokeAuthenticationFiltersChallenge(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor, ActionResult result)
            {
                return base.InvokeAuthenticationFiltersChallenge(controllerContext, filters, actionDescriptor, result);
            }

            protected override AuthenticationChallengeContext InvokeAuthenticationFiltersChallenge(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor, ActionResult result)
            {
                return PublicInvokeAuthenticationFiltersChallenge(controllerContext, filters, actionDescriptor, result);
            }

            protected override AuthorizationContext InvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return PublicInvokeAuthorizationFilters(controllerContext, filters, actionDescriptor);
            }

            public virtual AuthorizationContext PublicInvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return base.InvokeAuthorizationFilters(controllerContext, filters, actionDescriptor);
            }

            protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                return PublicFindAction(controllerContext, controllerDescriptor, actionName);
            }

            public virtual ActionDescriptor PublicFindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                return base.FindAction(controllerContext, controllerDescriptor, actionName);
            }

            protected override IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return PublicGetParameterValues(controllerContext, actionDescriptor);
            }

            public virtual IDictionary<string, object> PublicGetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return base.GetParameterValues(controllerContext, actionDescriptor);
            }
        }

        public class AsyncControllerActionInvokerWithCustomFindAction : AsyncControllerActionInvoker
        {
            protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                return base.FindAction(controllerContext, controllerDescriptor, "NormalAction");
            }
        }

        [ResetThreadAbort]
        private class TestController : AsyncController
        {
            public string Log;

            public ActionResult ActionCallsThreadAbortAsync()
            {
                Thread.CurrentThread.Abort();
                return null;
            }

            public ActionResult ActionCallsThreadAbortCompleted()
            {
                return null;
            }

            public ActionResult ResultCallsThreadAbort()
            {
                return new ActionResultWhichCallsThreadAbort();
            }

            public ActionResult NormalAction()
            {
                return new LoggingActionResult("From action");
            }

            [AuthenticationFilterReturnsResult]
            public void AuthenticationFilterShortCircuits()
            {
            }

            [AuthenticationFilterChallengeSetsResult]
            public void AuthenticationFilterChallenges()
            {
            }

            [AuthenticationFilterReturnsResult]
            [AuthenticationFilterChallengeSetsResult]
            public void AuthenticationFilterShortCircuitsAndChallenges()
            {
            }

            [AuthorizationFilterReturnsResult]
            public void AuthorizationFilterShortCircuits()
            {
            }

            [AuthenticationFilterChallengeSetsResult]
            [AuthorizationFilterReturnsResult]
            public void AuthorizationFilterShortCircuitsAndChallenges()
            {
            }

            [CustomExceptionFilterHandlesError]
            public void ActionThrowsExceptionAndIsHandledAsync()
            {
                throw new Exception("Some exception text.");
            }

            public void ActionThrowsExceptionAndIsHandledCompleted()
            {
            }

            [CustomExceptionFilterDoesNotHandleError]
            public void ActionThrowsExceptionAndIsNotHandledAsync()
            {
                throw new Exception("Some exception text.");
            }

            public void ActionThrowsExceptionAndIsNotHandledCompleted()
            {
            }

            [CustomExceptionFilterHandlesError]
            public ActionResult ResultThrowsExceptionAndIsHandled()
            {
                return new ActionResultWhichThrowsException();
            }

            [CustomExceptionFilterDoesNotHandleError]
            public ActionResult ResultThrowsExceptionAndIsNotHandled()
            {
                return new ActionResultWhichThrowsException();
            }

            private class AuthenticationFilterChallengeSetsResultAttribute : FilterAttribute, IAuthenticationFilter
            {
                public void OnAuthentication(AuthenticationContext filterContext)
                {
                }

                public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
                {
                    filterContext.Result = new LoggingActionResult("From authentication filter challenge");
                }
            }

            private class AuthenticationFilterReturnsResultAttribute : FilterAttribute, IAuthenticationFilter
            {
                public void OnAuthentication(AuthenticationContext filterContext)
                {
                    filterContext.Result = new LoggingActionResult("From authentication filter");
                }

                public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
                {
                }
            }

            private class AuthorizationFilterReturnsResultAttribute : FilterAttribute, IAuthorizationFilter
            {
                public void OnAuthorization(AuthorizationContext filterContext)
                {
                    filterContext.Result = new LoggingActionResult("From authorization filter");
                }
            }

            private class CustomExceptionFilterDoesNotHandleErrorAttribute : FilterAttribute, IExceptionFilter
            {
                public void OnException(ExceptionContext filterContext)
                {
                }
            }

            private class CustomExceptionFilterHandlesErrorAttribute : FilterAttribute, IExceptionFilter
            {
                public void OnException(ExceptionContext filterContext)
                {
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = new LoggingActionResult("From exception filter");
                }
            }

            private class ActionResultWhichCallsThreadAbort : ActionResult
            {
                public override void ExecuteResult(ControllerContext context)
                {
                    Thread.CurrentThread.Abort();
                }
            }

            private class ActionResultWhichThrowsException : ActionResult
            {
                public override void ExecuteResult(ControllerContext context)
                {
                    throw new Exception("Some exception text.");
                }
            }
        }

        private class ResetThreadAbortAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuted(ActionExecutedContext filterContext)
            {
                try
                {
                    Thread.ResetAbort();
                }
                catch (ThreadStateException)
                {
                    // thread wasn't being aborted
                }
            }

            public override void OnResultExecuted(ResultExecutedContext filterContext)
            {
                try
                {
                    Thread.ResetAbort();
                }
                catch (ThreadStateException)
                {
                    // thread wasn't being aborted
                }
            }
        }

        private class LoggingActionResult : ActionResult
        {
            private readonly string _logText;

            public LoggingActionResult(string logText)
            {
                _logText = logText;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                ((TestController)context.Controller).Log = _logText;
            }
        }
    }
}
