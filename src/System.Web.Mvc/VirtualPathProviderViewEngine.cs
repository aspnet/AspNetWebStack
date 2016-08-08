// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc.Properties;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public abstract class VirtualPathProviderViewEngine : IViewEngine
    {
        // format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{areaName}:"
        private const string CacheKeyFormat = ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:";
        private const string CacheKeyPrefixMaster = "Master";
        private const string CacheKeyPrefixPartial = "Partial";
        private const string CacheKeyPrefixView = "View";
        private static readonly string[] _emptyLocations = new string[0];
        private DisplayModeProvider _displayModeProvider;

        private Func<VirtualPathProvider> _vppFunc = () => HostingEnvironment.VirtualPathProvider;
        internal Func<string, string> GetExtensionThunk = VirtualPathUtility.GetExtension;
        private IViewLocationCache _viewLocationCache;

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] AreaMasterLocationFormats { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] AreaPartialViewLocationFormats { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] AreaViewLocationFormats { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] FileExtensions { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] MasterLocationFormats { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] PartialViewLocationFormats { get; set; }

        // Neither DefaultViewLocationCache.Null nor a DefaultViewLocationCache instance maintain internal state. Fine
        // if multiple threads race to initialize _viewLocationCache.
        public IViewLocationCache ViewLocationCache
        {
            get
            {
                if (_viewLocationCache == null)
                {
                    if (HttpContext.Current == null || HttpContext.Current.IsDebuggingEnabled)
                    {
                        _viewLocationCache = DefaultViewLocationCache.Null;
                    }
                    else
                    {
                        _viewLocationCache = new DefaultViewLocationCache();
                    }
                }

                return _viewLocationCache;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _viewLocationCache = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] ViewLocationFormats { get; set; }

        // Likely exists for testing only
        protected VirtualPathProvider VirtualPathProvider
        {
            get { return _vppFunc(); }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _vppFunc = () => value;
            }
        }

        // Provided for testing only; setter used in BuildManagerViewEngine but only for test scenarios
        internal Func<VirtualPathProvider> VirtualPathProviderFunc
        {
            get { return _vppFunc; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _vppFunc = value;
            }
        }

        protected internal DisplayModeProvider DisplayModeProvider
        {
            get { return _displayModeProvider ?? DisplayModeProvider.Instance; }
            set { _displayModeProvider = value; }
        }

        internal virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName)
        {
            return String.Format(CultureInfo.InvariantCulture, CacheKeyFormat,
                                 GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName);
        }

        internal static string AppendDisplayModeToCacheKey(string cacheKey, string displayMode)
        {
            // key format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{areaName}:"
            // so append "{displayMode}:" to the key
            return cacheKey + displayMode + ":";
        }

        protected abstract IView CreatePartialView(ControllerContext controllerContext, string partialPath);

        protected abstract IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath);

        protected virtual bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return VirtualPathProvider.FileExists(virtualPath);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }

            string[] searched;
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            string partialPath = GetPath(controllerContext, PartialViewLocationFormats, AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, CacheKeyPrefixPartial, useCache, out searched);

            if (String.IsNullOrEmpty(partialPath))
            {
                return new ViewEngineResult(searched);
            }

            return new ViewEngineResult(CreatePartialView(controllerContext, partialPath), this);
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewName");
            }

            string[] viewLocationsSearched;
            string[] masterLocationsSearched;

            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            string viewPath = GetPath(controllerContext, ViewLocationFormats, AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, CacheKeyPrefixView, useCache, out viewLocationsSearched);
            string masterPath = GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, CacheKeyPrefixMaster, useCache, out masterLocationsSearched);

            if (String.IsNullOrEmpty(viewPath) || (String.IsNullOrEmpty(masterPath) && !String.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(viewLocationsSearched.Union(masterLocationsSearched));
            }

            return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
        }

        private string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
        {
            searchedLocations = _emptyLocations;

            if (String.IsNullOrEmpty(name))
            {
                return String.Empty;
            }

            string areaName = AreaHelpers.GetAreaName(controllerContext.RouteData);
            bool usingAreas = !String.IsNullOrEmpty(areaName);
            List<ViewLocation> viewLocations = GetViewLocations(locations, (usingAreas) ? areaLocations : null);

            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                  MvcResources.Common_PropertyCannotBeNullOrEmpty, locationsPropertyName));
            }

            bool nameRepresentsPath = IsSpecificPath(name);
            string cacheKey = CreateCacheKey(cacheKeyPrefix, name, (nameRepresentsPath) ? String.Empty : controllerName, areaName);

            if (useCache)
            {
                // Only look at cached display modes that can handle the context.
                IEnumerable<IDisplayMode> possibleDisplayModes = DisplayModeProvider.GetAvailableDisplayModesForContext(controllerContext.HttpContext, controllerContext.DisplayMode);
                foreach (IDisplayMode displayMode in possibleDisplayModes)
                {
                    string cachedLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayMode.DisplayModeId));

                    if (cachedLocation == null)
                    {
                        // If any matching display mode location is not in the cache, fall back to the uncached behavior, which will repopulate all of our caches.
                        return null;
                    }

                    // A non-empty cachedLocation indicates that we have a matching file on disk. Return that result.
                    if (cachedLocation.Length > 0)
                    {
                        if (controllerContext.DisplayMode == null)
                        {
                            controllerContext.DisplayMode = displayMode;
                        }

                        return cachedLocation;
                    }
                    // An empty cachedLocation value indicates that we don't have a matching file on disk. Keep going down the list of possible display modes.
                }

                // GetPath is called again without using the cache.
                return null;
            }
            else
            {
                return nameRepresentsPath
                    ? GetPathFromSpecificName(controllerContext, name, cacheKey, ref searchedLocations)
                    : GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, cacheKey, ref searchedLocations);
            }
        }

        private string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string cacheKey, ref string[] searchedLocations)
        {
            string result = String.Empty;
            searchedLocations = new string[locations.Count];

            for (int i = 0; i < locations.Count; i++)
            {
                ViewLocation location = locations[i];
                string virtualPath = location.Format(name, controllerName, areaName);
                DisplayInfo virtualPathDisplayInfo = DisplayModeProvider.GetDisplayInfoForVirtualPath(virtualPath, controllerContext.HttpContext, path => FileExists(controllerContext, path), controllerContext.DisplayMode);

                if (virtualPathDisplayInfo != null)
                {
                    string resolvedVirtualPath = virtualPathDisplayInfo.FilePath;

                    searchedLocations = _emptyLocations;
                    result = resolvedVirtualPath;
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, virtualPathDisplayInfo.DisplayMode.DisplayModeId), result);

                    if (controllerContext.DisplayMode == null)
                    {
                        controllerContext.DisplayMode = virtualPathDisplayInfo.DisplayMode;
                    }

                    // Populate the cache for all other display modes. We want to cache both file system hits and misses so that we can distinguish
                    // in future requests whether a file's status was evicted from the cache (null value) or if the file doesn't exist (empty string).
                    IEnumerable<IDisplayMode> allDisplayModes = DisplayModeProvider.Modes;
                    foreach (IDisplayMode displayMode in allDisplayModes)
                    {
                        if (displayMode.DisplayModeId != virtualPathDisplayInfo.DisplayMode.DisplayModeId)
                        {
                            DisplayInfo displayInfoToCache = displayMode.GetDisplayInfo(controllerContext.HttpContext, virtualPath, virtualPathExists: path => FileExists(controllerContext, path));

                            string cacheValue = String.Empty;
                            if (displayInfoToCache != null && displayInfoToCache.FilePath != null)
                            {
                                cacheValue = displayInfoToCache.FilePath;
                            }
                            ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayMode.DisplayModeId), cacheValue);
                        }
                    }
                    break;
                }

                searchedLocations[i] = virtualPath;
            }

            return result;
        }

        private string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            string result = name;

            if (!(FilePathIsSupported(name) && FileExists(controllerContext, name)))
            {
                result = String.Empty;
                searchedLocations = new[] { name };
            }

            ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
            return result;
        }

        private bool FilePathIsSupported(string virtualPath)
        {
            if (FileExtensions == null)
            {
                // legacy behavior for custom ViewEngine that might not set the FileExtensions property
                return true;
            }
            else
            {
                // get rid of the '.' because the FileExtensions property expects extensions withouth a dot.
                string extension = GetExtensionThunk(virtualPath).TrimStart('.');
                return FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            }
        }

        private static List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
        {
            List<ViewLocation> allLocations = new List<ViewLocation>();

            if (areaViewLocationFormats != null)
            {
                foreach (string areaViewLocationFormat in areaViewLocationFormats)
                {
                    allLocations.Add(new AreaAwareViewLocation(areaViewLocationFormat));
                }
            }

            if (viewLocationFormats != null)
            {
                foreach (string viewLocationFormat in viewLocationFormats)
                {
                    allLocations.Add(new ViewLocation(viewLocationFormat));
                }
            }

            return allLocations;
        }

        private static bool IsSpecificPath(string name)
        {
            char c = name[0];
            return (c == '~' || c == '/');
        }

        public virtual void ReleaseView(ControllerContext controllerContext, IView view)
        {
            IDisposable disposable = view as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private class AreaAwareViewLocation : ViewLocation
        {
            public AreaAwareViewLocation(string virtualPathFormatString)
                : base(virtualPathFormatString)
            {
            }

            public override string Format(string viewName, string controllerName, string areaName)
            {
                return String.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName);
            }
        }

        private class ViewLocation
        {
            protected string _virtualPathFormatString;

            public ViewLocation(string virtualPathFormatString)
            {
                _virtualPathFormatString = virtualPathFormatString;
            }

            public virtual string Format(string viewName, string controllerName, string areaName)
            {
                return String.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName);
            }
        }
    }
}
