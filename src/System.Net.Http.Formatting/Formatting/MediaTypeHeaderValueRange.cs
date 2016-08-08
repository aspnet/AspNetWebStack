// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting
{
    internal enum MediaTypeHeaderValueRange
    {
        /// <summary>
        /// Not a media type range
        /// </summary>
        None = 0,

        /// <summary>
        /// A subtype media range, e.g. "application/*".
        /// </summary>
        SubtypeMediaRange,

        /// <summary>
        /// An all media range, e.g. "*/*".
        /// </summary>
        AllMediaRange,
    }
}
