// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Model for the Facebook object connection.
    /// </summary>
    /// <typeparam name="T">Type of the connection.</typeparam>
    public class FacebookConnection<T>
    {
        /// <summary>
        /// Gets or sets the connection data.
        /// </summary>
        public T Data { get; set; }
    }
}