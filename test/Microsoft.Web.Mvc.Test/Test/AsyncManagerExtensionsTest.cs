// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Web.Mvc.Async;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class AsyncManagerExtensionsTest
    {
        [Fact]
        public void RegisterTask_AsynchronousCompletion()
        {
            // Arrange
            SimpleSynchronizationContext syncContext = new SimpleSynchronizationContext();
            AsyncManager asyncManager = new AsyncManager(syncContext);
            bool endDelegateWasCalled = false;

            using (ManualResetEvent waitHandle = new ManualResetEvent(false /* initialState */))
            {
                Func<AsyncCallback, IAsyncResult> beginDelegate = callback =>
                {
                    Assert.Equal(1, asyncManager.OutstandingOperations.Count);
                    MockAsyncResult asyncResult = new MockAsyncResult(false /* completedSynchronously */);
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Assert.Equal(1, asyncManager.OutstandingOperations.Count);
                        callback(asyncResult);
                        waitHandle.Set();
                    });
                    return asyncResult;
                };
                Action<IAsyncResult> endDelegate = delegate { endDelegateWasCalled = true; };

                // Act
                asyncManager.RegisterTask(beginDelegate, endDelegate);
                waitHandle.WaitOne();

                // Assert
                Assert.True(endDelegateWasCalled);
                Assert.True(syncContext.SendWasCalled);
                Assert.Equal(0, asyncManager.OutstandingOperations.Count);
            }
        }

        [Fact]
        public void RegisterTask_AsynchronousCompletion_SwallowsExceptionsThrownByEndDelegate()
        {
            // Arrange
            SimpleSynchronizationContext syncContext = new SimpleSynchronizationContext();
            AsyncManager asyncManager = new AsyncManager(syncContext);
            bool endDelegateWasCalled = false;

            using (ManualResetEvent waitHandle = new ManualResetEvent(false /* initialState */))
            {
                Func<AsyncCallback, IAsyncResult> beginDelegate = callback =>
                {
                    MockAsyncResult asyncResult = new MockAsyncResult(false /* completedSynchronously */);
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        callback(asyncResult);
                        waitHandle.Set();
                    });
                    return asyncResult;
                };
                Action<IAsyncResult> endDelegate = delegate
                {
                    endDelegateWasCalled = true;
                    throw new Exception("This is a sample exception.");
                };

                // Act
                asyncManager.RegisterTask(beginDelegate, endDelegate);
                waitHandle.WaitOne();

                // Assert
                Assert.True(endDelegateWasCalled);
                Assert.Equal(0, asyncManager.OutstandingOperations.Count);
            }
        }

        [Fact]
        public void RegisterTask_ResetsOutstandingOperationCountIfBeginMethodThrows()
        {
            // Arrange
            SimpleSynchronizationContext syncContext = new SimpleSynchronizationContext();
            AsyncManager asyncManager = new AsyncManager(syncContext);

            Func<AsyncCallback, IAsyncResult> beginDelegate = cb => { throw new InvalidOperationException("BeginDelegate throws."); };
            Action<IAsyncResult> endDelegate = ar => { Assert.True(false, "This should never be called."); };

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { asyncManager.RegisterTask(beginDelegate, endDelegate); }, "BeginDelegate throws.");

            Assert.Equal(0, asyncManager.OutstandingOperations.Count);
        }

        [Fact]
        public void RegisterTask_SynchronousCompletion()
        {
            // Arrange
            SimpleSynchronizationContext syncContext = new SimpleSynchronizationContext();
            AsyncManager asyncManager = new AsyncManager(syncContext);
            bool endDelegateWasCalled = false;

            Func<AsyncCallback, IAsyncResult> beginDelegate = callback =>
            {
                Assert.Equal(1, asyncManager.OutstandingOperations.Count);
                MockAsyncResult asyncResult = new MockAsyncResult(true /* completedSynchronously */);
                callback(asyncResult);
                return asyncResult;
            };
            Action<IAsyncResult> endDelegate = delegate { endDelegateWasCalled = true; };

            // Act
            asyncManager.RegisterTask(beginDelegate, endDelegate);

            // Assert
            Assert.True(endDelegateWasCalled);
            Assert.False(syncContext.SendWasCalled);
            Assert.Equal(0, asyncManager.OutstandingOperations.Count);
        }

        [Fact]
        public void RegisterTask_ThrowsIfAsyncManagerIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { AsyncManagerExtensions.RegisterTask(null, _ => null, _ => { }); }, "asyncManager");
        }

        [Fact]
        public void RegisterTask_ThrowsIfBeginDelegateIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new AsyncManager().RegisterTask(null, _ => { }); }, "beginDelegate");
        }

        [Fact]
        public void RegisterTask_ThrowsIfEndDelegateIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new AsyncManager().RegisterTask(_ => null, null); }, "endDelegate");
        }

        private class SimpleSynchronizationContext : SynchronizationContext
        {
            public bool SendWasCalled;

            public override void Send(SendOrPostCallback d, object state)
            {
                SendWasCalled = true;
                d(state);
            }
        }

        private class MockAsyncResult : IAsyncResult
        {
            private readonly bool _completedSynchronously;

            public MockAsyncResult(bool completedSynchronously)
            {
                _completedSynchronously = completedSynchronously;
            }

            public object AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously
            {
                get { return _completedSynchronously; }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
