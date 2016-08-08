// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class ActionFilterAttribute : FilterAttribute, IActionFilter, IResultFilter
    {
        // The OnXxx() methods are virtual rather than abstract so that a developer need override
        // only the ones that interest him.

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
