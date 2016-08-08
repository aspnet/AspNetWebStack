// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.WebPages.Deployment;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    internal sealed class WebPageRoute
    {
        private static readonly Lazy<bool> _isRootExplicitlyDisabled = new Lazy<bool>(() => WebPagesDeployment.IsExplicitlyDisabled("~/"));
        private bool? _isExplicitlyDisabled;

        internal bool IsExplicitlyDisabled
        {
            get { return _isExplicitlyDisabled ?? _isRootExplicitlyDisabled.Value; }
            set { _isExplicitlyDisabled = value; }
        }

        internal void DoPostResolveRequestCache(HttpContextBase context)
        {
            if (IsExplicitlyDisabled)
            {
                // If the root config is explicitly disabled, do not process the request.
                return;
            }

            // Parse incoming URL (we trim off the first two chars since they're always "~/")
            string requestPath = context.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + context.Request.PathInfo;
            string[] registeredExtensions = WebPageHttpHandler.SupportedExtensions;

            // Check if this request matches a file in the app
            WebPageMatch webpageRouteMatch = MatchRequest(requestPath, registeredExtensions, VirtualPathFactoryManager.InstancePathExists, context, DisplayModeProvider.Instance);
            if (webpageRouteMatch != null)
            {
                // If it matches then save some data for the WebPage's UrlData
                context.Items[typeof(WebPageMatch)] = webpageRouteMatch;

                string virtualPath = "~/" + webpageRouteMatch.MatchedPath;

                // Verify that this path is enabled before remapping
                if (!WebPagesDeployment.IsExplicitlyDisabled(virtualPath))
                {
                    IHttpHandler handler = WebPageHttpHandler.CreateFromVirtualPath(virtualPath);
                    if (handler != null)
                    {
                        SessionStateUtil.SetUpSessionState(context, handler);
                        // Remap to our handler
                        context.RemapHandler(handler);
                    }
                }
            }
            else
            {
                // Bug:904704 If its not a match, but to a supported extension, we want to return a 404 instead of a 403
                string extension = PathUtil.GetExtension(requestPath);
                foreach (string supportedExt in registeredExtensions)
                {
                    if (String.Equals("." + supportedExt, extension, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new HttpException(404, null);
                    }
                }
            }
        }

        private static bool FileExists(string virtualPath, Func<string, bool> virtualPathExists)
        {
            var path = "~/" + virtualPath;
            return virtualPathExists(path);
        }

        internal static WebPageMatch GetWebPageMatch(HttpContextBase context)
        {
            WebPageMatch webPageMatch = (WebPageMatch)context.Items[typeof(WebPageMatch)];
            return webPageMatch;
        }

        private static string GetRouteLevelMatch(string pathValue, string[] supportedExtensions, Func<string, bool> virtualPathExists, HttpContextBase context, DisplayModeProvider displayModeProvider)
        {
            for (int i = 0; i < supportedExtensions.Length; i++)
            {
                string supportedExtension = supportedExtensions[i];

                // For performance, avoid multiple calls to String.Concat
                string virtualPath;
                // Only add the extension if it's not already there
                if (!PathHelpers.EndsWithExtension(pathValue, supportedExtension))
                {
                    virtualPath = "~/" + pathValue + "." + supportedExtension;
                }
                else
                {
                    virtualPath = "~/" + pathValue;
                }
                DisplayInfo virtualPathDisplayInfo = displayModeProvider.GetDisplayInfoForVirtualPath(virtualPath, context, virtualPathExists, currentDisplayMode: null);

                if (virtualPathDisplayInfo != null)
                {
                    // If there's an exact match on disk, return it unless it starts with an underscore
                    if (Path.GetFileName(virtualPathDisplayInfo.FilePath).StartsWith("_", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new HttpException(404, WebPageResources.WebPageRoute_UnderscoreBlocked);
                    }

                    string resolvedVirtualPath = virtualPathDisplayInfo.FilePath;

                    // Matches are not expected to be virtual paths so remove the ~/ from the match
                    if (resolvedVirtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedVirtualPath = resolvedVirtualPath.Remove(0, 2);
                    }

                    DisplayModeProvider.SetDisplayMode(context, virtualPathDisplayInfo.DisplayMode);

                    return resolvedVirtualPath;
                }
            }

            return null;
        }

        internal static WebPageMatch MatchRequest(string pathValue, string[] supportedExtensions, Func<string, bool> virtualPathExists, HttpContextBase context, DisplayModeProvider displayModes)
        {
            string currentLevel = String.Empty;
            string currentPathInfo = pathValue;

            // We can skip the file exists check and normal lookup for empty paths, but we still need to look for default pages
            if (!String.IsNullOrEmpty(pathValue))
            {
                // If the file exists and its not a supported extension, let the request go through
                if (FileExists(pathValue, virtualPathExists))
                {
                    // TODO: Look into switching to RawURL to eliminate the need for this issue
                    bool foundSupportedExtension = false;
                    for (int i = 0; i < supportedExtensions.Length; i++)
                    {
                        string supportedExtension = supportedExtensions[i];
                        if (PathHelpers.EndsWithExtension(pathValue, supportedExtension))
                        {
                            foundSupportedExtension = true;
                            break;
                        }
                    }

                    if (!foundSupportedExtension)
                    {
                        return null;
                    }
                }

                // For each trimmed part of the path try to add a known extension and
                // check if it matches a file in the application.
                currentLevel = pathValue;
                currentPathInfo = String.Empty;
                while (true)
                {
                    // Does the current route level patch any supported extension?
                    string routeLevelMatch = GetRouteLevelMatch(currentLevel, supportedExtensions, virtualPathExists, context, displayModes);
                    if (routeLevelMatch != null)
                    {
                        return new WebPageMatch(routeLevelMatch, currentPathInfo);
                    }

                    // Try to remove the last path segment (e.g. go from /foo/bar to /foo)
                    int indexOfLastSlash = currentLevel.LastIndexOf('/');
                    if (indexOfLastSlash == -1)
                    {
                        // If there are no more slashes, we're done
                        break;
                    }
                    else
                    {
                        // Chop off the last path segment to get to the next one
                        currentLevel = currentLevel.Substring(0, indexOfLastSlash);

                        // And save the path info in case there is a match
                        currentPathInfo = pathValue.Substring(indexOfLastSlash + 1);
                    }
                }
            }

            return MatchDefaultFiles(pathValue, supportedExtensions, virtualPathExists, context, displayModes, currentLevel);
        }

        private static WebPageMatch MatchDefaultFiles(string pathValue, string[] supportedExtensions, Func<string, bool> virtualPathExists, HttpContextBase context, DisplayModeProvider displayModes, string currentLevel)
        {
            // If we haven't found anything yet, now try looking for default.* or index.* at the current url
            currentLevel = pathValue;
            string currentLevelDefault;
            string currentLevelIndex;
            if (String.IsNullOrEmpty(currentLevel))
            {
                currentLevelDefault = "default";
                currentLevelIndex = "index";
            }
            else
            {
                if (currentLevel[currentLevel.Length - 1] != '/')
                {
                    currentLevel += "/";
                }
                currentLevelDefault = currentLevel + "default";
                currentLevelIndex = currentLevel + "index";
            }

            // Does the current route level match any supported extension?
            string defaultMatch = GetRouteLevelMatch(currentLevelDefault, supportedExtensions, virtualPathExists, context, displayModes);
            if (defaultMatch != null)
            {
                return new WebPageMatch(defaultMatch, String.Empty);
            }

            string indexMatch = GetRouteLevelMatch(currentLevelIndex, supportedExtensions, virtualPathExists, context, displayModes);
            if (indexMatch != null)
            {
                return new WebPageMatch(indexMatch, String.Empty);
            }

            return null;
        }
    }
}
