// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// A <see cref="MultipartStreamProvider"/> suited for writing each MIME body parts of the MIME multipart
    /// message to a file using a <see cref="FileStream"/>.
    /// </summary>
    public class MultipartFileStreamProvider : MultipartStreamProvider
    {
        private const int MinBufferSize = 1;
        private const int DefaultBufferSize = 0x1000;

        private string _rootPath;
        private int _bufferSize = DefaultBufferSize;

        private Collection<MultipartFileData> _fileData = new Collection<MultipartFileData>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFileStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        public MultipartFileStreamProvider(string rootPath)
            : this(rootPath, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFileStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        /// <param name="bufferSize">The number of bytes buffered for writes to a file.</param>
        public MultipartFileStreamProvider(string rootPath, int bufferSize)
        {
            if (rootPath == null)
            {
                throw Error.ArgumentNull("rootPath");
            }

            if (bufferSize < MinBufferSize)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, MinBufferSize);
            }

            _rootPath = Path.GetFullPath(rootPath);
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Gets a collection containing the local files names and associated HTTP content headers of MIME 
        /// body parts written to file.
        /// </summary>
        public Collection<MultipartFileData> FileData
        {
            get { return _fileData; }
        }

        /// <summary>
        /// Gets the root path where the content of MIME multipart body parts are written to.
        /// </summary>
        protected string RootPath
        {
            get { return _rootPath; }
        }

        /// <summary>
        /// Gets the number of bytes buffered for writes to a file.
        /// </summary>
        protected int BufferSize
        {
            get { return _bufferSize; }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stream is closed by caller (MultipartWriteDelegatingStream is just a wrapper that calls into the inner stream.)")]
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

            string localFilePath;
            try
            {
                string filename = GetLocalFileName(headers);
                localFilePath = Path.Combine(_rootPath, Path.GetFileName(filename));
            }
            catch (Exception e)
            {
                throw Error.InvalidOperation(e, Properties.Resources.MultipartStreamProviderInvalidLocalFileName);
            }

            // Add local file name 
            MultipartFileData fileData = new MultipartFileData(headers, localFilePath);
            _fileData.Add(fileData);

            return File.Create(localFilePath, _bufferSize, FileOptions.Asynchronous);
        }

        /// <summary>
        /// Gets the name of the local file which will be combined with the root path to
        /// create an absolute file name where the contents of the current MIME body part
        /// will be stored.
        /// </summary>
        /// <param name="headers">The headers for the current MIME body part.</param>
        /// <returns>A relative filename with no path component.</returns>
        public virtual string GetLocalFileName(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            return String.Format(CultureInfo.InvariantCulture, "BodyPart_{0}", Guid.NewGuid());
        }
    }
}
