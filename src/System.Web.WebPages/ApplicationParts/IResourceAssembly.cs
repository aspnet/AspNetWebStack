// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace System.Web.WebPages.ApplicationParts
{
    // For unit testing purpose since Assembly is not Moqable
    internal interface IResourceAssembly
    {
        string Name { get; }
        Stream GetManifestResourceStream(string name);
        IEnumerable<string> GetManifestResourceNames();
        IEnumerable<Type> GetTypes();
    }
}
