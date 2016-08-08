// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages.Deployment
{
    internal static class AssemblyUtils
    {
        // Copied from AssemblyRefs.cs
        private const string SharedLibPublicKey = "31bf3856ad364e35";

        internal static readonly AssemblyName ThisAssemblyName = new AssemblyName(typeof(AssemblyUtils).Assembly.FullName);
        internal static readonly Version WebPagesV1Version = new Version(1, 0, 0, 0);
        private static readonly string _binFileName = Path.GetFileName(ThisAssemblyName.Name) + ".dll";

        // Special case MWI because it does not share the same assembly version as the rest of WebPages.
        private static readonly Version _mwiVersion = new AssemblyName(typeof(InfrastructureHelper).Assembly.FullName).Version;

        private static readonly AssemblyName _mwiAssemblyName = GetFullName("Microsoft.Web.Infrastructure", _mwiVersion);

        private static readonly AssemblyName[] _version1AssemblyList = new[]
        {
            _mwiAssemblyName,
            GetFullName("System.Web.Razor", WebPagesV1Version),
            GetFullName("System.Web.Helpers", WebPagesV1Version),
            GetFullName("System.Web.WebPages", WebPagesV1Version),
            GetFullName("System.Web.WebPages.Administration", WebPagesV1Version),
            GetFullName("System.Web.WebPages.Razor", WebPagesV1Version),
            GetFullName("WebMatrix.Data", WebPagesV1Version),
            GetFullName("WebMatrix.WebData", WebPagesV1Version)
        };

        private static readonly AssemblyName[] _versionCurrentAssemblyList = new[]
        {
            _mwiAssemblyName,
            GetFullName("System.Web.Razor", ThisAssemblyName.Version),
            GetFullName("System.Web.Helpers", ThisAssemblyName.Version),
            GetFullName("System.Web.WebPages", ThisAssemblyName.Version),
            GetFullName("System.Web.WebPages.Administration", ThisAssemblyName.Version),
            GetFullName("System.Web.WebPages.Razor", ThisAssemblyName.Version),
            GetFullName("WebMatrix.Data", ThisAssemblyName.Version),
            GetFullName("WebMatrix.WebData", ThisAssemblyName.Version)
        };

        internal static Version GetMaxWebPagesVersion()
        {
            return GetMaxWebPagesVersion(GetLoadedAssemblies());
        }

        internal static Version GetMaxWebPagesVersion(IEnumerable<AssemblyName> loadedAssemblies)
        {
            return GetWebPagesAssemblies(loadedAssemblies).Max(c => c.Version);
        }

        internal static bool IsVersionAvailable(Version version)
        {
            return IsVersionAvailable(GetLoadedAssemblies(), version);
        }

        internal static bool IsVersionAvailable(IEnumerable<AssemblyName> loadedAssemblies, Version version)
        {
            return GetWebPagesAssemblies(loadedAssemblies).Any(c => c.Version == version);
        }

        private static IEnumerable<AssemblyName> GetWebPagesAssemblies(IEnumerable<AssemblyName> loadedAssemblies)
        {
            return (from otherName in loadedAssemblies
                    where NamesMatch(ThisAssemblyName, otherName, matchVersion: false)
                    select otherName);
        }

        /// <summary>
        /// Returns the version of a System.Web.WebPages.Deployment.dll if it is present in the bin and matches the name and 
        /// public key token of the current assembly.
        /// </summary>
        /// <returns>Version from bin if present, null otherwise.</returns>
        internal static Version GetVersionFromBin(string binDirectory, IFileSystem fileSystem, Func<string, AssemblyName> getAssemblyNameThunk = null)
        {
            // If a version of the assembly is present both in the bin and the GAC, the GAC would win.
            // To work around this, we'll look for a physical file on disk with the same name as the current assembly and load it to determine the version.
            // Determine if the Deployment assembly is present in the bin
            var assemblyInBin = Path.Combine(binDirectory, _binFileName);
            if (fileSystem.FileExists(assemblyInBin))
            {
                try
                {
                    getAssemblyNameThunk = getAssemblyNameThunk ?? AssemblyName.GetAssemblyName;
                    AssemblyName assemblyName = getAssemblyNameThunk(assemblyInBin);
                    if (NamesMatch(ThisAssemblyName, assemblyName, matchVersion: false))
                    {
                        return assemblyName.Version;
                    }
                }
                catch (BadImageFormatException)
                {
                    // Do nothing. 
                }
                catch (SecurityException)
                {
                    // Do nothing
                }
                catch (FileLoadException)
                {
                    // Do nothing.
                }
            }
            return null;
        }

        internal static bool NamesMatch(AssemblyName left, AssemblyName right, bool matchVersion)
        {
            return Equals(left.Name, right.Name) &&
                   Equals(left.CultureInfo, right.CultureInfo) &&
                   Enumerable.SequenceEqual(left.GetPublicKeyToken(), right.GetPublicKeyToken()) &&
                   (!matchVersion || Equals(left.Version, right.Version));
        }

        internal static IEnumerable<AssemblyName> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(GetAssemblyName)
                .ToList();
        }

        internal static IEnumerable<AssemblyName> GetAssembliesForVersion(Version version)
        {
            if (version == WebPagesV1Version)
            {
                return _version1AssemblyList;
            }
            return _versionCurrentAssemblyList;
        }

        private static AssemblyName GetAssemblyName(Assembly assembly)
        {
            return new AssemblyName(assembly.FullName);
        }

        private static AssemblyName GetFullName(string name, Version version, string publicKeyToken)
        {
            return new AssemblyName(String.Format(CultureInfo.InvariantCulture,
                                                  "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}",
                                                  name, version, publicKeyToken));
        }

        internal static AssemblyName GetFullName(string name, Version version)
        {
            return GetFullName(name, version, SharedLibPublicKey);
        }

        public static IDictionary<string, Version> GetAssembliesMatchingOtherVersions(IDictionary<string, IEnumerable<string>> references)
        {
            var webPagesAssemblies = AssemblyUtils.GetAssembliesForVersion(AssemblyUtils.ThisAssemblyName.Version);
            if (references == null || webPagesAssemblies == null || !webPagesAssemblies.Any())
            {
                return new Dictionary<string, Version>(0);
            }

            var matchingVersions = from item in references
                                   let matchedVersion = GetMatchingVersion(webPagesAssemblies, item.Value)
                                   where matchedVersion != null
                                   select new KeyValuePair<string, Version>(item.Key, matchedVersion);
            return matchingVersions.ToDictionary(k => k.Key, k => k.Value);
        }

        private static Version GetMatchingVersion(IEnumerable<AssemblyName> webPagesAssemblies, IEnumerable<string> references)
        {
            // Return assemblies that match in name but not in version.
            var matchingVersions = from webPagesAssembly in webPagesAssemblies
                                   from referenceName in references
                                   let referencedAssembly = new AssemblyName(referenceName)
                                   where AssemblyUtils.NamesMatch(webPagesAssembly, referencedAssembly, matchVersion: false) && webPagesAssembly.Version != referencedAssembly.Version
                                   select referencedAssembly.Version;
            return matchingVersions.FirstOrDefault();
        }
    }
}
