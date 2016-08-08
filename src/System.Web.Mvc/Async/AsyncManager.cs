// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;

namespace System.Web.Mvc.Async
{
    public class AsyncManager
    {
        private readonly SynchronizationContext _syncContext;

        /// <summary>
        /// default timeout is 45 sec
        /// </summary>
        /// <remarks>
        /// from: http://msdn.microsoft.com/en-us/library/system.web.ui.page.asynctimeout.aspx
        /// </remarks>
        private int _timeout = 45 * 1000;

        public AsyncManager()
            : this(null /* syncContext */)
        {
        }

        public AsyncManager(SynchronizationContext syncContext)
        {
            _syncContext = syncContext ?? SynchronizationContextUtil.GetSynchronizationContext();

            OutstandingOperations = new OperationCounter();
            OutstandingOperations.Completed += delegate
            {
                Finish();
            };

            Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public event EventHandler Finished;

        public OperationCounter OutstandingOperations { get; private set; }

        public IDictionary<string, object> Parameters { get; private set; }

        /// <summary>
        /// Measured in milliseconds, Timeout.Infinite means 'no timeout'
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value < -1)
                {
                    throw Error.AsyncCommon_InvalidTimeout("value");
                }
                _timeout = value;
            }
        }

        /// <summary>
        /// The developer may call this function to signal that all operations are complete instead of
        /// waiting for the operation counter to reach zero.
        /// </summary>
        public virtual void Finish()
        {
            EventHandler handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Executes a callback in the current synchronization context, which gives access to HttpContext and related items.
        /// </summary>
        /// <param name="action"></param>
        public virtual void Sync(Action action)
        {
            _syncContext.Sync(action);
        }
    }
}
