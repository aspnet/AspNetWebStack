// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Provides a <see cref="MultipartStreamProvider"/> implementation that returns a <see cref="MemoryStream"/> instance.
    /// This facilitates deserialization or other manipulation of the contents in memory.
    /// </summary>
    public class MultipartMemoryStreamProvider : MultipartStreamProvider
    {
        /// <summary>
        /// This <see cref="MultipartStreamProvider"/> implementation returns a <see cref="MemoryStream"/> instance.
        /// This facilitates deserialization or other manipulation of the contents in memory. 
        /// </summary>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
            {
                throw Error.ArgumentNull("parent");
            }

            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            return new MemoryStream();
        }
    }
}
