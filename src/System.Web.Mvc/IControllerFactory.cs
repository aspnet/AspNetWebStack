// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using System.Web.SessionState;

namespace System.Web.Mvc
{
    public interface IControllerFactory
    {
        IController CreateController(RequestContext requestContext, string controllerName);
        SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName);
        void ReleaseController(IController controller);
    }
}
