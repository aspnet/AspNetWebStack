// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Internal.Web.Utils
{
    internal interface IFileSystem
    {
        bool FileExists(string path);

        Stream ReadFile(string path);

        Stream OpenFile(string path);

        IEnumerable<string> EnumerateFiles(string root);
    }
}
