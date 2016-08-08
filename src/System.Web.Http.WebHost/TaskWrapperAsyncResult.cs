// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Wraps a <see cref="Task"/>, optionally overriding the State object (since the Task Asynchronous Pattern doesn't normally use it).
    /// </summary>
    /// <remarks>Class copied from System.Web.Mvc, but with modifications</remarks>
    internal sealed class TaskWrapperAsyncResult : IAsyncResult
    {
        private bool? _completedSynchronously;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskWrapperAsyncResult"/> class.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to wrap.</param>
        /// <param name="asyncState">User-defined object that qualifies or contains information about an asynchronous operation.</param>
        public TaskWrapperAsyncResult(Task task, object asyncState)
        {
            if (task == null)
            {
                throw Error.ArgumentNull("task");
            }

            Task = task;
            AsyncState = asyncState;
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState { get; private set; }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get { return ((IAsyncResult)Task).AsyncWaitHandle; }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get { return _completedSynchronously ?? ((IAsyncResult)Task).CompletedSynchronously; }
            internal set { _completedSynchronously = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get { return ((IAsyncResult)Task).IsCompleted; }
        }

        /// <summary>
        /// Gets the task.
        /// </summary>
        public Task Task { get; private set; }
    }
}
