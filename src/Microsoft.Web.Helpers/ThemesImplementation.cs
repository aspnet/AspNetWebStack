// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Microsoft.Internal.Web.Utils;
using Resources;

namespace Microsoft.Web.Helpers
{
    internal class ThemesImplementation
    {
        internal static readonly object CurrentThemeKey = new object();
        internal static readonly object ThemeDirectoryKey = new object();
        internal static readonly object DefaultThemeKey = new object();
        internal static readonly object ThemesInitializedKey = new object();
        private readonly VirtualPathProvider _vpp;
        private readonly IDictionary<object, object> _currentScope;

        public ThemesImplementation(VirtualPathProvider vpp, IDictionary<object, object> scopeStorage)
        {
            _vpp = vpp;
            _currentScope = scopeStorage;
        }

        public string ThemeDirectory
        {
            get
            {
                EnsureInitialized();
                return (string)_currentScope[ThemeDirectoryKey];
            }
            private set
            {
                Debug.Assert(value != null);
                _currentScope[ThemeDirectoryKey] = value;
            }
        }

        /// <summary>
        /// This should live throughout the application life cycle
        /// and be set in _appstart.cshtml
        /// </summary>
        public string DefaultTheme
        {
            get
            {
                EnsureInitialized();
                return (string)_currentScope[DefaultThemeKey];
            }
            private set
            {
                Debug.Assert(value != null);
                _currentScope[DefaultThemeKey] = value;
            }
        }

        /// <summary>
        /// The current theme to use. When this is set,
        /// all GetResource checks will check if the CurrentTheme
        /// contains the file, and if it doesn't it will fall back to
        /// the DefaultTheme
        /// </summary>
        public string CurrentTheme
        {
            get
            {
                EnsureInitialized();
                return (string)_currentScope[CurrentThemeKey] ?? DefaultTheme;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value");
                }

                // EnsureValidTheme would verify if themes have been correctly initialized and that the value specified is a valid theme.
                if (!IsValidTheme(AvailableThemes, value))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, HelpersToolkitResources.Themes_InvalidTheme, value), "value");
                }
                _currentScope[CurrentThemeKey] = value;
            }
        }

        public ReadOnlyCollection<string> AvailableThemes
        {
            get
            {
                EnsureInitialized();
                return GetAvailableThemes(ThemeDirectory);
            }
        }

        private string CurrentThemePath
        {
            get { return Path.Combine(ThemeDirectory, CurrentTheme); }
        }

        private string DefaultThemePath
        {
            get { return Path.Combine(ThemeDirectory, DefaultTheme); }
        }

        private bool ThemesInitialized
        {
            get
            {
                bool? value = (bool?)_currentScope[ThemesInitializedKey];
                return value != null && value.Value;
            }
            set { _currentScope[ThemesInitializedKey] = value; }
        }

        public void Initialize(string themeDirectory, string defaultTheme)
        {
            if (String.IsNullOrEmpty(themeDirectory))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "themeDirectory");
            }

            if (String.IsNullOrEmpty(defaultTheme))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "defaultTheme");
            }

            var availableThemes = GetAvailableThemes(themeDirectory);
            if (!IsValidTheme(availableThemes, defaultTheme))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, HelpersToolkitResources.Themes_InvalidTheme, defaultTheme), "defaultTheme");
            }

            ThemeDirectory = themeDirectory;
            DefaultTheme = defaultTheme;
            ThemesInitialized = true;
        }

        /// <summary>
        /// Get a file that lives directly inside the theme directory
        /// </summary>
        /// <param name="fileName">The filename to look for</param>
        /// <returns>The full path to the file that matches the requested file</returns>
        public string GetResourcePath(string fileName)
        {
            return GetResourcePath(String.Empty, fileName);
        }

        public string GetResourcePath(string folder, string fileName)
        {
            EnsureInitialized();

            if (folder == null)
            {
                throw new ArgumentNullException("folder", HelpersToolkitResources.Themes_FolderCannotBeNull);
            }

            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fileName");
            }

            return FindMatchingFile(Path.Combine(CurrentThemePath, folder), fileName) ??
                   FindMatchingFile(Path.Combine(DefaultThemePath, folder), fileName);
        }

        /// <summary>
        /// Try and find a file in the specified folder that matches name.
        /// </summary>
        /// <returns>The full path to the file that matches the requested file
        /// or null if no matching file is found</returns>
        internal string FindMatchingFile(string folder, string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(folder));
            Debug.Assert(!String.IsNullOrEmpty(name));

            // Get the virtual path information
            VirtualDirectory directory = _vpp.GetDirectory(folder);

            // If the folder specified doesn't exist
            // or it doesn't contain any files
            if (directory == null || directory.Files == null)
            {
                return null;
            }

            // Go through every file in the directory
            foreach (VirtualFile file in directory.Files)
            {
                string path = file.VirtualPath;

                // Compare the filename to the filename that we passed
                if (Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            // If no matching files, return null
            return null;
        }

        private ReadOnlyCollection<string> GetAvailableThemes(string themesRoot)
        {
            VirtualDirectory directory = _vpp.GetDirectory(themesRoot);

            var themes = new List<string>();

            // Go through every file in the directory
            foreach (VirtualDirectory dir in directory.Directories)
            {
                themes.Add(dir.Name);
            }
            return themes.AsReadOnly();
        }

        private void EnsureInitialized()
        {
            if (!ThemesInitialized)
            {
                throw new InvalidOperationException(HelpersToolkitResources.Themes_NotInitialized);
            }
        }

        private static bool IsValidTheme(IEnumerable<string> availableThemes, string theme)
        {
            return availableThemes.Contains(theme, StringComparer.OrdinalIgnoreCase);
        }
    }
}
