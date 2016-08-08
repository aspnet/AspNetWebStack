// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    public class MultipartFileData
    {
        public MultipartFileData(HttpContentHeaders headers, string localFileName)
        {
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            if (localFileName == null)
            {
                throw Error.ArgumentNull("localFileName");
            }

            Headers = headers;
            LocalFileName = localFileName;
        }

        public HttpContentHeaders Headers { get; private set; }

        public string LocalFileName { get; private set; }
    }
}
