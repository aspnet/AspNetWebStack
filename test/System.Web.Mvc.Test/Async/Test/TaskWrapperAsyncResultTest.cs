// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Async.Test
{
    public class TaskWrapperAsyncResultTest
    {
        [Fact]
        public void PropertiesHaveCorrectValues()
        {
            // Arrange
            Mock<MyTask> mockTask = new Mock<MyTask>();
            WaitHandle waitHandle = new Mock<WaitHandle>().Object;

            mockTask.Setup(o => o.AsyncState).Returns(10);
            mockTask.Setup(o => o.AsyncWaitHandle).Returns(waitHandle);
            mockTask.Setup(o => o.CompletedSynchronously).Returns(true);
            mockTask.Setup(o => o.IsCompleted).Returns(true);

            // Act
            TaskWrapperAsyncResult taskWrapper = new TaskWrapperAsyncResult(mockTask.Object, asyncState: 20);

            // Assert
            Assert.Equal(20, taskWrapper.AsyncState);
            Assert.Equal(waitHandle, taskWrapper.AsyncWaitHandle);
            Assert.True(taskWrapper.CompletedSynchronously);
            Assert.True(taskWrapper.IsCompleted);
            Assert.Equal(mockTask.Object, taskWrapper.Task);
        }

        // Assists in mocking a Task by passing a dummy action to the Task constructor [which defers execution]
        public class MyTask : Task, IAsyncResult
        {
            public MyTask()
                : base(() => { })
            {
            }

            public new virtual object AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            public virtual WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public virtual bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public new virtual bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
