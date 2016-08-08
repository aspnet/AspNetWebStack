// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Filters
{
    public enum FilterScope
    {
        Global = 0,
        Controller = 10,
        Action = 20,
    }
}
