// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;
using System.Xml;

namespace System.Web.Http.SelfHost.Channels
{
    internal sealed class HttpMessage : Message
    {
        private HttpRequestMessage _request;
        private HttpResponseMessage _response;
        private MessageHeaders _headers;
        private MessageProperties _properties;

        public HttpMessage(HttpRequestMessage request)
        {
            Contract.Assert(request != null, "The 'request' parameter should not be null.");
            _request = request;
            Headers.To = request.RequestUri;
            IsRequest = true;
        }

        public HttpMessage(HttpResponseMessage response)
        {
            Contract.Assert(response != null, "The 'response' parameter should not be null.");
            _response = response;
            IsRequest = false;
        }

        public override MessageVersion Version
        {
            get
            {
                EnsureNotDisposed();
                return MessageVersion.None;
            }
        }

        public override MessageHeaders Headers
        {
            get
            {
                EnsureNotDisposed();
                if (_headers == null)
                {
                    _headers = new MessageHeaders(MessageVersion.None);
                }

                return _headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                EnsureNotDisposed();
                if (_properties == null)
                {
                    _properties = new MessageProperties();
                    _properties.AllowOutputBatching = false;
                }

                return _properties;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                long? contentLength = GetHttpContentLength();
                return contentLength.HasValue && contentLength.Value == 0;
            }
        }

        public override bool IsFault
        {
            get { return false; }
        }

        public bool IsRequest { get; private set; }

        public HttpRequestMessage GetHttpRequestMessage(bool extract)
        {
            EnsureNotDisposed();
            Contract.Assert(IsRequest, "This method should only be called when IsRequest is true.");
            if (extract)
            {
                HttpRequestMessage req = _request;
                _request = null;
                return req;
            }

            return _request;
        }

        public HttpResponseMessage GetHttpResponseMessage(bool extract)
        {
            EnsureNotDisposed();
            Contract.Assert(!IsRequest, "This method should only be called when IsRequest is false.");
            if (extract)
            {
                HttpResponseMessage res = _response;
                _response = null;
                return res;
            }

            return _response;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            long? contentLength = GetHttpContentLength();
            string contentString = null;

            if (IsRequest)
            {
                contentString = contentLength.HasValue
                                    ? Error.Format(SRResources.MessageBodyIsHttpRequestMessageWithKnownContentLength, contentLength.Value)
                                    : SRResources.MessageBodyIsHttpRequestMessageWithUnknownContentLength;
            }
            else
            {
                contentString = contentLength.HasValue
                                    ? Error.Format(SRResources.MessageBodyIsHttpResponseMessageWithKnownContentLength, contentLength.Value)
                                    : SRResources.MessageBodyIsHttpResponseMessageWithUnknownContentLength;
            }

            writer.WriteString(contentString);
        }

        protected override void OnWriteMessage(XmlDictionaryWriter writer)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            throw Error.NotSupported(GetNotSupportedMessage());
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (_request != null)
            {
                _request.DisposeRequestResources();
                _request.Dispose();
                _request = null;
            }

            if (_response != null)
            {
                _response.Dispose();
                _response = null;
            }
        }

        private static string GetNotSupportedMessage()
        {
            return Error.Format(
                SRResources.MessageReadWriteCopyNotSupported,
                HttpMessageExtensions.ToHttpRequestMessageMethodName,
                HttpMessageExtensions.ToHttpResponseMessageMethodName,
                typeof(HttpMessage).Name);
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw Error.ObjectDisposed(SRResources.MessageClosed, typeof(Message).Name);
            }
        }

        private long? GetHttpContentLength()
        {
            HttpContent content = IsRequest
                                      ? GetHttpRequestMessage(false).Content
                                      : GetHttpResponseMessage(false).Content;

            if (content == null)
            {
                return 0;
            }

            return content.Headers.ContentLength;
        }
    }
}
