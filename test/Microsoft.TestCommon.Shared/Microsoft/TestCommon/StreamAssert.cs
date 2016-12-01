// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.TestCommon
{
    //// TODO RONCAIN using System.Runtime.Serialization.Json;

    /// <summary>
    /// MSTest utility for testing code operating against a stream.
    /// </summary>
    public class StreamAssert
    {
        private static StreamAssert singleton = new StreamAssert();

        public static StreamAssert Singleton { get { return singleton; } }

        /// <summary>
        /// Creates a <see cref="MemoryStream"/>, invokes <paramref name="codeThatWrites"/> to write to it,
        /// rewinds the stream to the beginning and invokes <paramref name="codeThatReadsAsync"/>.
        /// </summary>
        /// <param name="codeThatWrites">Code to write to the stream. It cannot be <c>null</c>.</param>
        /// <param name="codeThatReadsAsync">Code that reads from the stream. It cannot be <c>null</c>.</param>
        public async Task WriteAndReadAsync(Func<MemoryStream, Task> codeThatWrites, Func<MemoryStream, Task> codeThatReadsAsync)
        {
            if (codeThatWrites == null)
            {
                throw new ArgumentNullException("codeThatWrites");
            }

            if (codeThatReadsAsync == null)
            {
                throw new ArgumentNullException("codeThatReads");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await codeThatWrites(stream);

                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);

                await codeThatReadsAsync(stream);
            }
        }

        /// <summary>
        /// Creates a <see cref="Stream"/>, invokes <paramref name="codeThatWrites"/> to write to it,
        /// rewinds the stream to the beginning and invokes <paramref name="codeThatReads"/> to obtain
        /// the result to return from this method.
        /// </summary>
        /// <param name="codeThatWrites">Code to write to the stream.  It cannot be <c>null</c>.</param>
        /// <param name="codeThatReads">Code that reads from the stream and returns the result.  It cannot be <c>null</c>.</param>
        /// <returns>The value returned from <paramref name="codeThatReads"/>.</returns>
        public static object WriteAndReadResult(Action<Stream> codeThatWrites, Func<Stream, object> codeThatReads)
        {
            if (codeThatWrites == null)
            {
                throw new ArgumentNullException("codeThatWrites");
            }

            if (codeThatReads == null)
            {
                throw new ArgumentNullException("codeThatReads");
            }

            object result = null;
            using (MemoryStream stream = new MemoryStream())
            {
                codeThatWrites(stream);

                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);

                result = codeThatReads(stream);
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="Stream"/>, invokes <paramref name="codeThatWrites"/> to write to it,
        /// rewinds the stream to the beginning and invokes <paramref name="codeThatReads"/> to obtain
        /// the result to return from this method.
        /// </summary>
        /// <typeparam name="T">The type of the result expected.</typeparam>
        /// <param name="codeThatWrites">Code to write to the stream.  It cannot be <c>null</c>.</param>
        /// <param name="codeThatReads">Code that reads from the stream and returns the result.  It cannot be <c>null</c>.</param>
        /// <returns>The value returned from <paramref name="codeThatReads"/>.</returns>
        public T WriteAndReadResult<T>(Action<Stream> codeThatWrites, Func<Stream, object> codeThatReads)
        {
            return (T)WriteAndReadResult(codeThatWrites, codeThatReads);
        }
    }
}
