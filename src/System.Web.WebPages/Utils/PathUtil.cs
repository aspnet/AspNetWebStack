// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages
{
    internal static class PathUtil
    {
        /// <summary>
        /// Path.GetExtension performs a CheckInvalidPathChars(path) which blows up for paths that do not translate to valid physical paths but are valid paths in ASP.NET
        /// This method is a near clone of Path.GetExtension without a call to CheckInvalidPathChars(path);
        /// </summary>
        internal static string GetExtension(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }
            int current = path.Length;
            while (--current >= 0)
            {
                char ch = path[current];
                if (ch == '.')
                {
                    if (current == path.Length - 1)
                    {
                        break;
                    }
                    return path.Substring(current);
                }
                if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                {
                    break;
                }
            }
            return String.Empty;
        }

        internal static bool IsWithinAppRoot(string appDomainAppVirtualPath, string virtualPath)
        {
            if (appDomainAppVirtualPath == null)
            {
                // If the runtime has not been initialized, just return true.
                return true;
            }

            var absPath = virtualPath;
            if (!VirtualPathUtility.IsAbsolute(absPath))
            {
                absPath = VirtualPathUtility.ToAbsolute(absPath);
            }
            // We need to call this overload because it returns null if the path is not within the application root.
            // The overload calls into MakeVirtualPathAppRelative(string virtualPath, string applicationPath, bool nullIfNotInApp), with 
            // nullIfNotInApp set to true.
            return VirtualPathUtility.ToAppRelative(absPath, appDomainAppVirtualPath) != null;
        }

        /// <summary>
        /// Determines true if the path is simply "MyPath", and not app-relative "~/MyPath" or absolute "/MyApp/MyPath" or relative "../Test/MyPath"
        /// </summary>
        /// <returns>True if it is a not app-relative, absolute or relative.</returns>
        internal static bool IsSimpleName(string path)
        {
            if (VirtualPathUtility.IsAbsolute(path) || VirtualPathUtility.IsAppRelative(path))
            {
                return false;
            }
            if (path.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
