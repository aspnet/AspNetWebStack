// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Web.Mvc.Async
{
    // This class is used for the following pattern:

    // public IAsyncResult BeginInner(..., callback, state);
    // public TInnerResult EndInner(asyncResult);
    // public IAsyncResult BeginOuter(..., callback, state);
    // public TOuterResult EndOuter(asyncResult);

    // That is, Begin/EndOuter() wrap Begin/EndInner(), potentially with pre- and post-processing.

    [DebuggerNonUserCode]
    internal static class AsyncResultWrapper
    {
        // helper methods

        private static readonly EndInvokeDelegate<Action, AsyncVoid> _voidEndInvoke = (IAsyncResult asyncResult, Action action) =>
        {
            action();
            return default(AsyncVoid);
        };

        // kicks off an asynchronous operation

        public static IAsyncResult Begin<TResult>(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag = null, int timeout = Timeout.Infinite)
        {
            WrappedAsyncResult<TResult> asyncResult = new WrappedAsyncResult<TResult>(beginDelegate, endDelegate, tag, callbackSyncContext: null);
            asyncResult.Begin(callback, state, timeout);
            return asyncResult;
        }

        public static IAsyncResult Begin<TResult, TState>(AsyncCallback callback, object callbackState, BeginInvokeDelegate<TState> beginDelegate, EndInvokeDelegate<TState, TResult> endDelegate, TState invokeState, object tag = null, int timeout = Timeout.Infinite, SynchronizationContext callbackSyncContext = null)
        {
            WrappedAsyncResult<TResult, TState> asyncResult = new WrappedAsyncResult<TResult, TState>(beginDelegate, endDelegate, invokeState, tag, callbackSyncContext);
            asyncResult.Begin(callback, callbackState, timeout);
            return asyncResult;
        }

        public static IAsyncResult Begin<TState>(AsyncCallback callback, object callbackState, BeginInvokeDelegate<TState> beginDelegate, EndInvokeVoidDelegate<TState> endDelegate, TState invokeState, object tag = null, int timeout = Timeout.Infinite, SynchronizationContext callbackSyncContext = null)
        {
            WrappedAsyncVoid<TState> asyncResult = new WrappedAsyncVoid<TState>(beginDelegate, endDelegate, invokeState, tag, callbackSyncContext);
            asyncResult.Begin(callback, callbackState, timeout);
            return asyncResult;
        }

        // wraps a synchronous operation in an asynchronous wrapper, but still completes synchronously

        public static IAsyncResult BeginSynchronous<TResult, TState>(AsyncCallback callback, object callbackState, EndInvokeDelegate<TState, TResult> func, TState funcState, object tag)
        {
            // Frequently called, so use static delegates

            // Inline delegates that take a generic argument from a generic method don't get cached by the compiler so use a field from a static generic class
            BeginInvokeDelegate<TState> beginDelegate = CachedDelegates<TState>.CompletedBeginInvoke;

            // Pass in the blocking function as the End() method
            WrappedAsyncResult<TResult, TState> asyncResult = new WrappedAsyncResult<TResult, TState>(beginDelegate, func, funcState, tag, callbackSyncContext: null);
            asyncResult.Begin(callback, callbackState, Timeout.Infinite);
            return asyncResult;
        }

        public static IAsyncResult BeginSynchronous(AsyncCallback callback, object state, Action action, object tag)
        {
            return BeginSynchronous<AsyncVoid, Action>(callback, state, _voidEndInvoke, action, tag);
        }

        // completes an asynchronous operation

        public static TResult End<TResult>(IAsyncResult asyncResult)
        {
            return End<TResult>(asyncResult, tag: null);
        }

        public static TResult End<TResult>(IAsyncResult asyncResult, object tag)
        {
            return WrappedAsyncResultBase<TResult>.Cast(asyncResult, tag).End();
        }

        public static void End(IAsyncResult asyncResult)
        {
            End(asyncResult, tag: null);
        }

        public static void End(IAsyncResult asyncResult, object tag)
        {
            End<AsyncVoid>(asyncResult, tag);
        }

        private static class CachedDelegates<TState>
        {
            internal static BeginInvokeDelegate<TState> CompletedBeginInvoke = (AsyncCallback asyncCallback, object asyncState, TState invokeState) =>
            {
                // Begin() doesn't perform any work on its own and returns immediately.
                SimpleAsyncResult innerAsyncResult = new SimpleAsyncResult(asyncState);
                innerAsyncResult.MarkCompleted(completedSynchronously: true, callback: asyncCallback);
                return innerAsyncResult;
            };
        }

        [DebuggerNonUserCode]
        [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The Timer will be disposed of either when it fires or when the operation completes successfully.")]
        private abstract class WrappedAsyncResultBase<TResult> : IAsyncResult
        {
            private const int AsyncStateNone = 0;
            private const int AsyncStateBeginUnwound = 1;
            private const int AsyncStateCallbackFired = 2;

            private int _asyncState;
            private readonly object _beginDelegateLockObj = new object();
            private readonly SingleEntryGate _endExecutedGate = new SingleEntryGate(); // prevent End() from being called twice
            private readonly SingleEntryGate _handleCallbackGate = new SingleEntryGate(); // prevent callback from being handled multiple times
            private readonly object _tag; // prevent an instance of this type from being passed to the wrong End() method
            private IAsyncResult _innerAsyncResult;
            private AsyncCallback _originalCallback;
            private volatile bool _timedOut;
            private Timer _timer;
            private readonly SynchronizationContext _callbackSyncContext;

            protected WrappedAsyncResultBase(object tag, SynchronizationContext callbackSyncContext)
            {
                _tag = tag;
                _callbackSyncContext = callbackSyncContext;
            }

            public object AsyncState
            {
                get { return _innerAsyncResult.AsyncState; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return null; }
            }

            public bool CompletedSynchronously { get; private set; }

            public bool IsCompleted
            {
                get { return _timedOut || _innerAsyncResult.IsCompleted; }
            }

            // kicks off the process, instantiates a timer if requested
            public void Begin(AsyncCallback callback, object state, int timeout)
            {
                _originalCallback = callback;

                // Force the target Begin() operation to complete before the callback can continue,
                // since the target operation might perform post-processing of the data.
                lock (_beginDelegateLockObj)
                {
                    _innerAsyncResult = CallBeginDelegate(HandleAsynchronousCompletion, state);

                    // If the callback has already fired, then the completion routine has no-oped and we
                    // can just treat this as if it were a normal synchronous completion.
                    int originalState = Interlocked.Exchange(ref _asyncState, AsyncStateBeginUnwound);
                    bool callbackAlreadyFired = (originalState == AsyncStateCallbackFired);

                    CompletedSynchronously = callbackAlreadyFired || _innerAsyncResult.CompletedSynchronously;

                    if (!CompletedSynchronously)
                    {
                        if (timeout > Timeout.Infinite)
                        {
                            CreateTimer(timeout);
                        }
                    }
                }

                if (CompletedSynchronously)
                {
                    if (callback != null)
                    {
                        callback(this);
                    }
                }
            }

            protected abstract IAsyncResult CallBeginDelegate(AsyncCallback callback, object callbackState);

            protected abstract TResult CallEndDelegate(IAsyncResult asyncResult);

            public static WrappedAsyncResultBase<TResult> Cast(IAsyncResult asyncResult, object tag)
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }

                WrappedAsyncResultBase<TResult> castResult = asyncResult as WrappedAsyncResultBase<TResult>;
                if (castResult != null && Equals(castResult._tag, tag))
                {
                    return castResult;
                }
                else
                {
                    throw Error.AsyncCommon_InvalidAsyncResult("asyncResult");
                }
            }

            private void CallbackUsingSyncContext()
            {
                _callbackSyncContext.Sync(() => _originalCallback(this));
            }

            private void CreateTimer(int timeout)
            {
                // this method should be called within a lock(_beginDelegateLockObj)
                _timer = new Timer(HandleTimeout, null, timeout, Timeout.Infinite /* disable periodic signaling */);
            }

            public TResult End()
            {
                if (!_endExecutedGate.TryEnter())
                {
                    throw Error.AsyncCommon_AsyncResultAlreadyConsumed();
                }

                if (_timedOut)
                {
                    throw new TimeoutException();
                }
                WaitForBeginToCompleteAndDestroyTimer();

                return CallEndDelegate(_innerAsyncResult);
            }

            private void ExecuteAsynchronousCallback(bool timedOut)
            {
                WaitForBeginToCompleteAndDestroyTimer();

                if (_handleCallbackGate.TryEnter())
                {
                    _timedOut = timedOut;
                    if (_originalCallback != null)
                    {
                        if (_callbackSyncContext != null)
                        {
                            CallbackUsingSyncContext();
                        }
                        else
                        {
                            _originalCallback(this);
                        }
                    }
                }
            }

            private void HandleAsynchronousCompletion(IAsyncResult asyncResult)
            {
                // Transition the async state to CALLBACK_FIRED. If the Begin* method hasn't yet unwound,
                // then we can no-op here since the Begin method will query the _asyncState field and
                // treat this as a regular synchronous completion.
                int originalState = Interlocked.Exchange(ref _asyncState, AsyncStateCallbackFired);
                if (originalState != AsyncStateBeginUnwound)
                {
                    return;
                }

                ExecuteAsynchronousCallback(timedOut: false);
            }

            private void HandleTimeout(object state)
            {
                ExecuteAsynchronousCallback(timedOut: true);
            }

            private void WaitForBeginToCompleteAndDestroyTimer()
            {
                lock (_beginDelegateLockObj)
                {
                    // Wait for the target Begin() method to complete, as it might be performing
                    // post-processing. This also forces a memory barrier, so _innerAsyncResult
                    // is guaranteed to be non-null at this point.

                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }
                    _timer = null;
                }
            }
        }

        private sealed class WrappedAsyncResult<TResult> : WrappedAsyncResultBase<TResult>
        {
            private readonly BeginInvokeDelegate _beginDelegate;
            private readonly EndInvokeDelegate<TResult> _endDelegate;

            public WrappedAsyncResult(BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag, SynchronizationContext callbackSyncContext)
                : base(tag, callbackSyncContext)
            {
                _beginDelegate = beginDelegate;
                _endDelegate = endDelegate;
            }

            protected override IAsyncResult CallBeginDelegate(AsyncCallback callback, object callbackState)
            {
                return _beginDelegate(callback, callbackState);
            }

            protected override TResult CallEndDelegate(IAsyncResult asyncResult)
            {
                return _endDelegate(asyncResult);
            }
        }

        private sealed class WrappedAsyncResult<TResult, TState> : WrappedAsyncResultBase<TResult>
        {
            private readonly BeginInvokeDelegate<TState> _beginDelegate;
            private readonly EndInvokeDelegate<TState, TResult> _endDelegate;
            private readonly TState _state;

            public WrappedAsyncResult(BeginInvokeDelegate<TState> beginDelegate, EndInvokeDelegate<TState, TResult> endDelegate, TState state, object tag, SynchronizationContext callbackSyncContext)
                : base(tag, callbackSyncContext)
            {
                _beginDelegate = beginDelegate;
                _endDelegate = endDelegate;
                _state = state;
            }

            protected override TResult CallEndDelegate(IAsyncResult asyncResult)
            {
                return _endDelegate(asyncResult, _state);
            }

            protected override IAsyncResult CallBeginDelegate(AsyncCallback callback, object callbackState)
            {
                return _beginDelegate(callback, callbackState, _state);
            }
        }

        private sealed class WrappedAsyncVoid<TState> : WrappedAsyncResultBase<AsyncVoid>
        {
            private readonly BeginInvokeDelegate<TState> _beginDelegate;
            private readonly EndInvokeVoidDelegate<TState> _endDelegate;
            private readonly TState _state;

            public WrappedAsyncVoid(BeginInvokeDelegate<TState> beginDelegate, EndInvokeVoidDelegate<TState> endDelegate, TState state, object tag, SynchronizationContext callbackSyncContext)
                : base(tag, callbackSyncContext)
            {
                _beginDelegate = beginDelegate;
                _endDelegate = endDelegate;
                _state = state;
            }

            protected override AsyncVoid CallEndDelegate(IAsyncResult asyncResult)
            {
                _endDelegate(asyncResult, _state);
                return default(AsyncVoid);
            }

            protected override IAsyncResult CallBeginDelegate(AsyncCallback callback, object callbackState)
            {
                return _beginDelegate(callback, callbackState, _state);
            }
        }
    }
}
