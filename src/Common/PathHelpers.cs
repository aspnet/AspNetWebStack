// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace System.Web
{
    /// <summary>
    /// Helpers for working with IO paths.
    /// </summary>
    internal static class PathHelpers
    {
        /// <summary>
        /// Returns whether the path has the specified file extension.
        /// </summary>
        public static bool EndsWithExtension(string path, string extension)
        {
            Contract.Assert(path != null);
            Contract.Assert(extension != null && extension.Length > 0);

            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                int extensionLength = extension.Length;
                int pathLength = path.Length;
                return (pathLength > extensionLength && path[pathLength - extensionLength - 1] == '.');
            }
            return false;
        }
    }
}
