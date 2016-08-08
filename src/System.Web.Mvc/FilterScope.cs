// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public enum FilterScope
    {
        First = 0,
        Global = 10,
        Controller = 20,
        Action = 30,
        Last = 100,
    }
}
