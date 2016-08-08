// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncodingReplyChannel : LayeredChannel<IReplyChannel>, IReplyChannel, IChannel, ICommunicationObject
    {
        public HttpMessageEncodingReplyChannel(ChannelManagerBase channelManager, IReplyChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public EndpointAddress LocalAddress
        {
            get { return InnerChannel.LocalAddress; }
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return InnerChannel.BeginReceiveRequest(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            RequestContext innerContext = InnerChannel.EndReceiveRequest(result);
            return WrapRequestContext(innerContext);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            RequestContext innerContext;
            context = null;
            if (!InnerChannel.EndTryReceiveRequest(result, out innerContext))
            {
                return false;
            }

            context = WrapRequestContext(innerContext);
            return true;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return InnerChannel.EndWaitForRequest(result);
        }

        public RequestContext ReceiveRequest()
        {
            RequestContext innerContext = InnerChannel.ReceiveRequest();
            return WrapRequestContext(innerContext);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext innerContext = InnerChannel.ReceiveRequest(timeout);
            return WrapRequestContext(innerContext);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            RequestContext innerContext;
            if (InnerChannel.TryReceiveRequest(timeout, out innerContext))
            {
                context = WrapRequestContext(innerContext);
                return true;
            }

            context = null;
            return false;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return InnerChannel.WaitForRequest(timeout);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        private static RequestContext WrapRequestContext(RequestContext innerContext)
        {
            return (innerContext != null)
                       ? new HttpMessageEncodingRequestContext(innerContext)
                       : (RequestContext)null;
        }
    }
}
