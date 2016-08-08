// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncManagerTest
    {
        [Fact]
        public void FinishEvent_ExplicitCallToFinishMethod()
        {
            // Arrange
            AsyncManager helper = new AsyncManager();

            bool delegateCalled = false;
            helper.Finished += delegate { delegateCalled = true; };

            // Act
            helper.Finish();

            // Assert
            Assert.True(delegateCalled);
        }

        [Fact]
        public void FinishEvent_LinkedToOutstandingOperationsCompletedEvent()
        {
            // Arrange
            AsyncManager helper = new AsyncManager();

            bool delegateCalled = false;
            helper.Finished += delegate { delegateCalled = true; };

            // Act
            helper.OutstandingOperations.Increment();
            helper.OutstandingOperations.Decrement();

            // Assert
            Assert.True(delegateCalled);
        }

        [Fact]
        public void OutstandingOperationsProperty()
        {
            // Act
            AsyncManager helper = new AsyncManager();

            // Assert
            Assert.NotNull(helper.OutstandingOperations);
        }

        [Fact]
        public void ParametersProperty()
        {
            // Act
            AsyncManager helper = new AsyncManager();

            // Assert
            Assert.NotNull(helper.Parameters);
        }

        [Fact]
        public void Sync()
        {
            // Arrange
            Mock<SynchronizationContext> mockSyncContext = new Mock<SynchronizationContext>();
            mockSyncContext
                .Setup(c => c.Send(It.IsAny<SendOrPostCallback>(), null))
                .Callback(
                    delegate(SendOrPostCallback d, object state) { d(state); });

            AsyncManager helper = new AsyncManager(mockSyncContext.Object);
            bool wasCalled = false;

            // Act
            helper.Sync(() => { wasCalled = true; });

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public void TimeoutProperty()
        {
            // Arrange
            int setValue = 50;
            AsyncManager helper = new AsyncManager();

            // Act
            int defaultTimeout = helper.Timeout;
            helper.Timeout = setValue;
            int newTimeout = helper.Timeout;

            // Assert
            Assert.Equal(45000, defaultTimeout);
            Assert.Equal(setValue, newTimeout);
        }

        [Fact]
        public void TimeoutPropertyThrowsIfDurationIsOutOfRange()
        {
            // Arrange
            int timeout = -30;
            AsyncManager helper = new AsyncManager();

            // Act & assert
            Assert.ThrowsArgumentOutOfRange(
                delegate { helper.Timeout = timeout; }, "value",
                @"The timeout value must be non-negative or Timeout.Infinite.");
        }
    }
}
