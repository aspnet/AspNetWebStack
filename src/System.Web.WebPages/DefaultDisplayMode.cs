// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages
{
    /// <summary>
    /// The <see cref="DefaultDisplayMode"/> can take any suffix and determine if there is a corresponding
    /// file that exists given a path and request by transforming the path to contain the suffix.
    /// Add a new DefaultDisplayMode to the Modes collection to handle a new suffix or inherit from
    /// DefaultDisplayMode to provide custom logic to transform paths with a suffix.
    /// </summary>
    public class DefaultDisplayMode : IDisplayMode
    {
        private readonly string _suffix;

        public DefaultDisplayMode()
            : this(DisplayModeProvider.DefaultDisplayModeId)
        {
        }

        public DefaultDisplayMode(string suffix)
        {
            _suffix = suffix ?? String.Empty;
        }

        /// <summary>
        /// When set, the <see cref="DefaultDisplayMode"/> will only be available to return Display Info for a request
        /// if the ContextCondition evaluates to true.
        /// </summary>
        public Func<HttpContextBase, bool> ContextCondition { get; set; }

        public virtual string DisplayModeId
        {
            get { return _suffix; }
        }

        public bool CanHandleContext(HttpContextBase httpContext)
        {
            return ContextCondition == null || ContextCondition(httpContext);
        }

        /// <summary>
        /// Returns DisplayInfo with the transformed path if it exists.
        /// </summary>
        public virtual DisplayInfo GetDisplayInfo(HttpContextBase httpContext, string virtualPath, Func<string, bool> virtualPathExists)
        {
            string transformedFilename = TransformPath(virtualPath, _suffix);
            if (transformedFilename != null && virtualPathExists(transformedFilename))
            {
                return new DisplayInfo(transformedFilename, this);
            }

            return null;
        }

        /// <summary>
        /// Transforms paths according to the following rules:
        /// \some\path.blah\file.txt.zip -> \some\path.blah\file.txt.suffix.zip
        /// \some\path.blah\file -> \some\path.blah\file.suffix
        /// </summary>
        protected virtual string TransformPath(string virtualPath, string suffix)
        {
            if (String.IsNullOrEmpty(suffix))
            {
                return virtualPath;
            }

            string extension = Path.GetExtension(virtualPath);
            return Path.ChangeExtension(virtualPath, suffix + extension);
        }
    }
}
