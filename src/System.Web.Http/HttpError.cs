// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a serializable container for storing error information. This information is stored 
    /// as key/value pairs. The dictionary keys to look up standard error information are available 
    /// on the <see cref="HttpErrorKeys"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This type is only a dictionary to get the right serialization format")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "DCS does not support IXmlSerializable types that are also marked as [Serializable]")]
    [XmlRoot("Error")]
    public sealed class HttpError : Dictionary<string, object>, IXmlSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class.
        /// </summary>
        public HttpError()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class containing error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message to associate with this instance.</param>
        public HttpError(string message)
            : this()
        {
            if (message == null)
            {
                throw Error.ArgumentNull("message");
            }

            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The exception to use for error information.</param>
        /// <param name="includeErrorDetail"><c>true</c> to include the exception information in the error; <c>false</c> otherwise</param>
        public HttpError(Exception exception, bool includeErrorDetail)
            : this()
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            Message = SRResources.ErrorOccurred;

            if (includeErrorDetail)
            {
                Add(HttpErrorKeys.ExceptionMessageKey, exception.Message);
                Add(HttpErrorKeys.ExceptionTypeKey, exception.GetType().FullName);
                Add(HttpErrorKeys.StackTraceKey, exception.StackTrace);
                if (exception.InnerException != null)
                {
                    Add(HttpErrorKeys.InnerExceptionKey, new HttpError(exception.InnerException, includeErrorDetail));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class for <paramref name="modelState"/>.
        /// </summary>
        /// <param name="modelState">The invalid model state to use for error information.</param>
        /// <param name="includeErrorDetail"><c>true</c> to include exception messages in the error; <c>false</c> otherwise</param>
        public HttpError(ModelStateDictionary modelState, bool includeErrorDetail)
            : this()
        {
            if (modelState == null)
            {
                throw Error.ArgumentNull("modelState");
            }

            if (modelState.IsValid)
            {
                throw Error.Argument("modelState", SRResources.ValidModelState);
            }

            Message = SRResources.BadRequest;

            HttpError modelStateError = new HttpError();
            foreach (KeyValuePair<string, ModelState> keyModelStatePair in modelState)
            {
                string key = keyModelStatePair.Key;
                ModelErrorCollection errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    IEnumerable<string> errorMessages = errors.Select(error =>
                    {
                        if (includeErrorDetail && error.Exception != null)
                        {
                            return error.Exception.Message;
                        }
                        else
                        {
                            return String.IsNullOrEmpty(error.ErrorMessage) ? SRResources.ErrorOccurred : error.ErrorMessage;
                        }
                    }).ToArray();
                    modelStateError.Add(key, errorMessages);
                }
            }

            Add(HttpErrorKeys.ModelStateKey, modelStateError);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class containing error message <paramref name="message"/> 
        /// and error message detail <paramref name="messageDetail"/>.
        /// </summary>
        /// <param name="message">The error message to associate with this instance.</param>
        /// <param name="messageDetail">The error message detail to associate with this instance.</param>
        internal HttpError(string message, string messageDetail)
            : this(message)
        {
            if (messageDetail == null)
            {
                throw Error.ArgumentNull("message");
            }

            Add(HttpErrorKeys.MessageDetailKey, messageDetail);
        }

        /// <summary>
        /// The high-level, user-visible message explaining the cause of the error. Information carried in this field 
        /// should be considered public in that it will go over the wire regardless of the <see cref="IncludeErrorDetailPolicy"/>. 
        /// As a result care should be taken not to disclose sensitive information about the server or the application.
        /// </summary>
        public string Message
        {
            get { return GetPropertyValue<String>(HttpErrorKeys.MessageKey); }
            set { this[HttpErrorKeys.MessageKey] = value; }
        }

        /// <summary>
        /// The <see cref="ModelState"/> containing information about the errors that occurred during model binding.
        /// </summary>
        /// <remarks>
        /// The inclusion of <see cref="System.Exception"/> information carried in the <see cref="ModelState"/> is
        /// controlled by the <see cref="IncludeErrorDetailPolicy"/>. All other information in the <see cref="ModelState"/>
        /// should be considered public in that it will go over the wire. As a result care should be taken not to 
        /// disclose sensitive information about the server or the application.
        /// </remarks>
        public HttpError ModelState
        {
            get { return GetPropertyValue<HttpError>(HttpErrorKeys.ModelStateKey); }
        }

        /// <summary>
        /// A detailed description of the error intended for the developer to understand exactly what failed.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the <see cref="IncludeErrorDetailPolicy"/>. The 
        /// field is expected to contain information about the server or the application that should not 
        /// be disclosed broadly.
        /// </remarks>
        public string MessageDetail
        {
            get { return GetPropertyValue<String>(HttpErrorKeys.MessageDetailKey); }
            set { this[HttpErrorKeys.MessageDetailKey] = value; }
        }

        /// <summary>
        /// The message of the <see cref="System.Exception"/> if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the <see cref="IncludeErrorDetailPolicy"/>. The 
        /// field is expected to contain information about the server or the application that should not 
        /// be disclosed broadly.
        /// </remarks>
        public string ExceptionMessage
        {
            get { return GetPropertyValue<String>(HttpErrorKeys.ExceptionMessageKey); }
            set { this[HttpErrorKeys.ExceptionMessageKey] = value; }
        }

        /// <summary>
        /// The type of the <see cref="System.Exception"/> if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the <see cref="IncludeErrorDetailPolicy"/>. The 
        /// field is expected to contain information about the server or the application that should not 
        /// be disclosed broadly.
        /// </remarks>
        public string ExceptionType
        {
            get { return GetPropertyValue<String>(HttpErrorKeys.ExceptionTypeKey); }
            set { this[HttpErrorKeys.ExceptionTypeKey] = value; }
        }

        /// <summary>
        /// The stack trace information associated with this instance if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the <see cref="IncludeErrorDetailPolicy"/>. The 
        /// field is expected to contain information about the server or the application that should not 
        /// be disclosed broadly.
        /// </remarks>
        public string StackTrace
        {
            get { return GetPropertyValue<String>(HttpErrorKeys.StackTraceKey); }
            set { this[HttpErrorKeys.StackTraceKey] = value; }
        }

        /// <summary>
        /// The inner <see cref="System.Exception"/> associated with this instance if available.
        /// </summary>
        /// <remarks>
        /// The inclusion of this field is controlled by the <see cref="IncludeErrorDetailPolicy"/>. The 
        /// field is expected to contain information about the server or the application that should not 
        /// be disclosed broadly.
        /// </remarks>
        public HttpError InnerException
        {
            get { return GetPropertyValue<HttpError>(HttpErrorKeys.InnerExceptionKey); }
        }

        /// <summary>
        /// Gets a particular property value from this error instance.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="key">The name of the error property.</param>
        /// <returns>The value of the error property.</returns>
        public TValue GetPropertyValue<TValue>(string key)
        {
            TValue value;
            if (this.TryGetValue(key, out value))
            {
                return value;
            }
            return default(TValue);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string key = XmlConvert.DecodeName(reader.LocalName);
                string value = reader.ReadInnerXml();

                this.Add(key, value);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<string, object> keyValuePair in this)
            {
                string key = keyValuePair.Key;
                object value = keyValuePair.Value;
                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    HttpError innerError = value as HttpError;
                    if (innerError == null)
                    {
                        writer.WriteValue(value);
                    }
                    else
                    {
                        ((IXmlSerializable)innerError).WriteXml(writer);
                    }
                }
                writer.WriteEndElement();
            }
        }
    }
}
