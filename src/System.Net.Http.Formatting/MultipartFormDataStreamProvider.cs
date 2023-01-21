// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.IO;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// A <see cref="MultipartStreamProvider"/> implementation suited for use with HTML file uploads for writing file
    /// content to a <see cref="FileStream"/>. The stream provider looks at the <b>Content-Disposition</b> header
    /// field and determines an output <see cref="Stream"/> based on the presence of a <b>filename</b> parameter.
    /// If a <b>filename</b> parameter is present in the <b>Content-Disposition</b> header field then the body
    /// part is written to a <see cref="FileStream"/>, otherwise it is written to a <see cref="MemoryStream"/>.
    /// This makes it convenient to process MIME Multipart HTML Form data which is a combination of form
    /// data and file content.
    /// </summary>
    public class MultipartFormDataStreamProvider : MultipartFileStreamProvider
    {
        // pass around cancellation token through field to maintain backward compat.
        private CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        public MultipartFormDataStreamProvider(string rootPath)
            : base(rootPath)
        {
            FormData = HttpValueCollection.Create();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        /// <param name="bufferSize">The number of bytes buffered for writes to the file.</param>
        public MultipartFormDataStreamProvider(string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
            FormData = HttpValueCollection.Create();
        }

        /// <summary>
        /// Gets a <see cref="NameValueCollection"/> of form data passed as part of the multipart form data.
        /// </summary>
        public NameValueCollection FormData { get; private set; }

        /// <summary>
        /// This body part stream provider examines the headers provided by the MIME multipart parser
        /// and decides whether it should return a file stream or a memory stream for the body part to be
        /// written to.
        /// </summary>
        /// <param name="parent">The parent MIME multipart HttpContent instance.</param>
        /// <param name="headers">Header fields describing the body part</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (MultipartFormDataStreamProviderHelper.IsFileContent(parent, headers))
            {
                return base.GetStream(parent, headers);
            }

            return new MemoryStream();
        }

        /// <summary>
        /// Read the non-file contents as form data.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the post processing.</returns>
        public override Task ExecutePostProcessingAsync()
        {
            // This method predates support for cancellation, and we need to make sure it is always invoked when
            // ExecutePostProcessingAsync is called for compatability.
            return MultipartFormDataStreamProviderHelper.ReadFormDataAsync(Contents, FormData,
                _cancellationToken);
        }

        /// <summary>
        /// Read the non-file contents as form data.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the post processing.</returns>
        public override Task ExecutePostProcessingAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return ExecutePostProcessingAsync();
        }
    }
}
