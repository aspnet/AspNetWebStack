// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc.Async;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Unsealed so that subclassed types can set properties in the default constructor.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AsyncTimeoutAttribute : ActionFilterAttribute
    {
        // duration is specified in milliseconds
        public AsyncTimeoutAttribute(int duration)
        {
            if (duration < -1)
            {
                throw Error.AsyncCommon_InvalidTimeout("duration");
            }

            Duration = duration;
        }

        public int Duration { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            IAsyncManagerContainer container = filterContext.Controller as IAsyncManagerContainer;
            if (container == null)
            {
                throw Error.AsyncCommon_ControllerMustImplementIAsyncManagerContainer(filterContext.Controller.GetType());
            }

            container.AsyncManager.Timeout = Duration;

            base.OnActionExecuting(filterContext);
        }
    }
}
