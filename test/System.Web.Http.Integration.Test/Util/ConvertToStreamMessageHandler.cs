// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Util
{
    internal class ConvertToStreamMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContent requestContent = await ToStreamContent(request.Content);
            request.Content = requestContent;
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            HttpContent responseContent = await ToStreamContent(response.Content);
            response.Content = responseContent;
            return response;
        }

        private static Task<HttpContent> ToStreamContent(HttpContent content)
        {
            ObjectContent objectContent = content as ObjectContent;
            if (objectContent != null)
            {
                return ToStreamContent(objectContent);
            }
            else
            {
                return Task.FromResult(content);
            }
        }

        private static async Task<HttpContent> ToStreamContent(ObjectContent content)
        {
            Stream stream = await content.ReadAsStreamAsync();
            StreamContent streamContent = new StreamContent(stream);
            foreach (var header in content.Headers)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return streamContent;
        }
    }
}
