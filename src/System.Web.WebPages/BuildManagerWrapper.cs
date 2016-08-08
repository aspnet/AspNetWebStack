// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Util;
using System.Xml.Linq;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages
{
    /// <summary>
    /// Wraps the caching and instantiation of paths of the BuildManager. 
    /// In case of precompiled non-updateable sites, the only way to verify if a file exists is to call BuildManager.GetObjectFactory. However this method is less performant than
    /// VirtualPathProvider.FileExists which is used for all other scenarios. In this class, we optimize for the first scenario by storing the results of GetObjectFactory for a 
    /// long duration.
    /// </summary>
    internal sealed class BuildManagerWrapper : IVirtualPathFactory
    {
        internal static readonly Guid KeyGuid = Guid.NewGuid();
        private static readonly TimeSpan _objectFactoryCacheDuration = TimeSpan.FromMinutes(1);
        private readonly IVirtualPathUtility _virtualPathUtility;
        private readonly Func<VirtualPathProvider> _vppFunc;
        private readonly bool _isPrecompiled;
        private readonly FileExistenceCache _vppCache;
        private IEnumerable<string> _supportedExtensions;

        public BuildManagerWrapper()
            : this(() => HostingEnvironment.VirtualPathProvider, new VirtualPathUtilityWrapper())
        {
        }

        public BuildManagerWrapper(VirtualPathProvider vpp, IVirtualPathUtility virtualPathUtility)
            : this(() => vpp, virtualPathUtility)
        {
            Contract.Assert(vpp != null);
        }

        public BuildManagerWrapper(Func<VirtualPathProvider> vppFunc, IVirtualPathUtility virtualPathUtility)
        {
            Contract.Assert(vppFunc != null);
            Contract.Assert(virtualPathUtility != null);

            _vppFunc = vppFunc;
            _virtualPathUtility = virtualPathUtility;
            _isPrecompiled = IsNonUpdatablePrecompiledApp();
            if (!_isPrecompiled)
            {
                _vppCache = new FileExistenceCache(vppFunc);
            }
        }

        public IEnumerable<string> SupportedExtensions
        {
            get { return _supportedExtensions ?? WebPageHttpHandler.GetRegisteredExtensions(); }
            set { _supportedExtensions = value; }
        }

        /// <summary>
        /// Determines if a page exists in the website. 
        /// This method switches between a long duration cache or a short duration FileExistenceCache depending on whether the site is precompiled. 
        /// This is an optimization because BuildManager.GetObjectFactory is comparably slower than performing VirtualPathFactory.Exists
        /// </summary>
        public bool Exists(string virtualPath)
        {
            if (_isPrecompiled)
            {
                return ExistsInPrecompiledSite(virtualPath);
            }
            return ExistsInVpp(virtualPath);
        }

        internal bool IsNonUpdatablePrecompiledApp()
        {
            VirtualPathProvider vpp = _vppFunc();
            // VirtualPathProvider currently null in some test scenarios e.g. PreApplicationStartCodeTest.StartTest
            if (vpp == null)
            {
                return false;
            }
            return IsNonUpdateablePrecompiledApp(vpp, _virtualPathUtility);
        }

        /// <summary>
        /// An app's is precompiled for our purposes if 
        /// (a) it has a PreCompiledApp.config file in the site root, 
        /// (b) The PreCompiledApp.config says that the app is not Updatable.
        /// </summary>
        /// <remarks>
        /// This code is based on System.Web.DynamicData.Misc.IsNonUpdatablePrecompiledAppNoCache (DynamicData)
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "We want to replicate the behavior of BuildManager which catches all exceptions.")]
        internal static bool IsNonUpdateablePrecompiledApp(VirtualPathProvider vpp, IVirtualPathUtility virtualPathUtility)
        {
            var virtualPath = virtualPathUtility.ToAbsolute("~/PrecompiledApp.config");
            if (!vpp.FileExists(virtualPath))
            {
                return false;
            }

            XDocument document;
            using (Stream stream = vpp.GetFile(virtualPath).Open())
            {
                try
                {
                    document = XDocument.Load(stream);
                }
                catch
                {
                    // If we are unable to load the file, ignore it. The BuildManager behaves identically.
                    return false;
                }
            }

            if (document.Root == null || !document.Root.Name.LocalName.Equals("precompiledApp", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var updatableAttribute = document.Root.Attribute("updatable");
            if (updatableAttribute != null)
            {
                bool result;
                return Boolean.TryParse(updatableAttribute.Value, out result) && (result == false);
            }
            return false;
        }

        private bool ExistsInPrecompiledSite(string virtualPath)
        {
            var key = GetKeyFromVirtualPath(virtualPath);

            // We assume that the key is unique enough to avoid collisions.
            var buildManagerResult = (BuildManagerResult)HttpRuntime.Cache.Get(key);
            if (buildManagerResult == null)
            {
                // For precompiled apps, we cache the ObjectFactory and use it in the CreateInstance method. 
                var objectFactory = GetObjectFactory(virtualPath);
                buildManagerResult = new BuildManagerResult { ObjectFactory = objectFactory, Exists = objectFactory != null };
                // Cache the result with a sliding expiration for a long duration. 
                HttpRuntime.Cache.Add(key, buildManagerResult, null, Cache.NoAbsoluteExpiration, _objectFactoryCacheDuration, CacheItemPriority.Low, null);
            }
            return buildManagerResult.Exists;
        }

        /// <summary>
        /// Determines if a site exists in the VirtualPathProvider.
        /// Results of hits are cached for a very short amount of time in the FileExistenceCache.
        /// </summary>
        private bool ExistsInVpp(string virtualPath)
        {
            Debug.Assert(_vppCache != null);
            return _vppCache.FileExists(virtualPath);
        }

        /// <summary>
        /// Determines if an ObjectFactory exists for the virtualPath. 
        /// The BuildManager complains if we pass in extensions that aren't registered for compilation. So we ensure that the virtual path is not 
        /// extensionless and that it is one of the extension
        /// </summary>
        private IWebObjectFactory GetObjectFactory(string virtualPath)
        {
            if (IsPathExtensionSupported(virtualPath))
            {
                return BuildManager.GetObjectFactory(virtualPath, throwIfNotFound: false);
            }
            return null;
        }

        public object CreateInstance(string virtualPath)
        {
            return CreateInstanceOfType<object>(virtualPath);
        }

        public T CreateInstanceOfType<T>(string virtualPath) where T : class
        {
            if (_isPrecompiled)
            {
                var buildManagerResult = (BuildManagerResult)HttpRuntime.Cache.Get(GetKeyFromVirtualPath(virtualPath));
                // The cache could have evicted our results. In this case, we'll simply fall through to CreateInstanceFromVirtualPath
                if (buildManagerResult != null)
                {
                    Debug.Assert(buildManagerResult.Exists && buildManagerResult.ObjectFactory != null, "This method must only be called if the file exists.");
                    return buildManagerResult.ObjectFactory.CreateInstance() as T;
                }
            }

            return (T)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(T));
        }

        /// <summary>
        /// Determines if the extension is one of the extensions registered with WebPageHttpHandler. 
        /// </summary>
        public bool IsPathExtensionSupported(string virtualPath)
        {
            string extension = PathUtil.GetExtension(virtualPath);
            return !String.IsNullOrEmpty(extension)
                   && SupportedExtensions.Contains(extension.Substring(1), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a reasonably unique key for a given virtual path by concatenating it with a Guid.
        /// </summary>
        private static string GetKeyFromVirtualPath(string virtualPath)
        {
            return KeyGuid.ToString() + "_" + virtualPath;
        }

        private class BuildManagerResult
        {
            public bool Exists { get; set; }

            public IWebObjectFactory ObjectFactory { get; set; }
        }
    }
}
