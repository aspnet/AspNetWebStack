// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal abstract class ChannelAcceptor<TChannel> : CommunicationObject, IChannelAcceptor<TChannel>
        where TChannel : class, IChannel
    {
        private ChannelManagerBase _channelManager;

        protected ChannelAcceptor(ChannelManagerBase channelManager)
        {
            _channelManager = channelManager;
        }

        protected ChannelManagerBase ChannelManager
        {
            get { return _channelManager; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)_channelManager).CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)_channelManager).OpenTimeout; }
        }

        public abstract TChannel AcceptChannel(TimeSpan timeout);

        public abstract IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);

        public abstract TChannel EndAcceptChannel(IAsyncResult result);

        public abstract bool WaitForChannel(TimeSpan timeout);

        public abstract IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);

        public abstract bool EndWaitForChannel(IAsyncResult result);

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}
