// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc
{
    public interface IControllerActivator
    {
        IController Create(RequestContext requestContext, Type controllerType);
    }
}
