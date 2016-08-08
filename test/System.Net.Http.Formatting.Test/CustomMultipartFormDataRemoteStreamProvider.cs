// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Moq;

namespace System.Net.Http
{
    public class CustomMultipartFormDataRemoteStreamProvider : MultipartFormDataRemoteStreamProvider
    {
        public readonly string UrlBase = "http://some/path/to/";

        public readonly List<Stream> RemoteStreams = new List<Stream>();

        private readonly bool _isResultNull;

        public CustomMultipartFormDataRemoteStreamProvider()
        {
        }

        public CustomMultipartFormDataRemoteStreamProvider(bool isResultNull)
        {
            _isResultNull = isResultNull;
        }

        public override RemoteStreamInfo GetRemoteStream(HttpContent parent, HttpContentHeaders headers)
        {
            string fileName = headers.ContentDisposition.FileName;
            return _isResultNull
                ? null
                : new RemoteStreamInfo(CreateMockStream(), UrlBase + fileName, fileName);
        }

        private Stream CreateMockStream()
        {
            Stream stream = new Mock<Stream>().Object;
            RemoteStreams.Add(stream);
            return stream;
        }
    }
}
