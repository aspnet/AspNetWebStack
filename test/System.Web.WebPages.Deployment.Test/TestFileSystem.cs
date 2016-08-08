// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Deployment.Test
{
    public class TestFileSystem : IFileSystem
    {
        private readonly Dictionary<string, MemoryStream> _files = new Dictionary<string, MemoryStream>(StringComparer.OrdinalIgnoreCase);

        public void AddFile(string file, MemoryStream content = null)
        {
            content = content ?? new MemoryStream();
            _files[file] = content;
        }

        public bool FileExists(string path)
        {
            return _files.ContainsKey(path);
        }

        public Stream ReadFile(string path)
        {
            return _files[path];
        }

        public Stream OpenFile(string path)
        {
            MemoryStream memoryStream;
            if (_files.TryGetValue(path, out memoryStream))
            {
                var copiedStream = new MemoryStream(memoryStream.ToArray());
                _files[path] = copiedStream;
            }
            else
            {
                AddFile(path);
            }
            return _files[path];
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return from file in _files.Keys
                   where Path.GetDirectoryName(file).Equals(path, StringComparison.OrdinalIgnoreCase)
                   select file;
        }
    }
}
