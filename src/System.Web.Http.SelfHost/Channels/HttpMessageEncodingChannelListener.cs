// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncodingChannelListener : LayeredChannelListener<IReplyChannel>
    {
        private IChannelListener<IReplyChannel> _innerChannelListener;

        public HttpMessageEncodingChannelListener(Binding binding, IChannelListener<IReplyChannel> innerListener) :
            base(binding, innerListener)
        {
        }

        protected override void OnOpening()
        {
            _innerChannelListener = (IChannelListener<IReplyChannel>)InnerChannelListener;
            base.OnOpening();
        }

        protected override IReplyChannel OnAcceptChannel(TimeSpan timeout)
        {
            IReplyChannel innerChannel = _innerChannelListener.AcceptChannel(timeout);
            return WrapInnerChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IReplyChannel OnEndAcceptChannel(IAsyncResult result)
        {
            IReplyChannel innerChannel = _innerChannelListener.EndAcceptChannel(result);
            return WrapInnerChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return _innerChannelListener.EndWaitForChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return _innerChannelListener.WaitForChannel(timeout);
        }

        private IReplyChannel WrapInnerChannel(IReplyChannel innerChannel)
        {
            return (innerChannel != null)
                       ? new HttpMessageEncodingReplyChannel(this, innerChannel)
                       : (IReplyChannel)null;
        }
    }
}
