// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// MSTest assert class to make assertions about tests using <see cref="Task"/>.
    /// </summary>
    public class TaskAssert
    {
        /// <summary>
        /// Asserts the given task has been started.  TAP guidelines are that all
        /// <see cref="Task"/> objects returned from public API's have been started.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to test.</param>
        public void IsStarted(Task task)
        {
            Assert.NotNull(task);
            Assert.True(task.Status != TaskStatus.Created);
        }

        /// <summary>
        /// Asserts the given task completes successfully.  This method will block the
        /// current thread waiting for the task, but will timeout if it does not complete.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to test.</param>
        public Task SucceedsAsync(Task task)
        {
            IsStarted(task);
            return task;
        }

        /// <summary>
        /// Asserts the given task completes successfully and returns a <typeparamref name="T"/> result.
        /// This method will block the current thread waiting for the task, but will timeout if it does not complete.
        /// </summary>
        /// <typeparam name="T">The result of the <see cref="Task"/>.</typeparam>
        /// <param name="task">The <see cref="Task"/> to test.</param>
        /// <returns>The result from that task.</returns>
        public Task<T> SucceedsWithResultAsync<T>(Task<T> task)
        {
            IsStarted(task);
            return task;
        }
    }
}
