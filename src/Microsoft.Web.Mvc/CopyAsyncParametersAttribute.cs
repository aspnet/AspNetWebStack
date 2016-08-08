// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class CopyAsyncParametersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            IAsyncManagerContainer container = filterContext.Controller as IAsyncManagerContainer;
            if (container != null)
            {
                AsyncManager asyncManager = container.AsyncManager;
                foreach (var entry in filterContext.ActionParameters)
                {
                    asyncManager.Parameters[entry.Key] = entry.Value;
                }
            }
        }
    }
}
