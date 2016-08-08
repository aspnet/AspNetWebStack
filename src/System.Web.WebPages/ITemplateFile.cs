// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// An interface that provides information about the current executing file.
    /// WebPageRenderingBase implements this type so that all pages excluding AppStart pages could be queried to identify the 
    /// current executing file.
    /// </summary>
    public interface ITemplateFile
    {
        TemplateFileInfo TemplateInfo { get; }
    }
}
