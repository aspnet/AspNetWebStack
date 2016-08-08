// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers
{
    [Flags]
    public enum WebGridPagerModes
    {
        Numeric = 0x1,
        NextPrevious = 0x2,
        FirstLast = 0x4,
        All = 0x7
    }
}
