// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.Mvc
{
    public class FileStreamResult : FileResult
    {
        // default buffer size as defined in BufferedStream type
        private const int BufferSize = 0x1000;

        public FileStreamResult(Stream fileStream, string contentType)
            : base(contentType)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }

            FileStream = fileStream;
        }

        public Stream FileStream { get; private set; }

        protected override void WriteFile(HttpResponseBase response)
        {
            // grab chunks of data and write to the output stream
            Stream outputStream = response.OutputStream;
            using (FileStream)
            {
                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    int bytesRead = FileStream.Read(buffer, 0, BufferSize);
                    if (bytesRead == 0)
                    {
                        // no more data
                        break;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}
