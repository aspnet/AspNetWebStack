// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Compilation;

namespace System.Web.WebPages.Deployment
{
    internal sealed class BuildManagerWrapper : IBuildManager
    {
        /// <summary>
        /// Reads a special cached file from %WindDir%\Microsoft.NET\Framework\vx.x\ASP.NET Temporary Files\&lt;x&gt;\&lt;y&gt;\UserCache that is 
        /// available across AppDomain recycles.
        /// </summary>
        public Stream ReadCachedFile(string path)
        {
            return BuildManager.ReadCachedFile(path);
        }

        /// <summary>
        /// Creates or opens a special cached file that is created under  %WindDir%\Microsoft.NET\Framework\vx.x\ASP.NET Temporary Files\&lt;x&gt;\&lt;y&gt;\UserCache that is 
        /// available across AppDomain recycles.
        /// </summary>
        public Stream CreateCachedFile(string path)
        {
            return BuildManager.CreateCachedFile(path);
        }
    }
}
