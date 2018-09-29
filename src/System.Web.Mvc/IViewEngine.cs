// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IViewEngine
    {
        ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache);
        ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache);
        // ------------------- Branch: support_generic_models_in_views (start) -------------------
        ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache, Type[] genericTypes);
        ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, Type[] genericTypes);
        // ------------------- Branch: support_generic_models_in_views ( end ) -------------------
        void ReleaseView(ControllerContext controllerContext, IView view);
    }
}
