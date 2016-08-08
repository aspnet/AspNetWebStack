// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// An abstract base class used to create an association between <see cref="HttpRequestMessage"/> or 
    /// <see cref="HttpResponseMessage"/> instances that have certain characteristics 
    /// and a specific <see cref="MediaTypeHeaderValue"/>. 
    /// </summary>
    public abstract class MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="MediaTypeMapping"/> with the
        /// given <paramref name="mediaType"/> value.
        /// </summary>
        /// <param name="mediaType">
        /// The <see cref="MediaTypeHeaderValue"/> that is associated with <see cref="HttpRequestMessage"/> or 
        /// <see cref="HttpResponseMessage"/> instances that have the given characteristics of the 
        /// <see cref="MediaTypeMapping"/>.
        /// </param>
        protected MediaTypeMapping(MediaTypeHeaderValue mediaType)
        {
            if (mediaType == null)
            {
                throw Error.ArgumentNull("mediaType");
            }

            MediaType = mediaType;
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="MediaTypeMapping"/> with the
        /// given <paramref name="mediaType"/> value.
        /// </summary>
        /// <param name="mediaType">
        /// The <see cref="string"/> that is associated with <see cref="HttpRequestMessage"/> or 
        /// <see cref="HttpResponseMessage"/> instances that have the given characteristics of the 
        /// <see cref="MediaTypeMapping"/>.
        /// </param>
        protected MediaTypeMapping(string mediaType)
        {
            if (String.IsNullOrWhiteSpace(mediaType))
            {
                throw Error.ArgumentNull("mediaType");
            }

            MediaType = new MediaTypeHeaderValue(mediaType);
        }

        /// <summary>
        /// Gets the <see cref="MediaTypeHeaderValue"/> that is associated with <see cref="HttpRequestMessage"/> or 
        /// <see cref="HttpResponseMessage"/> instances that have the given characteristics of the 
        /// <see cref="MediaTypeMapping"/>.
        /// </summary>
        public MediaTypeHeaderValue MediaType { get; private set; }

        /// <summary>
        /// Returns the quality of the match of the <see cref="MediaTypeHeaderValue"/>
        /// associated with <paramref name="request"/>.
        /// </summary>
        /// <param name="request">
        /// The <see cref="HttpRequestMessage"/> to evaluate for the characteristics 
        /// associated with the <see cref="MediaTypeHeaderValue"/>
        /// of the <see cref="MediaTypeMapping"/>.
        /// </param> 
        /// <returns>
        /// The quality of the match. It must be between <c>0.0</c> and <c>1.0</c>.
        /// A value of <c>0.0</c> signifies no match.
        /// A value of <c>1.0</c> signifies a complete match.
        /// </returns>
        public abstract double TryMatchMediaType(HttpRequestMessage request);
    }
}
