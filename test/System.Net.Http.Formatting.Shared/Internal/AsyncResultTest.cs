// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Internal;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class AsyncResultTest
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);

        [Fact]
        public void Constructor_Initializes()
        {
            // Arrange
            AsyncCallback callback = new AsyncCallback(_ => { });
            object state = new object();

            // Act
            MockAsyncResult mockAsyncResult = new MockAsyncResult(callback, state);

            // Assert
            Assert.True(mockAsyncResult.HasCallback);
            Assert.False(mockAsyncResult.IsCompleted);
            Assert.False(mockAsyncResult.CompletedSynchronously);
            Assert.Same(state, mockAsyncResult.AsyncState);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_SetsCompletedSynchronously(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.Equal(completedSynchronously, mockAsyncResult.CompletedSynchronously);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_SetsIsCompleted(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.True(mockAsyncResult.IsCompleted);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_CallsCallback(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.True(mockCallback.WasInvoked);
            Assert.Same(mockAsyncResult, mockCallback.AsyncResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_ThrowsOnMultipleCompletes(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.Throws<InvalidOperationException>(() => mockAsyncResult.Complete(completedSynchronously));
        }

        [Fact]
        public void Complete_ThrowsIfCallbackThrows()
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(true);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => mockAsyncResult.Complete(false));
        }

        [Fact]
        public void End_ThrowsOnNullAsyncResult()
        {
            Assert.ThrowsArgumentNull(() => MockAsyncResult.End<MockAsyncResult>(null), "result");
        }

        [Fact]
        public void End_ThrowsOnInvalidAsyncResult()
        {
            Mock<IAsyncResult> mockIAsyncResult = new Mock<IAsyncResult>();
            Assert.ThrowsArgument(() => MockAsyncResult.End<MockAsyncResult>(mockIAsyncResult.Object), "result");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void End_ThrowsIfCalledTwiceOnSameAsyncResult(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);
            mockAsyncResult.Complete(completedSynchronously);

            // Act
            MockAsyncResult.End<MockAsyncResult>(mockAsyncResult);

            // Act
            Assert.Throws<InvalidOperationException>(() => MockAsyncResult.End<MockAsyncResult>(mockAsyncResult));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void End_ThrowsIfCompletedWithException(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);
#if NETSTANDARD1_3
            Exception applicationException = new RankException("Complete failed!");
#else
            Exception applicationException = new ApplicationException("Complete failed!");
#endif
            mockAsyncResult.Complete(completedSynchronously, applicationException);

            // Act/Assert
#if NETSTANDARD1_3
            Assert.Throws<RankException>(
#else
            Assert.Throws<ApplicationException>(
#endif
                () => MockAsyncResult.End<MockAsyncResult>(mockAsyncResult));
        }

        internal class MockAsyncResult : AsyncResult
        {
            public MockAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public new void Complete(bool completedSynchronously)
            {
                base.Complete(completedSynchronously);
            }

            public new void Complete(bool completedSynchronously, Exception e)
            {
                base.Complete(completedSynchronously, e);
            }

            public static new TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
            {
                return AsyncResult.End<TAsyncResult>(result);
            }
        }

        public class MockAsyncCallback
        {
            private bool _throwInCallback;

            public MockAsyncCallback(bool throwInCallback)
            {
                _throwInCallback = throwInCallback;
            }

            public bool WasInvoked { get; private set; }

            public IAsyncResult AsyncResult { get; private set; }

            public Exception CallbackException { get; private set; }

            public void Callback(IAsyncResult result)
            {
                WasInvoked = true;
                AsyncResult = result;
                if (_throwInCallback)
                {
                    CallbackException = new Exception("Callback exception");
                    throw CallbackException;
                }
            }
        }
    }
}
