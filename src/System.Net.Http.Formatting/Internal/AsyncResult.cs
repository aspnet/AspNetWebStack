// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Http;

namespace System.Net.Http.Internal
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "_manualResetEvent is disposed in End<TAsyncResult>")]
    internal abstract class AsyncResult : IAsyncResult
    {
        private AsyncCallback _callback;
        private object _state;

        private bool _isCompleted;
        private bool _completedSynchronously;
        private bool _endCalled;

        private Exception _exception;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
        }

        public object AsyncState
        {
            get { return _state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                Contract.Assert(false, "AsyncWaitHandle is not supported -- use callbacks instead.");
                return null;
            }
        }

        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
        }

        public bool HasCallback
        {
            get { return _callback != null; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        protected void Complete(bool completedSynchronously)
        {
            if (_isCompleted)
            {
                throw Error.InvalidOperation(Properties.Resources.AsyncResult_MultipleCompletes, GetType().Name);
            }

            _completedSynchronously = completedSynchronously;
            _isCompleted = true;

            if (_callback != null)
            {
                try
                {
                    _callback(this);
                }
                catch (Exception e)
                {
                    throw Error.InvalidOperation(e, Properties.Resources.AsyncResult_CallbackThrewException);
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;
            Complete(completedSynchronously);
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw Error.ArgumentNull("result");
            }

            TAsyncResult thisPtr = result as TAsyncResult;

            if (thisPtr == null)
            {
                throw Error.Argument("result", Properties.Resources.AsyncResult_ResultMismatch);
            }

            if (!thisPtr._isCompleted)
            {
                thisPtr.AsyncWaitHandle.WaitOne();
            }

            if (thisPtr._endCalled)
            {
                throw Error.InvalidOperation(Properties.Resources.AsyncResult_MultipleEnds);
            }

            thisPtr._endCalled = true;

            if (thisPtr._exception != null)
            {
                throw thisPtr._exception;
            }

            return thisPtr;
        }
    }
}
