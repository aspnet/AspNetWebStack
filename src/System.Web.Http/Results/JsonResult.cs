// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns an <see cref="HttpStatusCode.OK"/> response with JSON data.
    /// </summary>
    /// <typeparam name="T">The type of content in the entity body.</typeparam>
    public class JsonResult<T> : IHttpActionResult
    {
        private readonly T _content;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly Encoding _encoding;
        private readonly StatusCodeResult.IDependencyProvider _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <param name="request">The request message which led to this result.</param>
        public JsonResult(T content, JsonSerializerSettings serializerSettings, Encoding encoding,
            HttpRequestMessage request)
            : this(content, serializerSettings, encoding, new StatusCodeResult.DirectDependencyProvider(request))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult{T}"/> class with the values provided.
        /// </summary>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
        public JsonResult(T content, JsonSerializerSettings serializerSettings, Encoding encoding,
            ApiController controller)
            : this(content, serializerSettings, encoding, new StatusCodeResult.ApiControllerDependencyProvider(
                controller))
        {
        }

        private JsonResult(T content, JsonSerializerSettings serializerSettings, Encoding encoding,
            StatusCodeResult.IDependencyProvider dependencies)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException("serializerSettings");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            Contract.Assert(dependencies != null);

            _content = content;
            _serializerSettings = serializerSettings;
            _encoding = encoding;
            _dependencies = dependencies;
        }

        /// <summary>Gets the content value to serialize in the entity body.</summary>
        public T Content
        {
            get { return _content; }
        }

        /// <summary>Gets the serializer settings.</summary>
        public JsonSerializerSettings SerializerSettings
        {
            get { return _serializerSettings; }
        }

        /// <summary>Gets the content encoding.</summary>
        public Encoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>Gets the request message which led to this result.</summary>
        public HttpRequestMessage Request
        {
            get { return _dependencies.Request; }
        }

        /// <inheritdoc />
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                ArraySegment<byte> segment = Serialize();
                response.Content = new ByteArrayContent(segment.Array, segment.Offset, segment.Count);
                MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("application/json");
                contentType.CharSet = _encoding.WebName;
                response.Content.Headers.ContentType = contentType;
                response.RequestMessage = _dependencies.Request;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification =
            "False positive; specifying leaveOpen: true and CloseOutput=false prevents this condition.")]
        private ArraySegment<byte> Serialize()
        {
            JsonSerializer serializer = JsonSerializer.Create(_serializerSettings);

            using (MemoryStream stream = new MemoryStream())
            {
                const int DefaultStreamWriterBufferSize = 0x400;
                using (TextWriter textWriter = new StreamWriter(stream, _encoding,
                    bufferSize: DefaultStreamWriterBufferSize, leaveOpen: true))
                {
                    using (JsonWriter jsonWriter = new JsonTextWriter(textWriter) { CloseOutput = false })
                    {
                        serializer.Serialize(jsonWriter, _content);
                        jsonWriter.Flush();
                    }
                }

                Contract.Assert(stream.Length <= Int32.MaxValue);
                return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }
    }
}
