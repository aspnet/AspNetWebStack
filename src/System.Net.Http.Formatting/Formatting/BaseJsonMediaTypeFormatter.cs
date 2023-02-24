// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Abstract <see cref="MediaTypeFormatter"/> class to support Bson and Json.
    /// </summary>
    public abstract class BaseJsonMediaTypeFormatter : MediaTypeFormatter
    {
        // Though MaxDepth is not supported in netstandard1.3, we still override JsonReader's MaxDepth
        private int _maxDepth = FormattingUtilities.DefaultMaxDepth;

        private readonly IContractResolver _defaultContractResolver;

        private JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseJsonMediaTypeFormatter"/> class.
        /// </summary>
        protected BaseJsonMediaTypeFormatter()
        {
            // Initialize serializer settings
            _defaultContractResolver = new JsonContractResolver(this);
            _jsonSerializerSettings = CreateDefaultSerializerSettings();

            // Set default supported character encodings
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseJsonMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="BaseJsonMediaTypeFormatter"/> instance to copy settings from.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "MaxDepth is sealed in existing subclasses and its documentation carries warnings.")]
        protected BaseJsonMediaTypeFormatter(BaseJsonMediaTypeFormatter formatter)
            : base(formatter)
        {
            Contract.Assert(formatter != null);
            SerializerSettings = formatter.SerializerSettings;

            MaxDepth = formatter._maxDepth;
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings
        {
            get { return _jsonSerializerSettings; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _jsonSerializerSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed by this formatter.
        /// </summary>
        /// <remarks>
        /// Any override must call the base getter and setter. The setter may be called before a derived class
        /// constructor runs, so any override should be very careful about using derived class state.
        /// </remarks>
        public virtual int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                if (value < FormattingUtilities.DefaultMinDepth)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, FormattingUtilities.DefaultMinDepth);
                }

                _maxDepth = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="JsonSerializerSettings"/> instance with the default settings used by the <see cref="BaseJsonMediaTypeFormatter"/>.
        /// </summary>
        public JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = _defaultContractResolver,
                MissingMemberHandling = MissingMemberHandling.Ignore,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None
            };
        }

        /// <summary>
        /// Determines whether this <see cref="BaseJsonMediaTypeFormatter"/> can read objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object that will be read.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be read, otherwise <c>false</c>.</returns>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return true;
        }

        /// <summary>
        /// Determines whether this <see cref="BaseJsonMediaTypeFormatter"/> can write objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object that will be written.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be written, otherwise <c>false</c>.</returns>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return true;
        }

        /// <summary>
        /// Called during deserialization to read an object of the specified <paramref name="type"/>
        /// from the specified <paramref name="readStream"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object to read.</param>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="content">The <see cref="HttpContent"/> for the content being written.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>A <see cref="Task"/> whose result will be the object instance that has been read.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            try
            {
                return Task.FromResult(ReadFromStream(type, readStream, content, formatterLogger));
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError<object>(e);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Caller's formatterLogger is notified of problem in all cases where Exception is not rethrown.")]
        private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            Contract.Assert(type != null);
            Contract.Assert(readStream != null);

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
            {
                return GetDefaultValueForType(type);
            }

            // Get the character encoding for the content
            // Never non-null since SelectCharacterEncoding() throws in error / not found scenarios
            Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);

            try
            {
                return ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }

                formatterLogger.LogError(String.Empty, e);
                return GetDefaultValueForType(type);
            }
        }

        /// <summary>
        /// Called during deserialization to read an object of the specified <paramref name="type"/>
        /// from the specified <paramref name="readStream"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="ReadFromStreamAsync"/>.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> of object to read.</param>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when reading.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>The <see cref="object"/> instance that has been read.</returns>
        public virtual object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
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

            using (JsonReader jsonReader = CreateJsonReaderInternal(type, readStream, effectiveEncoding))
            {
                jsonReader.CloseInput = false;
                jsonReader.MaxDepth = _maxDepth;

                JsonSerializer jsonSerializer = CreateJsonSerializerInternal();

                EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> errorHandler = null;
                if (formatterLogger != null)
                {
                    // Error must always be marked as handled
                    // Failure to do so can cause the exception to be rethrown at every recursive level and overflow the stack for x64 CLR processes
                    errorHandler = (sender, e) =>
                    {
                        Exception exception = e.ErrorContext.Error;
                        formatterLogger.LogError(e.ErrorContext.Path, exception);
                        e.ErrorContext.Handled = true;
                    };
                    jsonSerializer.Error += errorHandler;
                }

                try
                {
                    return jsonSerializer.Deserialize(jsonReader, type);
                }
                finally
                {
                    if (errorHandler != null)
                    {
                        // Clean up the error handler in case CreateJsonSerializer() reuses a serializer
                        jsonSerializer.Error -= errorHandler;
                    }
                }
            }
        }

        private JsonReader CreateJsonReaderInternal(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            Contract.Assert(type != null);
            Contract.Assert(readStream != null);
            Contract.Assert(effectiveEncoding != null);

            JsonReader reader = CreateJsonReader(type, readStream, effectiveEncoding);
            if (reader == null)
            {
                throw Error.InvalidOperation(Properties.Resources.MediaTypeFormatter_JsonReaderFactoryReturnedNull, "CreateJsonReader");
            }

            return reader;
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="JsonReader"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="ReadFromStreamAsync"/>.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> of object to read.</param>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when reading.</param>
        /// <returns>The <see cref="JsonWriter"/> used during deserialization.</returns>
        public abstract JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is a public extensibility point, we can't predict what exceptions will come through")]
        private JsonSerializer CreateJsonSerializerInternal()
        {
            JsonSerializer serializer = null;
            try
            {
                serializer = CreateJsonSerializer();
            }
            catch (Exception exception)
            {
                throw Error.InvalidOperation(exception, Properties.Resources.JsonSerializerFactoryThrew, "CreateJsonSerializer");
            }

            if (serializer == null)
            {
                throw Error.InvalidOperation(Properties.Resources.JsonSerializerFactoryReturnedNull, "CreateJsonSerializer");
            }

            return serializer;
        }

        /// <summary>
        /// Called during serialization and deserialization to get the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="ReadFromStreamAsync"/> and <see cref="WriteToStreamAsync"/>.
        /// </remarks>
        /// <returns>The <see cref="JsonSerializer"/> used during serialization and deserialization.</returns>
        public virtual JsonSerializer CreateJsonSerializer()
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create(SerializerSettings);
            return jsonSerializer;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
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
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled();
            }

            try
            {
                WriteToStream(type, value, writeStream, content);
                return TaskHelpers.Completed();
            }
            catch (Exception e)
            {
                return TaskHelpers.FromError(e);
            }
        }

        private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Contract.Assert(type != null);
            Contract.Assert(writeStream != null);

            Encoding effectiveEncoding = SelectCharacterEncoding(content == null ? null : content.Headers);
            WriteToStream(type, value, writeStream, effectiveEncoding);
        }

        /// <summary>
        /// Called during serialization to write an object of the specified <paramref name="type"/>
        /// to the specified <paramref name="writeStream"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="WriteToStreamAsync"/>.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> of object to write.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="writeStream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when writing.</param>
        public virtual void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
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

            using (JsonWriter jsonWriter = CreateJsonWriterInternal(type, writeStream, effectiveEncoding))
            {
                jsonWriter.CloseOutput = false;

                JsonSerializer jsonSerializer = CreateJsonSerializerInternal();
                jsonSerializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }

        private JsonWriter CreateJsonWriterInternal(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            Contract.Assert(type != null);
            Contract.Assert(writeStream != null);
            Contract.Assert(effectiveEncoding != null);

            JsonWriter writer = CreateJsonWriter(type, writeStream, effectiveEncoding);
            if (writer == null)
            {
                throw Error.InvalidOperation(Properties.Resources.MediaTypeFormatter_JsonWriterFactoryReturnedNull, "CreateJsonWriter");
            }

            return writer;
        }

        /// <summary>
        /// Called during serialization to get the <see cref="JsonWriter"/>.
        /// </summary>
        /// <remarks>
        /// Public for delegating wrappers of this class.  Expected to be called only from
        /// <see cref="WriteToStreamAsync"/>.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> of object to write.</param>
        /// <param name="writeStream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when writing.</param>
        /// <returns>The <see cref="JsonWriter"/> used during serialization.</returns>
        public abstract JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding);
    }
}
