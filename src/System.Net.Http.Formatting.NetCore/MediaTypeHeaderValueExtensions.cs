// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    internal static class MediaTypeHeaderValueExtensions
    {
        public static MediaTypeHeaderValue Clone(this MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null && mediaType.GetType() == typeof(MediaTypeHeaderValue));

            var result = new MediaTypeHeaderValue(mediaType.MediaType);
            foreach (var parameter in mediaType.Parameters)
            {
                result.Parameters.Add(new NameValueHeaderValue(parameter.Name, parameter.Value));
            }

            return result;
        }
    }
}
