// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
#if NETFX_CORE
using NameValueCollection = System.Net.Http.Formatting.HttpValueCollection;
#endif

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods to allow HTML form URL-encoded data, also known as <c>application/x-www-form-urlencoded</c>, 
    /// to be read from <see cref="HttpContent"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpContentFormDataExtensions
    {
        private const string ApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Determines whether the specified content is HTML form URL-encoded data, also known as <c>application/x-www-form-urlencoded</c> data.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// <c>true</c> if the specified content is HTML form URL-encoded data; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFormData(this HttpContent content)
        {
            if (content == null)
            {
                throw Error.ArgumentNull("content");
            }

            MediaTypeHeaderValue contentType = content.Headers.ContentType;
            return contentType != null && String.Equals(ApplicationFormUrlEncoded, contentType.MediaType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that will yield a <see cref="NameValueCollection"/> instance containing the form data
        /// parsed as HTML form URL-encoded from the <paramref name="content"/> instance.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>A <see cref="Task{T}"/> which will provide the result. If the data can not be read
        /// as HTML form URL-encoded data then the result is null.</returns>
        public static Task<NameValueCollection> ReadAsFormDataAsync(this HttpContent content)
        {
            return ReadAsFormDataAsync(content, CancellationToken.None);
        }

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that will yield a <see cref="NameValueCollection"/> instance containing the form data
        /// parsed as HTML form URL-encoded from the <paramref name="content"/> instance.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{T}"/> which will provide the result. If the data can not be read
        /// as HTML form URL-encoded data then the result is null.</returns>
        public static Task<NameValueCollection> ReadAsFormDataAsync(this HttpContent content, CancellationToken cancellationToken)
        {
            if (content == null)
            {
                throw Error.ArgumentNull("content");
            }

            MediaTypeFormatter[] formatters = new MediaTypeFormatter[1] { new FormUrlEncodedMediaTypeFormatter() };
            return ReadAsAsyncCore(content, formatters, cancellationToken);
        }

        private static async Task<NameValueCollection> ReadAsAsyncCore(HttpContent content, MediaTypeFormatter[] formatters,
            CancellationToken cancellationToken)
        {
            FormDataCollection formData = await content.ReadAsAsync<FormDataCollection>(formatters, cancellationToken);
            return formData == null ? null : formData.ReadAsNameValueCollection();
        }
    }
}
