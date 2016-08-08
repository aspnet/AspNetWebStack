// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.WebPages.ApplicationParts;

namespace System.Web.WebPages.Test
{
    public abstract class TestResourceAssembly : IResourceAssembly
    {
        public abstract string Name { get; }

        public abstract Stream GetManifestResourceStream(string name);

        public abstract IEnumerable<string> GetManifestResourceNames();

        public abstract IEnumerable<Type> GetTypes();
    }
}
