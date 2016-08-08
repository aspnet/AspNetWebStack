// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// DisplayInfo wraps the resolved file path and IDisplayMode for a request and path.
    /// The returned IDisplayMode can be used to resolve other page elements for the request.
    /// </summary>
    public class DisplayInfo
    {
        public DisplayInfo(string filePath, IDisplayMode displayMode)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (displayMode == null)
            {
                throw new ArgumentNullException("displayMode");
            }

            FilePath = filePath;
            DisplayMode = displayMode;
        }

        /// <summary>
        /// The Display Mode used to resolve a virtual path.
        /// </summary>
        public IDisplayMode DisplayMode { get; private set; }

        /// <summary>
        /// Resolved path of a file that exists.
        /// </summary>
        public string FilePath { get; private set; }
    }
}
