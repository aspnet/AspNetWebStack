// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Ajax
{
    public enum InsertionMode
    {
        /// <summary>
        /// Replace the contents of the element.
        /// </summary>
        Replace = 0,

        /// <summary>
        /// Insert before the element.
        /// </summary>
        InsertBefore = 1,

        /// <summary>
        /// Insert after the element.
        /// </summary>
        InsertAfter = 2,

        /// <summary>
        /// Replace the entire element.
        /// </summary>
        ReplaceWith = 3
    }
}
