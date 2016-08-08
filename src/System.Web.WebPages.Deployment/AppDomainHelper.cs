// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Web.WebPages.Deployment
{
    internal static class AppDomainHelper
    {
        public static IDictionary<string, IEnumerable<string>> GetBinAssemblyReferences(string appPath, string configPath)
        {
            string binDirectory = Path.Combine(appPath, "bin");
            if (!Directory.Exists(binDirectory))
            {
                return null;
            }

            AppDomain appDomain = null;
            try
            {
                var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = appPath,
                    ConfigurationFile = configPath,
                    PrivateBinPath = binDirectory,
                };
                appDomain = AppDomain.CreateDomain(typeof(AppDomainHelper).Namespace, AppDomain.CurrentDomain.Evidence, appDomainSetup);

                var type = typeof(RemoteAssemblyLoader);
                var instance = (RemoteAssemblyLoader)appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

                return Directory.EnumerateFiles(binDirectory, "*.dll")
                    .ToDictionary(assemblyPath => assemblyPath,
                                  assemblyPath => instance.GetReferences(assemblyPath));
            }
            finally
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }

        private sealed class RemoteAssemblyLoader : MarshalByRefObject
        {
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method needs to be instance level for cross domain invocation"),
             SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom",
                 Justification = "We want to load this specific assembly.")]
            public IEnumerable<string> GetReferences(string assemblyPath)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                return assembly.GetReferencedAssemblies()
                    .Select(asmName => Assembly.Load(asmName.FullName).FullName)
                    .Concat(new[] { assembly.FullName })
                    .ToArray();
            }
        }
    }
}
