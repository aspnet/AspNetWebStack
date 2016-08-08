// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    [Flags]
    public enum HttpVerbs
    {
        Get = 1 << 0,
        Post = 1 << 1,
        Put = 1 << 2,
        Delete = 1 << 3,
        Head = 1 << 4,
        Patch = 1 << 5,
        Options = 1 << 6,
    }
}
