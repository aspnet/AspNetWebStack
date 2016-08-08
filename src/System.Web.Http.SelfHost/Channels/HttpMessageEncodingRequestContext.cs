// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.Channels
{
    internal class HttpMessageEncodingRequestContext : RequestContext
    {
        private const string HttpMessageEncodingRequestContextPropertyName = "MS_HttpMessageEncodingRequestContextKey";
        private const string DefaultReasonPhrase = "OK";

        private RequestContext _innerContext;
        private Message _configuredRequestMessage;

        public HttpMessageEncodingRequestContext(RequestContext innerContext)
        {
            Contract.Assert(innerContext != null, "The 'innerContext' parameter should not be null.");
            _innerContext = innerContext;
        }

        internal Exception Exception { get; set; }
        internal BufferManager BufferManager { get; set; }
        internal byte[] BufferToReturn { get; set; }

        public override Message RequestMessage
        {
            get
            {
                if (_configuredRequestMessage == null)
                {
                    _configuredRequestMessage = ConfigureRequestMessage(_innerContext.RequestMessage);
                }

                return _configuredRequestMessage;
            }
        }

        public override void Abort()
        {
            Cleanup();
            _innerContext.Abort();
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ConfigureResponseMessage(message);
            return _innerContext.BeginReply(message, timeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            ConfigureResponseMessage(message);
            return _innerContext.BeginReply(message, callback, state);
        }

        public override void Close(TimeSpan timeout)
        {
            Cleanup();
            _innerContext.Close(timeout);
        }

        public override void Close()
        {
            Cleanup();
            _innerContext.Close();
        }

        public override void EndReply(IAsyncResult result)
        {
            try
            {
                _innerContext.EndReply(result);
            }
            catch (Exception ex)
            {
                Exception = ex;
                throw;
            }
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            ConfigureResponseMessage(message);
            _innerContext.Reply(message, timeout);
        }

        public override void Reply(Message message)
        {
            ConfigureResponseMessage(message);
            _innerContext.Reply(message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // To avoid double buffering, we use the BufferedOutputStream's own buffer
                // that it created through the BufferManager passed to WriteStream.
                // We are responsible for returning that buffer to the BufferManager
                // because we used BufferedOutputStream.ToArray to take ownership
                // of its buffers.
                if (BufferManager != null && BufferToReturn != null)
                {
                    BufferManager.ReturnBuffer(BufferToReturn);
                    BufferToReturn = null;
                }
            }

            base.Dispose(disposing);
        }

        internal static HttpMessageEncodingRequestContext GetContextFromMessage(Message message)
        {
            HttpMessageEncodingRequestContext context = null;
            message.Properties.TryGetValue<HttpMessageEncodingRequestContext>(HttpMessageEncodingRequestContextPropertyName, out context);
            return context;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cleanup exceptions are ignored as they are not fatal to the host.")]
        private void Cleanup()
        {
            if (_configuredRequestMessage != null)
            {
                try
                {
                    _configuredRequestMessage.Close();
                }
                catch
                {
                }
            }
        }

        private static void CopyHeadersToNameValueCollection(HttpHeaders headers, NameValueCollection nameValueCollection)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                foreach (string value in header.Value)
                {
                    nameValueCollection.Add(header.Key, value);
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        private static Message ConfigureRequestMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }

            HttpRequestMessageProperty requestProperty;
            if (!message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out requestProperty))
            {
                throw Error.InvalidOperation(
                    SRResources.RequestMissingHttpRequestMessageProperty,
                    HttpRequestMessageProperty.Name,
                    typeof(HttpRequestMessageProperty).Name);
            }

            Uri uri = message.Headers.To;
            if (uri == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMissingToHeader);
            }

            HttpRequestMessage httpRequestMessage = message.ToHttpRequestMessage();
            if (httpRequestMessage == null)
            {
                if (!message.IsEmpty)
                {
                    throw Error.InvalidOperation(SRResources.NonHttpMessageMustBeEmpty, HttpMessageExtensions.ToHttpRequestMessageMethodName, typeof(HttpMessage).Name);
                }

                httpRequestMessage = new HttpRequestMessage();
                Message oldMessage = message;
                message = httpRequestMessage.ToMessage();
                message.Properties.CopyProperties(oldMessage.Properties);
                oldMessage.Close();
            }
            else
            {
                // Clear headers but not properties.
                message.Headers.Clear();
            }

            // Copy message properties to HttpRequestMessage. While it does have the
            // risk of allowing properties to get out of sync they in virtually all cases are
            // read-only so the risk is low. The downside to not doing it is that it isn't
            // possible to access anything from HttpRequestMessage (or OperationContent.Current)
            // which is worse.
            foreach (KeyValuePair<string, object> kv in message.Properties)
            {
                httpRequestMessage.Properties.Add(kv.Key, kv.Value);
            }

            if (httpRequestMessage.Content == null)
            {
                httpRequestMessage.Content = new ByteArrayContent(new byte[0]);
            }
            else
            {
                httpRequestMessage.Content.Headers.Clear();
            }

            message.Headers.To = uri;

            httpRequestMessage.RequestUri = uri;
            httpRequestMessage.Method = HttpMethodHelper.GetHttpMethod(requestProperty.Method);

            WebHeaderCollection headers = requestProperty.Headers;
            foreach (var headerName in headers.AllKeys)
            {
                string headerValue = headers[headerName];
                if (!httpRequestMessage.Headers.TryAddWithoutValidation(headerName, headerValue))
                {
                    httpRequestMessage.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
                }
            }

            return message;
        }

        private void ConfigureResponseMessage(Message message)
        {
            Contract.Assert(message != null);

            HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty();
            HttpResponseMessage httpResponseMessage = message.ToHttpResponseMessage();

            if (httpResponseMessage == null)
            {
                responseProperty.StatusCode = HttpStatusCode.InternalServerError;
                responseProperty.SuppressEntityBody = true;
            }
            else
            {
                responseProperty.StatusCode = httpResponseMessage.StatusCode;
                if (httpResponseMessage.ReasonPhrase != null &&
                    httpResponseMessage.ReasonPhrase != DefaultReasonPhrase)
                {
                    responseProperty.StatusDescription = httpResponseMessage.ReasonPhrase;
                }

                CopyHeadersToNameValueCollection(httpResponseMessage.Headers, responseProperty.Headers);
                HttpContent content = httpResponseMessage.Content;
                if (content != null)
                {
                    CopyHeadersToNameValueCollection(httpResponseMessage.Content.Headers, responseProperty.Headers);
                }
                else
                {
                    responseProperty.SuppressEntityBody = true;
                }
            }

            message.Properties.Clear();
            message.Headers.Clear();

            message.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);

            // The current request context flows with the Message for later use
            // by HttpMessageEncoder.WriteMessage
            message.Properties.Add(HttpMessageEncodingRequestContextPropertyName, this);
        }
    }
}
