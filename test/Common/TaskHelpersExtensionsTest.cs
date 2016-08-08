// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Threading.Tasks
{
    public class TaskHelpersExtensionsTest
    {
        // ----------------------------------------------------------------
        //   Task<object> Task<T>.CastToObject()

        [Fact, ForceGC]
        public Task ConvertFromTaskOfStringShouldSucceed()
        {
            // Arrange
            return Task.FromResult("StringResult")

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal("StringResult", (string)task.Result);
                });
        }

        [Fact, ForceGC]
        public Task ConvertFromTaskOfIntShouldSucceed()
        {
            // Arrange
            return Task.FromResult(123)

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal(123, (int)task.Result);
                });
        }

        [Fact, ForceGC]
        public Task ConvertFromFaultedTaskOfObjectShouldBeHandled()
        {
            // Arrange
            return TaskHelpers.FromError<object>(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.Faulted, task.Status);
                    Assert.IsType<InvalidOperationException>(task.Exception.GetBaseException());
                });
        }

        [Fact, ForceGC]
        public Task ConvertFromCancelledTaskOfStringShouldBeHandled()
        {
            // Arrange
            return TaskHelpers.Canceled<string>()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.Canceled, task.Status);
                });
        }

        // ----------------------------------------------------------------
        //   Task<object> Task.CastToObject()

        [Fact, ForceGC]
        public Task ConvertFromTaskShouldSucceed()
        {
            // Arrange
            return TaskHelpers.Completed()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                    Assert.Equal(null, task.Result);
                });
        }

        [Fact, ForceGC]
        public Task ConvertFromFaultedTaskShouldBeHandled()
        {
            // Arrange
            return TaskHelpers.FromError(new InvalidOperationException())

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.Faulted, task.Status);
                    Assert.IsType<InvalidOperationException>(task.Exception.GetBaseException());
                });
        }

        [Fact, ForceGC]
        public Task ConvertFromCancelledTaskShouldBeHandled()
        {
            // Arrange
            return TaskHelpers.Canceled()

            // Act
                .CastToObject()

            // Assert
                .ContinueWith((task) =>
                {
                    Assert.Equal(TaskStatus.Canceled, task.Status);
                });
        }

        // -----------------------------------------------------------------
        //  bool Task.TryGetResult(Task<TResult>, out TResult)

        [Fact, ForceGC]
        public void TryGetResult_CompleteTask_ReturnsTrueAndGivesResult()
        {
            // Arrange
            var task = Task.FromResult(42);

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.True(result);
            Assert.Equal(42, value);
        }

        [Fact, ForceGC]
        public void TryGetResult_FaultedTask_ReturnsFalse()
        {
            // Arrange
            var task = TaskHelpers.FromError<int>(new Exception());

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.False(result);
            var ex = task.Exception; // Observe the task exception
        }

        [Fact, ForceGC]
        public void TryGetResult_CanceledTask_ReturnsFalse()
        {
            // Arrange
            var task = TaskHelpers.Canceled<int>();

            // Act
            int value;
            bool result = task.TryGetResult(out value);

            // Assert
            Assert.False(result);
        }

        [Fact, ForceGC]
        public Task TryGetResult_IncompleteTask_ReturnsFalse()
        {
            // Arrange
            var incompleteTask = new Task<int>(() => 42);

            // Act
            int value;
            bool result = incompleteTask.TryGetResult(out value);

            // Assert
            Assert.False(result);

            incompleteTask.Start();
            return incompleteTask;  // Make sure the task gets observed
        }
    }
}
