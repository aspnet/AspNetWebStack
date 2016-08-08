// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace System.Web.WebPages.ApplicationParts
{
    // Implementation of IResourceAssembly over a standard assembly
    internal class ResourceAssembly : IResourceAssembly
    {
        private readonly Assembly _assembly;

        public ResourceAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            _assembly = assembly;
        }

        public string Name
        {
            get
            {
                // Need this for medium trust
                AssemblyName assemblyName = new AssemblyName(_assembly.FullName);
                Debug.Assert(assemblyName != null, "Assembly name should not be null");
                // Use the assembly short name
                return assemblyName.Name;
            }
        }

        public Stream GetManifestResourceStream(string name)
        {
            return _assembly.GetManifestResourceStream(name);
        }

        public IEnumerable<string> GetManifestResourceNames()
        {
            return _assembly.GetManifestResourceNames();
        }

        public IEnumerable<Type> GetTypes()
        {
            return _assembly.GetExportedTypes();
        }

        public override bool Equals(object obj)
        {
            var otherAssembly = obj as ResourceAssembly;
            return otherAssembly != null && otherAssembly._assembly.Equals(_assembly);
        }

        public override int GetHashCode()
        {
            return _assembly.GetHashCode();
        }

        public override String ToString()
        {
            return _assembly.ToString();
        }
    }
}
