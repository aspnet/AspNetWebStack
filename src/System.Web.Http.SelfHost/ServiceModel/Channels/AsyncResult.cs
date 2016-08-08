// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    // AsyncResult starts acquired; Complete releases.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Ported from WCF")]
    internal abstract class AsyncResult : IAsyncResult
    {
        private static AsyncCallback _asyncCompletionWrapperCallback;
        private readonly AsyncCallback _completionCallback;
        private bool _completedSynchronously;
        private bool _endCalled;
        private Exception _exception;
        private bool _isCompleted;
        private AsyncCompletion _nextAsyncCompletion;
        private readonly object _state;
        private Action _beforePrepareAsyncCompletionAction;
        private Func<IAsyncResult, bool> _checkSyncValidationFunc;
        private ManualResetEvent _manualResetEvent;
        private readonly object _manualResetEventLock = new object();

        protected AsyncResult(AsyncCallback callback, object state)
        {
            _completionCallback = callback;
            _state = state;
        }

        /// <summary>
        /// Can be utilized by subclasses to write core completion code for both the sync and async paths
        /// in one location, signalling chainable synchronous completion with the boolean result,
        /// and leveraging PrepareAsyncCompletion for conversion to an AsyncCallback.
        /// </summary>
        /// <remarks>NOTE: requires that "this" is passed in as the state object to the asynchronous sub-call being used with a completion routine.</remarks>
        protected delegate bool AsyncCompletion(IAsyncResult result);

        public object AsyncState
        {
            get { return _state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (_manualResetEventLock)
                {
                    if (_manualResetEvent == null)
                    {
                        _manualResetEvent = new ManualResetEvent(_isCompleted);
                    }
                }

                return _manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
        }

        public bool HasCallback
        {
            get { return _completionCallback != null; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        // used in conjunction with PrepareAsyncCompletion to allow for finally blocks
        protected Action<AsyncResult, Exception> OnCompleting { get; set; }

        // subclasses like TraceAsyncResult can use this to wrap the callback functionality in a scope
        protected Action<AsyncCallback, IAsyncResult> VirtualCallback { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated or FailFast")]
        protected void Complete(bool didCompleteSynchronously)
        {
            if (_isCompleted)
            {
                throw Error.InvalidOperation(SRResources.AsyncResultCompletedTwice, GetType());
            }

            _completedSynchronously = didCompleteSynchronously;
            if (OnCompleting != null)
            {
                // Allow exception replacement, like a catch/throw pattern.
                try
                {
                    OnCompleting(this, _exception);
                }
                catch (Exception e)
                {
                    _exception = e;
                }
            }

            if (didCompleteSynchronously)
            {
                // If we completedSynchronously, then there's no chance that the manualResetEvent was created so
                // we don't need to worry about a race
                Contract.Assert(_manualResetEvent == null, "No ManualResetEvent should be created for a synchronous AsyncResult.");
                _isCompleted = true;
            }
            else
            {
                lock (_manualResetEventLock)
                {
                    _isCompleted = true;
                    if (_manualResetEvent != null)
                    {
                        _manualResetEvent.Set();
                    }
                }
            }

            if (_completionCallback != null)
            {
                try
                {
                    if (VirtualCallback != null)
                    {
                        VirtualCallback(_completionCallback, this);
                    }
                    else
                    {
                        _completionCallback(this);
                    }
                }
#pragma warning disable 1634
#pragma warning suppress 56500 // transferring exception to another thread
                catch (Exception e)
                {
                    // String is not resourced because it occurs only in debug build, only with a fatal assert,
                    // and because it appears as an unused resource in a release build
                    Contract.Assert(false, Error.Format("{0}{1}{2}", "Async Callback threw an exception.", Environment.NewLine, e.ToString()));
                }
#pragma warning restore 1634
            }
        }

        protected void Complete(bool didCompleteSynchronously, Exception error)
        {
            _exception = error;
            Complete(didCompleteSynchronously);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated")]
        private static void AsyncCompletionWrapperCallback(IAsyncResult result)
        {
            if (result == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidNullAsyncResult);
            }
            if (result.CompletedSynchronously)
            {
                return;
            }

            AsyncResult thisPtr = (AsyncResult)result.AsyncState;
            if (!thisPtr.OnContinueAsyncCompletion(result))
            {
                return;
            }

            AsyncCompletion callback = thisPtr.GetNextCompletion();
            if (callback == null)
            {
                ThrowInvalidAsyncResult(result);
            }

            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                completeSelf = callback(result);
            }
            catch (Exception e)
            {
                completeSelf = true;
                completionException = e;
            }

            if (completeSelf)
            {
                thisPtr.Complete(false, completionException);
            }
        }

        // Note: this should be only derived by the TransactedAsyncResult
        protected virtual bool OnContinueAsyncCompletion(IAsyncResult result)
        {
            return true;
        }

        // Note: this should be used only by the TransactedAsyncResult
        protected void SetBeforePrepareAsyncCompletionAction(Action completionAction)
        {
            _beforePrepareAsyncCompletionAction = completionAction;
        }

        // Note: this should be used only by the TransactedAsyncResult
        protected void SetCheckSyncValidationFunc(Func<IAsyncResult, bool> validationFunc)
        {
            _checkSyncValidationFunc = validationFunc;
        }

        protected AsyncCallback PrepareAsyncCompletion(AsyncCompletion callback)
        {
            if (_beforePrepareAsyncCompletionAction != null)
            {
                _beforePrepareAsyncCompletionAction();
            }

            _nextAsyncCompletion = callback;
            if (AsyncResult._asyncCompletionWrapperCallback == null)
            {
                AsyncResult._asyncCompletionWrapperCallback = new AsyncCallback(AsyncCompletionWrapperCallback);
            }
            return AsyncResult._asyncCompletionWrapperCallback;
        }

        protected bool CheckSyncContinue(IAsyncResult result)
        {
            AsyncCompletion dummy;
            return TryContinueHelper(result, out dummy);
        }

        protected bool SyncContinue(IAsyncResult result)
        {
            AsyncCompletion callback;
            if (TryContinueHelper(result, out callback))
            {
                return callback(result);
            }
            else
            {
                return false;
            }
        }

        private bool TryContinueHelper(IAsyncResult result, out AsyncCompletion callback)
        {
            if (result == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidNullAsyncResult);
            }

            callback = null;
            if (_checkSyncValidationFunc != null)
            {
                if (!_checkSyncValidationFunc(result))
                {
                    return false;
                }
            }
            else if (!result.CompletedSynchronously)
            {
                return false;
            }

            callback = GetNextCompletion();
            if (callback == null)
            {
                ThrowInvalidAsyncResult("Only call Check/SyncContinue once per async operation (once per PrepareAsyncCompletion).");
            }
            return true;
        }

        private AsyncCompletion GetNextCompletion()
        {
            AsyncCompletion result = _nextAsyncCompletion;
            _nextAsyncCompletion = null;
            return result;
        }

        protected static void ThrowInvalidAsyncResult(IAsyncResult result)
        {
            if (result == null)
            {
                throw Error.ArgumentNull("result");
            }

            throw Error.InvalidOperation(SRResources.InvalidAsyncResultImplementation, result.GetType());
        }

        protected static void ThrowInvalidAsyncResult(string debugText)
        {
            string message = SRResources.InvalidAsyncResultImplementationGeneric;
            if (debugText != null)
            {
#if DEBUG
                message += " " + debugText;
#endif
            }
            throw Error.InvalidOperation(message);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Existing API")]
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
            where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw Error.ArgumentNull("result");
            }

            TAsyncResult asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw Error.Argument("result", SRResources.InvalidAsyncResult);
            }

            if (asyncResult._endCalled)
            {
                throw Error.InvalidOperation(SRResources.AsyncResultAlreadyEnded);
            }

            asyncResult._endCalled = true;

            if (!asyncResult._isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult._manualResetEvent != null)
            {
                asyncResult._manualResetEvent.Close();
            }

            if (asyncResult._exception != null)
            {
                throw asyncResult._exception;
            }

            return asyncResult;
        }
    }
}
