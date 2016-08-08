// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.WebPages
{
    public sealed class DisplayModeProvider
    {
        public static readonly string MobileDisplayModeId = "Mobile";
        public static readonly string DefaultDisplayModeId = String.Empty;
        private static readonly object _displayModeKey = new object();
        private static readonly DisplayModeProvider _instance = new DisplayModeProvider();

        private readonly List<IDisplayMode> _displayModes = new List<IDisplayMode>
        {
            new DefaultDisplayMode(MobileDisplayModeId)
            {
                ContextCondition = context => context.GetOverriddenBrowser().IsMobileDevice
            },
            new DefaultDisplayMode()
        };

        internal DisplayModeProvider()
        {
            // The type is a psuedo-singleton. A user would gain nothing from constructing it since we won't use anything but DisplayModeProvider.Instance internally.
        }

        /// <summary>
        /// Restricts the search for Display Info to Display Modes either equal to or following the current
        /// Display Mode in Modes. For example, a page being rendered in the Default Display Mode will not
        /// display Mobile partial views in order to achieve a consistent look and feel.
        /// </summary>
        public bool RequireConsistentDisplayMode { get; set; }

        public static DisplayModeProvider Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// All Display Modes that are available to handle a request.
        /// </summary>
        public IList<IDisplayMode> Modes
        {
            get { return _displayModes; }
        }

        private int FindFirstAvailableDisplayMode(IDisplayMode currentDisplayMode, bool requireConsistentDisplayMode)
        {
            if (requireConsistentDisplayMode && currentDisplayMode != null)
            {
                int first = _displayModes.IndexOf(currentDisplayMode);
                return (first >= 0) ? first : _displayModes.Count;
            }
            return 0;
        }

        /// <summary>
        /// Returns any IDisplayMode that can handle the given request.
        /// </summary>
        public IEnumerable<IDisplayMode> GetAvailableDisplayModesForContext(HttpContextBase httpContext, IDisplayMode currentDisplayMode)
        {
            return GetAvailableDisplayModesForContext(httpContext, currentDisplayMode, RequireConsistentDisplayMode);
        }

        internal IEnumerable<IDisplayMode> GetAvailableDisplayModesForContext(HttpContextBase httpContext, IDisplayMode currentDisplayMode, bool requireConsistentDisplayMode)
        {
            int first = FindFirstAvailableDisplayMode(currentDisplayMode, requireConsistentDisplayMode);
            for (int i = first; i < _displayModes.Count; i++)
            {
                IDisplayMode mode = _displayModes[i];
                if (mode.CanHandleContext(httpContext))
                {
                    yield return mode;
                }
            }
        }

        /// <summary>
        /// Returns DisplayInfo from the first IDisplayMode in Modes that can handle the given request and locate the virtual path.
        /// If currentDisplayMode is not null and RequireConsistentDisplayMode is set to true the search for DisplayInfo will only
        /// start with the currentDisplayMode.
        /// </summary>
        public DisplayInfo GetDisplayInfoForVirtualPath(string virtualPath, HttpContextBase httpContext, Func<string, bool> virtualPathExists, IDisplayMode currentDisplayMode)
        {
            return GetDisplayInfoForVirtualPath(virtualPath, httpContext, virtualPathExists, currentDisplayMode, RequireConsistentDisplayMode);
        }

        internal DisplayInfo GetDisplayInfoForVirtualPath(string virtualPath, HttpContextBase httpContext, Func<string, bool> virtualPathExists, IDisplayMode currentDisplayMode,
                                                          bool requireConsistentDisplayMode)
        {
            // Performance sensitive
            int first = FindFirstAvailableDisplayMode(currentDisplayMode, requireConsistentDisplayMode);
            for (int i = first; i < _displayModes.Count; i++)
            {
                IDisplayMode mode = _displayModes[i];
                if (mode.CanHandleContext(httpContext))
                {
                    DisplayInfo info = mode.GetDisplayInfo(httpContext, virtualPath, virtualPathExists);
                    if (info != null)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        internal static IDisplayMode GetDisplayMode(HttpContextBase context)
        {
            return context != null ? context.Items[_displayModeKey] as IDisplayMode : null;
        }

        internal static void SetDisplayMode(HttpContextBase context, IDisplayMode displayMode)
        {
            if (context != null)
            {
                context.Items[_displayModeKey] = displayMode;
            }
        }
    }
}
