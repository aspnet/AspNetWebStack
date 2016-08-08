// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http
{
    /// <summary>
    /// An exception that allows for a given <see cref="HttpResponseMessage"/>
    /// to be returned to the client.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This type is not meant to be serialized")]
    [SuppressMessage("Microsoft.Usage", "CA2240:Implement ISerializable correctly", Justification = "This type has no serializable state")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "HttpResponseException is not a real exception and is just an easy way to return HttpResponseMessage")]
    public class HttpResponseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Instance is disposed elsewhere")]
        public HttpResponseException(HttpStatusCode statusCode)
            : this(new HttpResponseMessage(statusCode))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="response">The response message.</param>
        public HttpResponseException(HttpResponseMessage response)
            : base(SRResources.HttpResponseExceptionMessage)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            Response = response;
        }

        /// <summary>
        /// Gets the <see cref="HttpResponseMessage"/> to return to the client.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }
    }
}
