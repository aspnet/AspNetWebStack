// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http;

namespace System.Net.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Attempts to retrieve a strongly-typed value from a <paramref name="response"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="HttpResponseMessage.Content"/> is an instance of <see cref="ObjectContent"/>
        /// attempts to retrieve the <see cref="ObjectContent.Value"/> if it is compatible with <typeparamref name="T"/>.
        /// If it is it returns <c>true</c> and sets <paramref name="value"/>. If not it returns <c>false</c> and
        /// sets <paramref name="value"/> to the default instance of <typeparamref name="T"/>.
        /// </remarks>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="response">The response.</param>
        /// <param name="value">Will contain the retrieved value if this method succeeds.</param>
        /// <returns>Returns <c>true</c> if the response has a content with a value that can be cast to <typeparamref name="T"/>,
        /// <c>false</c> otherwise.</returns>
        public static bool TryGetContentValue<T>(this HttpResponseMessage response, out T value)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            ObjectContent content = response.Content as ObjectContent;
            if (content != null)
            {
                if (content.Value is T)
                {
                    value = (T)content.Value;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Attaches the given <paramref name="request"/> to the <paramref name="response"/> if the response does not already
        /// have a pointer to a request.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="request">The request.</param>
        internal static void EnsureResponseHasRequest(this HttpResponseMessage response, HttpRequestMessage request)
        {
            if (response != null && response.RequestMessage == null)
            {
                response.RequestMessage = request;
            }
        }
    }
}
