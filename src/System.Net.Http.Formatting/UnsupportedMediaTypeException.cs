// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Defines an exception type for signalling that a request's media type was not supported.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This type is not meant to be serialized")]
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "This type is not meant to be serialized")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "UnsupportedMediaTypeException is only used to propagate the media type back to the server layer")]
    public class UnsupportedMediaTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedMediaTypeException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="mediaType">The unsupported media type.</param>
        public UnsupportedMediaTypeException(string message, MediaTypeHeaderValue mediaType)
            : base(message)
        {
            if (mediaType == null)
            {
                throw Error.ArgumentNull("mediaType");
            }

            MediaType = mediaType;
        }

        public MediaTypeHeaderValue MediaType { get; private set; }
    }
}
