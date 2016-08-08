// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class SynchronizationContextUtilTest
    {
        [Fact]
        public void SyncWithAction()
        {
            // Arrange
            bool actionWasCalled = false;
            bool sendWasCalled = false;

            Mock<SynchronizationContext> mockSyncContext = new Mock<SynchronizationContext>();
            mockSyncContext
                .Setup(sc => sc.Send(It.IsAny<SendOrPostCallback>(), null))
                .Callback(
                    delegate(SendOrPostCallback d, object state)
                    {
                        sendWasCalled = true;
                        d(state);
                    });

            // Act
            SynchronizationContextUtil.Sync(mockSyncContext.Object, () => { actionWasCalled = true; });

            // Assert
            Assert.True(actionWasCalled);
            Assert.True(sendWasCalled);
        }

        [Fact]
        public void SyncWithActionCapturesException()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("Some exception text.");

            Mock<SynchronizationContext> mockSyncContext = new Mock<SynchronizationContext>();
            mockSyncContext
                .Setup(sc => sc.Send(It.IsAny<SendOrPostCallback>(), null))
                .Callback(
                    delegate(SendOrPostCallback d, object state)
                    {
                        try
                        {
                            d(state);
                        }
                        catch
                        {
                            // swallow exceptions, just like AspNetSynchronizationContext
                        }
                    });

            // Act & assert
            SynchronousOperationException thrownException = Assert.Throws<SynchronousOperationException>(
                delegate { SynchronizationContextUtil.Sync(mockSyncContext.Object, () => { throw exception; }); },
                @"An operation that crossed a synchronization context failed. See the inner exception for more information.");

            Assert.Equal(exception, thrownException.InnerException);
        }

        [Fact]
        public void SyncWithFunc()
        {
            // Arrange
            bool sendWasCalled = false;

            Mock<SynchronizationContext> mockSyncContext = new Mock<SynchronizationContext>();
            mockSyncContext
                .Setup(sc => sc.Send(It.IsAny<SendOrPostCallback>(), null))
                .Callback(
                    delegate(SendOrPostCallback d, object state)
                    {
                        sendWasCalled = true;
                        d(state);
                    });

            // Act
            int retVal = SynchronizationContextUtil.Sync(mockSyncContext.Object, () => 42);

            // Assert
            Assert.Equal(42, retVal);
            Assert.True(sendWasCalled);
        }
    }
}
