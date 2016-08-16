// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Threading.Tasks
{
    public class TaskHelpersExtensionsTest
    {
        // ----------------------------------------------------------------
        //   Task<object> Task<T>.CastToObject()

        [Fact, ForceGC]
        public async Task ConvertFromTaskOfStringShouldSucceed()
        {
            // Arrange
            var task = Task.FromResult("StringResult")
                .CastToObject();

            // Act
            var result = await task;

            // Assert
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal("StringResult", (string)result);
        }

        [Fact, ForceGC]
        public async Task ConvertFromTaskOfIntShouldSucceed()
        {
            // Arrange
            var task = Task.FromResult(123)
                .CastToObject();

            // Act
            var result = await task;

            // Assert
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(123, (int)result);
        }

        [Fact, ForceGC]
        public async Task ConvertFromFaultedTaskOfObjectShouldBeHandled()
        {
            // Arrange
            var task = TaskHelpers.FromError<object>(new InvalidOperationException())
                .CastToObject();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }

        [Fact, ForceGC]
        public async Task ConvertFromCancelledTaskOfStringShouldBeHandled()
        {
            // Arrange
            var task = TaskHelpers.Canceled<string>()
                .CastToObject();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        // ----------------------------------------------------------------
        //   Task<object> Task.CastToObject()

        [Fact, ForceGC]
        public async Task ConvertFromTaskShouldSucceed()
        {
            // Arrange
            var task = TaskHelpers.Completed()
                .CastToObject();

            // Act
            var result = await task;

            // Assert
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Null(result);
        }

        [Fact, ForceGC]
        public async Task ConvertFromFaultedTaskShouldBeHandled()
        {
            // Arrange
            var task = TaskHelpers.FromError(new InvalidOperationException())
                .CastToObject();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }

        [Fact, ForceGC]
        public async Task ConvertFromCancelledTaskShouldBeHandled()
        {
            // Arrange
            var task = TaskHelpers.Canceled()
                .CastToObject();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            Assert.Equal(TaskStatus.Canceled, task.Status);
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
