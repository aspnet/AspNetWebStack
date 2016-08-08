// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal abstract class LayeredChannel<TInnerChannel> : ChannelBase
        where TInnerChannel : class, IChannel
    {
        private TInnerChannel _innerChannel;
        private EventHandler _onInnerChannelFaulted;

        protected LayeredChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel)
            : base(channelManager)
        {
            Contract.Assert(innerChannel != null);

            _innerChannel = innerChannel;
            _onInnerChannelFaulted = new EventHandler(OnInnerChannelFaulted);
            _innerChannel.Faulted += _onInnerChannelFaulted;
        }

        protected TInnerChannel InnerChannel
        {
            get { return _innerChannel; }
        }

        public override T GetProperty<T>()
        {
            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return InnerChannel.GetProperty<T>();
        }

        protected override void OnClosing()
        {
            _innerChannel.Faulted -= _onInnerChannelFaulted;
            base.OnClosing();
        }

        protected override void OnAbort()
        {
            _innerChannel.Abort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            _innerChannel.Close(timeout);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginClose(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            _innerChannel.EndClose(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            _innerChannel.Open(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _innerChannel.EndOpen(result);
        }

        private void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            Fault();
        }
    }
}
