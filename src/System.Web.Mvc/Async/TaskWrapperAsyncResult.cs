// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Mvc.Async
{
    /// <summary>
    /// Wraps a <see cref="Task"/> class, optionally overriding the State object (since the Task Asynchronous Pattern doesn't normally use it).
    /// Copied from System.Web.
    /// </summary>
    internal sealed class TaskWrapperAsyncResult : IAsyncResult
    {
        private bool? _completedSynchronously;

        internal TaskWrapperAsyncResult(Task task, object asyncState, Action cleanupThunk = null)
        {
            Task = task;
            AsyncState = asyncState;
            CleanupThunk = cleanupThunk;
        }

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { return ((IAsyncResult)Task).AsyncWaitHandle; }
        }

        /// <summary>
        /// Cleanup logic to run after Task is finished
        /// </summary>
        public Action CleanupThunk { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get { return _completedSynchronously ?? ((IAsyncResult)Task).CompletedSynchronously; }
            internal set { _completedSynchronously = value; }
        }

        public bool IsCompleted
        {
            get { return ((IAsyncResult)Task).IsCompleted; }
        }

        internal Task Task { get; private set; }
    }
}
