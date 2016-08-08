// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http
{
    /// <summary>
    /// The <see cref="RouteParameter"/> class can be used to indicate properties about a route parameter (the literals and placeholders 
    /// located within segments of a <see cref="M:IHttpRoute.RouteTemplate"/>). 
    /// It can for example be used to indicate that a route parameter is optional.
    /// </summary>
    public sealed class RouteParameter
    {
        /// <summary>
        /// Optional Parameter
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly RouteParameter Optional = new RouteParameter();

        // singleton constructor
        private RouteParameter()
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Empty;
        }
    }
}
