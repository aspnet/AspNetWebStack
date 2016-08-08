// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet;

namespace System.Web.WebPages.Administration.PackageManager
{
    internal static class PackageExtensions
    {
        public static string GetDisplayName(this IPackage package)
        {
            string name = String.IsNullOrEmpty(package.Title) ? package.Id : package.Title;
            return String.Concat(name, ' ', package.Version);
        }
    }
}
