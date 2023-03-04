// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
#if !NETSTANDARD1_3 // Unnecessary when targeting netstandard1.3.
using System.Net.Http.Internal;
#endif
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    public class JsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
    {
        private readonly ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();
        private readonly XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();
        private readonly RequestHeaderMapping _requestHeaderMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            // Set default supported media types
            SupportedMediaTypes.Add(MediaTypeConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(MediaTypeConstants.TextJsonMediaType);

            _requestHeaderMapping = new XmlHttpRequestHeaderMapping();
            MediaTypeMappings.Add(_requestHeaderMapping);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="JsonMediaTypeFormatter"/> instance to copy settings from.</param>
        protected JsonMediaTypeFormatter(JsonMediaTypeFormatter formatter)
            : base(formatter)
        {
            Contract.Assert(formatter != null);

            UseDataContractJsonSerializer = formatter.UseDataContractJsonSerializer;
            Indent = formatter.Indent;
        }

        /// <summary>
        /// Gets the default media type for Json, namely "application/json".
        /// </summary>
        /// <remarks>
        /// The default media type does not have any <c>charset</c> parameter as
        /// the <see cref="Encoding"/> can be configured on a per <see cref="JsonMediaTypeFormatter"/>
        /// instance basis.
        /// </remarks>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get { return MediaTypeConstants.ApplicationJsonMediaType; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use <see cref="DataContractJsonSerializer"/> by default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if use <see cref="DataContractJsonSerializer"/> by default; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool UseDataContractJsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements when writing data.
        /// </summary>
        public bool Indent { get; set; }

        /// <inheritdoc/>
        public sealed override int MaxDepth
        {
            get
            {
                return base.MaxDepth;
            }
            set
            {
                base.MaxDepth = value;
                _readerQuotas.MaxDepth = value;
            }
        }

        /// <inheritdoc />
        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            return new JsonTextReader(new StreamReader(readStream, effectiveEncoding));
        }

        /// <inheritdoc />
        public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(writeStream, effectiveEncoding));
            if (Indent)
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            }

            return jsonWriter;
        }

        /// <inheritdoc />
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (UseDataContractJsonSerializer)
            {
                // If there is a registered non-null serializer, we can support this type.
                DataContractJsonSerializer serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t, throwOnError: false));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
            {
                return base.CanReadType(type);
            }
        }

        /// <inheritdoc />
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (UseDataContractJsonSerializer)
            {
                MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type);

                // If there is a registered non-null serializer, we can support this type.
                object serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t, throwOnError: false));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
            {
                return base.CanWriteType(type);
            }
        }

        /// <inheritdoc />
        public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            if (UseDataContractJsonSerializer)
            {
                DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);

#if NETSTANDARD1_3 // Unreachable when targeting netstandard1.3. Return just to satisfy the compiler.
                return null;
#else
                // DCS encodings are limited to UTF8, UTF16BE, and UTF16LE. Convert to UTF8 as we read.
                Stream innerStream =
                    string.Equals(effectiveEncoding.WebName, Utf8Encoding.WebName, StringComparison.OrdinalIgnoreCase) ?
                    new NonClosingDelegatingStream(readStream) :
                    new TranscodingStream(readStream, effectiveEncoding, Utf8Encoding, leaveOpen: true);

                // XmlDictionaryReader will always dispose of innerStream when we dispose of the reader.
                using XmlDictionaryReader reader =
                    JsonReaderWriterFactory.CreateJsonReader(innerStream, Utf8Encoding, _readerQuotas, onClose: null);
                return dataContractSerializer.ReadObject(reader);
#endif
            }
            else
            {
                return base.ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
            }
        }

        /// <inheritdoc />
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (UseDataContractJsonSerializer && Indent)
            {
                throw Error.NotSupported(Properties.Resources.UnsupportedIndent, typeof(DataContractJsonSerializer));
            }

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
        }

        /// <inheritdoc />
        public override void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            if (UseDataContractJsonSerializer)
            {
                if (MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type))
                {
                    if (value != null)
                    {
                        value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[] { value });
                    }
                }

                WritePreamble(writeStream, effectiveEncoding);
                if (string.Equals(effectiveEncoding.WebName, Utf8Encoding.WebName, StringComparison.OrdinalIgnoreCase))
                {
                    WriteObject(writeStream, type, value);
                }
                else
                {
                    // JsonReaderWriterFactory is internal and DataContractJsonSerializer only writes UTF8 for the
                    // netstandard1.3 project. In addition, DCS encodings are limited to UTF8, UTF16BE, and UTF16LE.
                    // Convert to UTF8 as we write.
                    using var innerStream = new TranscodingStream(writeStream, effectiveEncoding, Utf8Encoding, leaveOpen: true);
                    WriteObject(innerStream, type, value);
                }
            }
            else
            {
                base.WriteToStream(type, value, writeStream, effectiveEncoding);
            }
        }

        private void WriteObject(Stream stream, Type type, object value)
        {
            DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);

#if !NETSTANDARD1_3 // Unreachable when targeting netstandard1.3.
            // Do not dispose of the stream. WriteToStream handles that where it's needed.
            using XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Utf8Encoding, ownsStream: false);
            dataContractSerializer.WriteObject(writer, value);
#endif
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catch all is around an extensibile method")]
        private DataContractJsonSerializer CreateDataContractSerializer(Type type, bool throwOnError)
        {
            Contract.Assert(type != null);

#if NETSTANDARD1_3 // XsdDataContractExporter is not supported in netstandard1.3
            if (throwOnError)
            {
                throw new PlatformNotSupportedException(Error.Format(
                    Properties.Resources.JsonMediaTypeFormatter_DCS_NotSupported,
                    nameof(UseDataContractJsonSerializer)));
            }
            else
            {
                return null;
            }
#else

            DataContractJsonSerializer serializer = null;
            Exception exception = null;

            try
            {
                // Verify that type is a valid data contract by forcing the serializer to try to create a data contract
                FormattingUtilities.XsdDataContractExporter.GetRootElementName(type);

                serializer = CreateDataContractSerializer(type);
            }
            catch (Exception caught)
            {
                exception = caught;
            }

            if (serializer == null && throwOnError)
            {
                if (exception != null)
                {
                    throw Error.InvalidOperation(exception, Properties.Resources.SerializerCannotSerializeType,
                                  typeof(DataContractJsonSerializer).Name,
                                  type.Name);
                }
                else
                {
                    throw Error.InvalidOperation(Properties.Resources.SerializerCannotSerializeType,
                                  typeof(DataContractJsonSerializer).Name,
                                  type.Name);
                }
            }

            return serializer;
#endif
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="DataContractJsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="BaseJsonMediaTypeFormatter.ReadFromStreamAsync"/> and <see cref="WriteToStreamAsync"/>.
        /// </remarks>
        /// <param name="type">The type of object that will be serialized or deserialized.</param>
        /// <returns>The <see cref="DataContractJsonSerializer"/> used to serialize the object.</returns>
        public virtual DataContractJsonSerializer CreateDataContractSerializer(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return new DataContractJsonSerializer(type);
        }

        private DataContractJsonSerializer GetDataContractSerializer(Type type)
        {
            Contract.Assert(type != null, "Type cannot be null");

            DataContractJsonSerializer serializer =
                _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(type, throwOnError: true));

            if (serializer == null)
            {
                // A null serializer means the type cannot be serialized
                throw Error.InvalidOperation(Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name);
            }

            return serializer;
        }
    }
}
