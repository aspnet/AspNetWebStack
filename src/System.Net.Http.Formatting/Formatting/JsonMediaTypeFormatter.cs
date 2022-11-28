// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NETFX_CORE
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
#endif
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
#if !NETFX_CORE
using System.Net.Http.Internal;
using System.Runtime.Serialization.Json;
#endif
using System.Text;
using System.Threading;
#if !NETFX_CORE
using System.Threading.Tasks;
#endif
using System.Web.Http;
#if !NETFX_CORE
using System.Xml;
#endif
using Newtonsoft.Json;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    public class JsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
    {
#if !NETFX_CORE // DataContractJsonSerializer and MediaTypeMappings are not supported in portable library
        private ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();
        private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();
        private RequestHeaderMapping _requestHeaderMapping;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            // Set default supported media types
            SupportedMediaTypes.Add(MediaTypeConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(MediaTypeConstants.TextJsonMediaType);

#if !NETFX_CORE // MediaTypeMappings are not supported in portable library
            _requestHeaderMapping = new XmlHttpRequestHeaderMapping();
            MediaTypeMappings.Add(_requestHeaderMapping);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="JsonMediaTypeFormatter"/> instance to copy settings from.</param>
        protected JsonMediaTypeFormatter(JsonMediaTypeFormatter formatter)
            : base(formatter)
        {
            Contract.Assert(formatter != null);

#if !NETFX_CORE // UseDataContractJsonSerializer is not supported in portable library
            UseDataContractJsonSerializer = formatter.UseDataContractJsonSerializer;
#endif

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

#if !NETFX_CORE // DataContractJsonSerializer is not supported in portable library
        /// <summary>
        /// Gets or sets a value indicating whether to use <see cref="DataContractJsonSerializer"/> by default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if use <see cref="DataContractJsonSerializer"/> by default; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool UseDataContractJsonSerializer { get; set; }
#endif

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

#if !NETFX_CORE // DataContractJsonSerializer not supported in portable library; no need to override there
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
                using (XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(new NonClosingDelegatingStream(readStream), effectiveEncoding, _readerQuotas, null))
                {
                    return dataContractSerializer.ReadObject(reader);
                }
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

                DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                using (XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(writeStream, effectiveEncoding, ownsStream: false))
                {
                    dataContractSerializer.WriteObject(writer, value);
                }
            }
            else
            {
                base.WriteToStream(type, value, writeStream, effectiveEncoding);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catch all is around an extensibile method")]
        private DataContractJsonSerializer CreateDataContractSerializer(Type type, bool throwOnError)
        {
            Contract.Assert(type != null);

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
#endif
    }
}
