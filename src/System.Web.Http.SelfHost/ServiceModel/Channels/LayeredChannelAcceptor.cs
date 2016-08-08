// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal abstract class LayeredChannelAcceptor<TChannel, TInnerChannel> : ChannelAcceptor<TChannel>
        where TChannel : class, IChannel
        where TInnerChannel : class, IChannel
    {
        private IChannelListener<TInnerChannel> _innerListener;

        protected LayeredChannelAcceptor(ChannelManagerBase channelManager, IChannelListener<TInnerChannel> innerListener)
            : base(channelManager)
        {
            _innerListener = innerListener;
        }

        public override TChannel AcceptChannel(TimeSpan timeout)
        {
            TInnerChannel innerChannel = _innerListener.AcceptChannel(timeout);
            if (innerChannel == null)
            {
                return null;
            }
            else
            {
                return OnAcceptChannel(innerChannel);
            }
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerListener.BeginAcceptChannel(timeout, callback, state);
        }

        public override TChannel EndAcceptChannel(IAsyncResult result)
        {
            TInnerChannel innerChannel = _innerListener.EndAcceptChannel(result);
            if (innerChannel == null)
            {
                return null;
            }
            else
            {
                return OnAcceptChannel(innerChannel);
            }
        }

        public override bool WaitForChannel(TimeSpan timeout)
        {
            return _innerListener.WaitForChannel(timeout);
        }

        public override IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerListener.BeginWaitForChannel(timeout, callback, state);
        }

        public override bool EndWaitForChannel(IAsyncResult result)
        {
            return _innerListener.EndWaitForChannel(result);
        }

        protected abstract TChannel OnAcceptChannel(TInnerChannel innerChannel);
    }
}
