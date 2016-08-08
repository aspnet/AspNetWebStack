// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal abstract class LayeredChannelListener<TChannel> : ChannelListenerBase<TChannel>
        where TChannel : class, IChannel
    {
        private IChannelListener _innerChannelListener;
        private bool _sharedInnerListener;
        private EventHandler _onInnerListenerFaulted;

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : base(timeouts)
        {
            _sharedInnerListener = sharedInnerListener;
            _innerChannelListener = innerChannelListener;
            _onInnerListenerFaulted = new EventHandler(OnInnerListenerFaulted);
            if (_innerChannelListener != null)
            {
                _innerChannelListener.Faulted += _onInnerListenerFaulted;
            }
        }

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts)
            : this(sharedInnerListener, timeouts, null)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener)
            : this(sharedInnerListener, null, null)
        {
        }

        protected LayeredChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : this(false, timeouts, innerChannelListener)
        {
        }

        public override Uri Uri
        {
            get { return GetInnerListenerSnapshot().Uri; }
        }

        internal virtual IChannelListener InnerChannelListener
        {
            get { return _innerChannelListener; }

            set
            {
                lock (ThisLock)
                {
                    ThrowIfDisposedOrImmutable();
                    if (_innerChannelListener != null)
                    {
                        _innerChannelListener.Faulted -= _onInnerListenerFaulted;
                    }

                    _innerChannelListener = value;
                    if (_innerChannelListener != null)
                    {
                        _innerChannelListener.Faulted += _onInnerListenerFaulted;
                    }
                }
            }
        }

        internal bool SharedInnerListener
        {
            get { return _sharedInnerListener; }
        }

        public override T GetProperty<T>()
        {
            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            IChannelListener channelListener = InnerChannelListener;
            if (channelListener != null)
            {
                return channelListener.GetProperty<T>();
            }
            else
            {
                return default(T);
            }
        }

        internal void ThrowIfInnerListenerNotSet()
        {
            if (InnerChannelListener == null)
            {
                throw Error.InvalidOperation(SRResources.InnerListenerFactoryNotSet, GetType().ToString());
            }
        }

        internal IChannelListener GetInnerListenerSnapshot()
        {
            IChannelListener innerListener = InnerChannelListener;

            if (innerListener == null)
            {
                throw Error.InvalidOperation(SRResources.InnerListenerFactoryNotSet, GetType().ToString());
            }

            return innerListener;
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            ThrowIfInnerListenerNotSet();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (InnerChannelListener != null && !_sharedInnerListener)
            {
                InnerChannelListener.Open(timeout);
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(InnerChannelListener, _sharedInnerListener, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseOrAbort();
            if (InnerChannelListener != null && !_sharedInnerListener)
            {
                InnerChannelListener.Close(timeout);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseOrAbort();
            return new CloseAsyncResult(InnerChannelListener, _sharedInnerListener, timeout, callback, state);
        }

        protected override void OnAbort()
        {
            lock (ThisLock)
            {
                OnCloseOrAbort();
            }

            IChannelListener channelListener = InnerChannelListener;
            if (channelListener != null && !_sharedInnerListener)
            {
                channelListener.Abort();
            }
        }

        private void OnInnerListenerFaulted(object sender, EventArgs e)
        {
            // if our inner listener faulted, we should fault as well
            Fault();
        }

        private void OnCloseOrAbort()
        {
            IChannelListener channelListener = InnerChannelListener;
            if (channelListener != null)
            {
                channelListener.Faulted -= _onInnerListenerFaulted;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private static AsyncCallback _onCloseComplete = new AsyncCallback(OnCloseComplete);

            private ICommunicationObject _communicationObject;

            public CloseAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                _communicationObject = communicationObject;

                if (_communicationObject == null || sharedInnerListener)
                {
                    Complete(true);
                    return;
                }

                IAsyncResult result = _communicationObject.BeginClose(timeout, _onCloseComplete, this);

                if (result.CompletedSynchronously)
                {
                    _communicationObject.EndClose(result);
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
            private static void OnCloseComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr._communicationObject.EndClose(result);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private static AsyncCallback _onOpenComplete = new AsyncCallback(OnOpenComplete);

            private ICommunicationObject _communicationObject;

            public OpenAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                _communicationObject = communicationObject;

                if (_communicationObject == null || sharedInnerListener)
                {
                    Complete(true);
                    return;
                }

                IAsyncResult result = _communicationObject.BeginOpen(timeout, _onOpenComplete, this);
                if (result.CompletedSynchronously)
                {
                    _communicationObject.EndOpen(result);
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
            private static void OnOpenComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr._communicationObject.EndOpen(result);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }
        }
    }
}
