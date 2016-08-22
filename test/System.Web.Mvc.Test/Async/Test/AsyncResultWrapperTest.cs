// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncResultWrapperTest
    {
        [Fact]
        public void Begin_AsynchronousCompletion()
        {
            // Arrange
            AsyncCallback capturedCallback = null;
            IAsyncResult resultGivenToCallback = null;
            using (MockAsyncResult innerResult = new MockAsyncResult())
            {
                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                    ar => { resultGivenToCallback = ar; },
                    null,
                    (callback, callbackState, state) =>
                    {
                        capturedCallback = callback;
                        return innerResult;
                    },
                    (ar, state) => { },
                    null);

                capturedCallback(innerResult);

                // Assert
                Assert.Equal(outerResult, resultGivenToCallback);
            }
        }

        [Fact]
        public void Begin_AsynchronousCompletionWithState()
        {
            // Arrange
            using (MockAsyncResult innerResult = new MockAsyncResult())
            {
                object invokeState = new object();
                object capturedBeginState = null;
                object capturedEndState = null;

                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin(
                    null,
                    null,
                    (AsyncCallback callback, object callbackState, object innerInvokeState) =>
                    {
                        capturedBeginState = innerInvokeState;
                        return innerResult;
                    },
                    (IAsyncResult result, object innerInvokeState) =>
                    {
                        capturedEndState = innerInvokeState;
                    },
                    invokeState,
                    null,
                    Timeout.Infinite);
                AsyncResultWrapper.End(outerResult);

                // Assert
                Assert.Same(invokeState, capturedBeginState);
                Assert.Same(invokeState, capturedEndState);
            }
        }

        [Fact]
        public void Begin_AsynchronousCompletionWithStateAndResult()
        {
            // Arrange
            using (MockAsyncResult innerResult = new MockAsyncResult())
            {
                object invokeState = new object();
                object capturedBeginState = null;
                object capturedEndState = null;
                object expectedRetun = new object();

                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin(
                    null,
                    null,
                    (AsyncCallback callback, object callbackState, object innerInvokeState) =>
                    {
                        capturedBeginState = innerInvokeState;
                        return innerResult;
                    },
                    (IAsyncResult result, object innerInvokeState) =>
                    {
                        capturedEndState = innerInvokeState;
                        return expectedRetun;
                    },
                    invokeState,
                    null,
                    Timeout.Infinite);
                object endResult = AsyncResultWrapper.End<object>(outerResult);

                // Assert
                Assert.Same(expectedRetun, endResult);
                Assert.Same(invokeState, capturedBeginState);
                Assert.Same(invokeState, capturedEndState);
            }
        }

        [Fact]
        public void Begin_ReturnsAsyncResultWhichWrapsInnerResult()
        {
            // Arrange
            MockAsyncResult innerResult = new MockAsyncResult()
            {
                AsyncState = "inner state",
                CompletedSynchronously = true,
                IsCompleted = true
            };

            using (innerResult)
            {
                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                    null, "outer state",
                    (callback, callbackState, state) => innerResult,
                    (ar, state) => { },
                    null);

                // Assert
                Assert.Equal(innerResult.AsyncState, outerResult.AsyncState);
                Assert.Null(outerResult.AsyncWaitHandle);
                Assert.Equal(innerResult.CompletedSynchronously, outerResult.CompletedSynchronously);
                Assert.Equal(innerResult.IsCompleted, outerResult.IsCompleted);
            }
        }

        [Fact]
        public void Begin_SynchronousCompletion()
        {
            // Arrange
            IAsyncResult resultGivenToCallback = null;
            using (MockAsyncResult innerResult = new MockAsyncResult())
            {
                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                    ar => { resultGivenToCallback = ar; },
                    null,
                    (callback, callbackState, state) =>
                    {
                        callback(innerResult);
                        return innerResult;
                    },
                    (ar, state) => { },
                    null);

                // Assert
                Assert.Equal(outerResult, resultGivenToCallback);
            }
        }

        [Fact]
        public void Begin_AsynchronousButAlreadyCompleted()
        {
            // Arrange
            Mock<IAsyncResult> innerResultMock = new Mock<IAsyncResult>();
            innerResultMock.Setup(ir => ir.CompletedSynchronously).Returns(false);
            innerResultMock.Setup(ir => ir.IsCompleted).Returns(true);

            // Act
            IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                null,
                null,
                (callback, callbackState, state) =>
                {
                    callback(innerResultMock.Object);
                    return innerResultMock.Object;
                },
                (ar, state) => { },
                null);

            // Assert
            Assert.True(outerResult.CompletedSynchronously);
        }

        [Fact]
        public void Begin_WithCallbackSyncContext_CallsSendIfOperationCompletedAsynchronously()
        {
            // Arrange
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = false,
                IsCompleted = false
            };

            using (asyncResult)
            {
                bool originalCallbackCalled = false;
                IAsyncResult passedAsyncResult = null;
                AsyncCallback passedCallback = null;
                AsyncCallback originalCallback = ar =>
                {
                    originalCallbackCalled = true;
                    passedAsyncResult = ar;
                };
                object originalState = new object();
                DummySynchronizationContext syncContext = new DummySynchronizationContext();

                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                     originalCallback,
                     originalState,
                     (callback, callbackState, state) =>
                     {
                         passedCallback = callback;
                         asyncResult.AsyncState = callbackState;
                         return asyncResult;
                     },
                     (ar, state) =>
                     {
                         asyncResult.IsCompleted = true;
                         passedCallback(ar);
                     },
                     null,
                     callbackSyncContext: syncContext);
                AsyncResultWrapper.End(outerResult);

                // Assert
                Assert.True(originalCallbackCalled);
                Assert.False(passedAsyncResult.CompletedSynchronously);
                Assert.True(passedAsyncResult.IsCompleted);
                Assert.Same(originalState, passedAsyncResult.AsyncState);
                Assert.True(syncContext.SendCalled);
            }
        }

        [Fact]
        public void Begin_WithCallbackSyncContext_DoesNotCallSendIfOperationCompletedSynchronously()
        {
            // Arrange
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = true,
                IsCompleted = true
            };

            using (asyncResult)
            {
                bool originalCallbackCalled = false;
                IAsyncResult passedAsyncResult = null;
                AsyncCallback originalCallback = ar =>
                {
                    passedAsyncResult = ar;
                    originalCallbackCalled = true;
                };
                object originalState = new object();

                DummySynchronizationContext syncContext = new DummySynchronizationContext();

                // Act
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                    originalCallback,
                    originalState,
                    (callback, callbackState, state) =>
                    {
                        asyncResult.AsyncState = callbackState;
                        return asyncResult;
                    },
                    (ar, state) => { },
                    null,
                    callbackSyncContext: syncContext);

                // Assert
                Assert.True(originalCallbackCalled);
                Assert.True(passedAsyncResult.CompletedSynchronously);
                Assert.True(passedAsyncResult.IsCompleted);
                Assert.Same(originalState, passedAsyncResult.AsyncState);
                Assert.False(syncContext.SendCalled);
            }
        }

        [Fact]
        public void Begin_WithCallbackSyncContext_ThrowsAsyncEvenIfSendContextCaptures()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("Some exception text.");
            CapturingSynchronizationContext capturingSyncContext = new CapturingSynchronizationContext();
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = false,
                IsCompleted = true
            };

            using (asyncResult)
            {
                bool originalCallbackCalled = false;
                IAsyncResult passedAsyncResult = null;
                AsyncCallback passedCallback = null;
                AsyncCallback originalCallback = ar =>
                {
                    passedAsyncResult = ar;
                    originalCallbackCalled = true;
                    throw exception;
                };

                // Act & Assert
                IAsyncResult outerResult = AsyncResultWrapper.Begin<object>(
                     originalCallback,
                     null,
                     (callback, callbackState, state) =>
                     {
                         passedCallback = callback;
                         asyncResult.AsyncState = callbackState;
                         return asyncResult;
                     },
                     (ar, state) =>
                     {
                         asyncResult.IsCompleted = true;
                         passedCallback(ar);
                     },
                     null,
                     callbackSyncContext: capturingSyncContext);
                SynchronousOperationException thrownException = Assert.Throws<SynchronousOperationException>(
                    delegate
                    {
                        AsyncResultWrapper.End(outerResult);
                    },
                    @"An operation that crossed a synchronization context failed. See the inner exception for more information.");

                // Assert
                Assert.Equal(exception, thrownException.InnerException);
                Assert.True(originalCallbackCalled);
                Assert.False(passedAsyncResult.CompletedSynchronously);
                Assert.True(capturingSyncContext.SendCalled);
            }
        }

        [Fact]
        public void Begin_WithCallbackSyncContext_ThrowsSynchronous()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("Some exception text.");
            CapturingSynchronizationContext capturingSyncContext = new CapturingSynchronizationContext();
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = true,
                IsCompleted = true
            };

            using (asyncResult)
            {
                bool originalCallbackCalled = false;
                IAsyncResult passedAsyncResult = null;
                AsyncCallback originalCallback = ar =>
                {
                    passedAsyncResult = ar;
                    originalCallbackCalled = true;
                    throw exception;
                };

                // Act & Assert
                InvalidOperationException thrownException = Assert.Throws<InvalidOperationException>(
                    delegate
                    {
                        AsyncResultWrapper.Begin<object>(
                            originalCallback,
                            null,
                            (callback, callbackState, state) =>
                            {
                                asyncResult.AsyncState = callbackState;
                                return asyncResult;
                            },
                            (ar, state) => { },
                            null,
                            callbackSyncContext: capturingSyncContext);
                    },
                    exception.Message);

                // Assert
                Assert.Equal(exception, thrownException);
                Assert.True(originalCallbackCalled);
                Assert.True(passedAsyncResult.CompletedSynchronously);
                Assert.False(capturingSyncContext.SendCalled);
            }
        }

        [Fact]
        public void BeginSynchronous_Action()
        {
            // Arrange
            bool actionCalled = false;

            // Act
            IAsyncResult asyncResult = AsyncResultWrapper.BeginSynchronous(callback: null, state: null, action: delegate { actionCalled = true; }, tag: null);
            AsyncResultWrapper.End(asyncResult);

            // Assert
            Assert.True(actionCalled);
            Assert.True(asyncResult.IsCompleted);
            Assert.True(asyncResult.CompletedSynchronously);
        }

        [Fact]
        public void BeginSynchronous_Func()
        {
            object expectedReturn = new object();
            object expectedState = new object();
            object expectedCallbackState = new object();
            bool funcCalled = false;
            bool asyncCalledbackCalled = false;

            // Act
            IAsyncResult asyncResult = AsyncResultWrapper.BeginSynchronous(
                callback: (innerIAsyncResult) =>
                {
                    asyncCalledbackCalled = true;
                    Assert.NotNull(innerIAsyncResult);
                    Assert.Same(expectedCallbackState, innerIAsyncResult.AsyncState);
                    Assert.True(innerIAsyncResult.IsCompleted);
                    Assert.True(innerIAsyncResult.CompletedSynchronously);
                },
                callbackState: expectedCallbackState,
                func: (innerIAsyncResult, innerState) =>
                {
                    funcCalled = true;
                    Assert.NotNull(innerIAsyncResult);
                    Assert.Same(expectedCallbackState, innerIAsyncResult.AsyncState);
                    Assert.Same(expectedState, innerState);
                    Assert.True(innerIAsyncResult.IsCompleted);
                    Assert.True(innerIAsyncResult.CompletedSynchronously);
                    return expectedReturn;
                },
                funcState: expectedState,
                tag: null);
            object retVal = AsyncResultWrapper.End<object>(asyncResult);

            // Assert
            Assert.Same(expectedReturn, retVal);
            Assert.True(asyncResult.IsCompleted);
            Assert.True(asyncResult.CompletedSynchronously);
            Assert.True(funcCalled);
            Assert.True(asyncCalledbackCalled);
        }

        [Fact]
        public void End_ExecutesStoredDelegateAndReturnsValue()
        {
            // Arrange
            using (var mockResult = new MockAsyncResult())
            {
                IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                    null, null,
                    (callback, state) => mockResult,
                    ar => 42);

                // Act
                int returned = AsyncResultWrapper.End<int>(asyncResult);

                // Assert
                Assert.Equal(42, returned);
            }
        }

        [Fact]
        public void End_ThrowsIfAsyncResultIsIncorrectType()
        {
            // Arrange
            using (var mockResult = new MockAsyncResult())
            {
                IAsyncResult asyncResult = AsyncResultWrapper.Begin<object>(
                    null, null,
                    (callback, callbackState, state) => mockResult,
                    (ar, state) => { },
                    null);

                // Act & assert
                Assert.Throws<ArgumentException>(
                    delegate { AsyncResultWrapper.End<int>(asyncResult); },
                    "The provided IAsyncResult is not valid for this method." + Environment.NewLine
                  + "Parameter name: asyncResult");
            }
        }

        [Fact]
        public void End_ThrowsIfAsyncResultIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { AsyncResultWrapper.End(null); }, "asyncResult");
        }

        [Fact]
        public void End_ThrowsIfAsyncResultTagMismatch()
        {
            // Arrange
            using (var mockResult = new MockAsyncResult())
            {
                IAsyncResult asyncResult = AsyncResultWrapper.Begin<object>(
                    null, null,
                    (callback, callbackState, state) => mockResult,
                    (ar, state) => { },
                    null,
                    "some tag");

                // Act & assert
                Assert.Throws<ArgumentException>(
                    delegate { AsyncResultWrapper.End(asyncResult, "some other tag"); },
                    "The provided IAsyncResult is not valid for this method." + Environment.NewLine
                  + "Parameter name: asyncResult");
            }
        }

        [Fact]
        public void End_ThrowsIfCalledTwiceOnSameAsyncResult()
        {
            // Arrange
            using (var mockResult = new MockAsyncResult())
            {
                IAsyncResult asyncResult = AsyncResultWrapper.Begin<object>(
                null, null,
                (callback, callbackState, state) => mockResult,
                (ar, state) => { },
                null);

                // Act & assert
                AsyncResultWrapper.End(asyncResult);
                Assert.Throws<InvalidOperationException>(
                    delegate { AsyncResultWrapper.End(asyncResult); },
                    "The provided IAsyncResult has already been consumed.");
            }
        }

        [Fact]
        public void TimedOut()
        {
            // Arrange
            using (ManualResetEvent waitHandle = new ManualResetEvent(false /* initialState */))
            {
                AsyncCallback callback = ar => { waitHandle.Set(); };

                // Act & assert
                using (var mockResult = new MockAsyncResult())
                {
                    IAsyncResult asyncResult = AsyncResultWrapper.Begin<object>(
                    callback, null,
                    (innerCallback, callbackState, state) => mockResult,
                    (ar, state) => { Assert.True(false, "This callback should never execute since we timed out."); },
                    null,
                    null, 0);

                    // wait for the timeout
                    waitHandle.WaitOne();

                    Assert.True(asyncResult.IsCompleted);
                    Assert.Throws<TimeoutException>(
                        delegate { AsyncResultWrapper.End(asyncResult); });
                }
            }
        }

        private class DummySynchronizationContext : SynchronizationContext
        {
            public bool SendCalled { get; private set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                SendCalled = true;
                base.Send(d, state);
            }
        }

        private class CapturingSynchronizationContext : SynchronizationContext
        {
            public bool SendCalled { get; private set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                try
                {
                    SendCalled = true;
                    d(state);
                }
                catch
                {
                    // swallow exceptions, just like AspNetSynchronizationContext
                }
            }
        }
    }
}
