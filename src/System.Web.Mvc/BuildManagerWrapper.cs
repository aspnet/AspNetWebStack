// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.IO;
using System.Web.Compilation;

namespace System.Web.Mvc
{
    internal sealed class BuildManagerWrapper : IBuildManager
    {
        bool IBuildManager.FileExists(string virtualPath)
        {
            return BuildManager.GetObjectFactory(virtualPath, throwIfNotFound: false) != null;
        }

        Type IBuildManager.GetCompiledType(string virtualPath)
        {
            return BuildManager.GetCompiledType(virtualPath);
        }

        ICollection IBuildManager.GetReferencedAssemblies()
        {
            return BuildManager.GetReferencedAssemblies();
        }

        Stream IBuildManager.ReadCachedFile(string fileName)
        {
            return BuildManager.ReadCachedFile(fileName);
        }

        Stream IBuildManager.CreateCachedFile(string fileName)
        {
            return BuildManager.CreateCachedFile(fileName);
        }
    }
}
