// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Async.Test
{
    public class SimpleAsyncResultTest
    {
        [Fact]
        public void AsyncStateProperty()
        {
            // Arrange
            string expected = "Hello!";
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(expected);

            // Act
            object asyncState = asyncResult.AsyncState;

            // Assert
            Assert.Equal(expected, asyncState);
        }

        [Fact]
        public void AsyncWaitHandleProperty()
        {
            // Arrange
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(null);

            // Act
            WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle;

            // Assert
            Assert.Null(asyncWaitHandle);
        }

        [Fact]
        public void CompletedSynchronouslyProperty()
        {
            // Arrange
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(null);

            // Act
            bool completedSynchronously = asyncResult.CompletedSynchronously;

            // Assert
            Assert.False(completedSynchronously);
        }

        [Fact]
        public void IsCompletedProperty()
        {
            // Arrange
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(null);

            // Act
            bool isCompleted = asyncResult.IsCompleted;

            // Assert
            Assert.False(isCompleted);
        }

        [Fact]
        public void MarkCompleted_AsynchronousCompletion()
        {
            // Arrange
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(null);

            bool callbackWasCalled = false;
            AsyncCallback callback = ar =>
            {
                callbackWasCalled = true;
                Assert.Equal(asyncResult, ar);
                Assert.True(ar.IsCompleted);
                Assert.False(ar.CompletedSynchronously);
            };

            // Act & assert
            asyncResult.MarkCompleted(false, callback);
            Assert.True(callbackWasCalled);
        }

        [Fact]
        public void MarkCompleted_SynchronousCompletion()
        {
            // Arrange
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(null);

            bool callbackWasCalled = false;
            AsyncCallback callback = ar =>
            {
                callbackWasCalled = true;
                Assert.Equal(asyncResult, ar);
                Assert.True(ar.IsCompleted);
                Assert.True(ar.CompletedSynchronously);
            };

            // Act & assert
            asyncResult.MarkCompleted(true, callback);
            Assert.True(callbackWasCalled);
        }
    }
}
