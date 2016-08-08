// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{
    /// <summary>
    /// Provides extension methods for getting an <see cref="HttpRequestMessage"/> instance or
    /// an <see cref="HttpResponseMessage"/> instance from a <see cref="Message"/> instance and
    /// provides extension methods for creating a <see cref="Message"/> instance from either an 
    /// <see cref="HttpRequestMessage"/> instance or an 
    /// <see cref="HttpResponseMessage"/> instance.
    /// </summary>
    internal static class HttpMessageExtensions
    {
        internal const string ToHttpRequestMessageMethodName = "ToHttpRequestMessage";
        internal const string ToHttpResponseMessageMethodName = "ToHttpResponseMessage";
        internal const string ToMessageMethodName = "ToMessage";

        /// <summary>
        /// Returns a reference to the <see cref="HttpRequestMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpRequestMessage"/> 
        /// instance.
        /// </summary>
        /// <param name="message">The given <see cref="Message"/> that holds a reference to an 
        /// <see cref="HttpRequestMessage"/> instance.
        /// </param>
        /// <returns>
        /// A reference to the <see cref="HttpRequestMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpRequestMessage"/> 
        /// instance.
        /// </returns>
        public static HttpRequestMessage ToHttpRequestMessage(this Message message)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            HttpMessage httpMessage = message as HttpMessage;
            if (httpMessage != null && httpMessage.IsRequest)
            {
                return httpMessage.GetHttpRequestMessage(false);
            }

            return null;
        }

        /// <summary>
        /// Returns a reference to the <see cref="HttpResponseMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpResponseMessage"/> 
        /// instance.
        /// </summary>
        /// <param name="message">The given <see cref="Message"/> that holds a reference to an 
        /// <see cref="HttpResponseMessage"/> instance.
        /// </param>
        /// <returns>
        /// A reference to the <see cref="HttpResponseMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpResponseMessage"/> 
        /// instance.
        /// </returns>
        public static HttpResponseMessage ToHttpResponseMessage(this Message message)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            HttpMessage httpMessage = message as HttpMessage;
            if (httpMessage != null && !httpMessage.IsRequest)
            {
                return httpMessage.GetHttpResponseMessage(false);
            }

            return null;
        }

        /// <summary>
        /// Returns a reference to the <see cref="HttpResponseMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpResponseMessage"/> 
        /// instance.
        /// </summary>
        /// <remarks>The caller takes over the ownership of the associated <see cref="HttpRequestMessage"/> and is responsible for its disposal.</remarks>
        /// <param name="message">The given <see cref="Message"/> that holds a reference to an 
        /// <see cref="HttpResponseMessage"/> instance.
        /// </param>
        /// <returns>
        /// A reference to the <see cref="HttpResponseMessage"/> 
        /// instance held by the given <see cref="Message"/> or null if the <see cref="Message"/> does not
        /// hold a reference to an <see cref="HttpResponseMessage"/> 
        /// instance.
        /// The caller is responsible for disposing any <see cref="HttpResponseMessage"/> instance returned.
        /// </returns>
        public static HttpResponseMessage ExtractHttpResponseMessage(this Message message)
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            HttpMessage httpMessage = message as HttpMessage;
            if (httpMessage != null && !httpMessage.IsRequest)
            {
                return httpMessage.GetHttpResponseMessage(true);
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> that holds a reference to the given 
        /// <see cref="HttpRequestMessage"/> instance.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> 
        /// instance to which the new <see cref="Message"/> should hold a reference.
        /// </param>
        /// <returns>A <see cref="Message"/> that holds a reference to the given
        /// <see cref="HttpRequestMessage"/> instance.
        /// </returns>
        public static Message ToMessage(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return new HttpMessage(request);
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> that holds a reference to the given
        /// <see cref="HttpResponseMessage"/> instance.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> 
        /// instance to which the new <see cref="Message"/> should hold a reference.
        /// </param>
        /// <returns>A <see cref="Message"/> that holds a reference to the given
        /// <see cref="HttpResponseMessage"/> instance.
        /// </returns>
        public static Message ToMessage(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            return new HttpMessage(response);
        }
    }
}
