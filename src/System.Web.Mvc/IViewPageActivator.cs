﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IViewPageActivator
    {
        object Create(ControllerContext controllerContext, Type type);
        object Create(ControllerContext controllerContext, Type type, Type[] genericTypes);
    }
}
