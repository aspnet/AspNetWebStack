// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// Enum to indicate whether error details, such as exception messages and stack traces, should be included in error messages.
    /// </summary>
    public enum IncludeErrorDetailPolicy
    {
        /// <summary>
        /// Default to the host specific behavior. This looks at the CustomErrors setting on webhost and
        /// defaults to LocalOnly in selfhost.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Only include error details when responding to a local request.
        /// </summary>
        LocalOnly,

        /// <summary>
        /// Always include error details.
        /// </summary>
        Always,

        /// <summary>
        /// Never include error details.
        /// </summary>
        Never
    }
}
