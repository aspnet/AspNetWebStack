// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet;

namespace System.Web.WebPages.Administration.PackageManager
{
    public class WebPackageSource : PackageSource
    {
        public WebPackageSource(string source, string name)
            : base(source, name)
        {
        }

        public bool FilterPreferredPackages { get; set; }

        public override bool Equals(object obj)
        {
            WebPackageSource other = obj as WebPackageSource;
            return base.Equals(other) && FilterPreferredPackages == other.FilterPreferredPackages;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (FilterPreferredPackages ? 1 : 0);
        }
    }
}
