// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Web.Hosting;
using System.Web.WebPages.Scope;

namespace Microsoft.Web.Helpers
{
    public static class Themes
    {
        public static string ThemeDirectory
        {
            get { return Implementation.ThemeDirectory; }
        }

        public static string CurrentTheme
        {
            get { return Implementation.CurrentTheme; }
            set { Implementation.CurrentTheme = value; }
        }

        public static string DefaultTheme
        {
            get { return Implementation.DefaultTheme; }
        }

        public static ReadOnlyCollection<string> AvailableThemes
        {
            get { return Implementation.AvailableThemes; }
        }

        private static ThemesImplementation Implementation
        {
            get { return new ThemesImplementation(HostingEnvironment.VirtualPathProvider, ScopeStorage.CurrentScope); }
        }

        public static void Initialize(string themeDirectory, string defaultTheme)
        {
            Implementation.Initialize(themeDirectory, defaultTheme);
        }

        /// <summary>
        /// Get a file that lives directly inside the theme directory
        /// </summary>
        /// <param name="fileName">The filename to look for</param>
        /// <returns>The full path to the file that matches the requested file</returns>
        public static string GetResourcePath(string fileName)
        {
            return Implementation.GetResourcePath(fileName);
        }

        public static string GetResourcePath(string folder, string fileName)
        {
            return Implementation.GetResourcePath(folder, fileName);
        }
    }
}
