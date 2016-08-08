// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc.Async
{
    public abstract class AsyncActionDescriptor : ActionDescriptor
    {
        public abstract IAsyncResult BeginExecute(ControllerContext controllerContext, IDictionary<string, object> parameters, AsyncCallback callback, object state);

        public abstract object EndExecute(IAsyncResult asyncResult);

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
        {
            string errorMessage = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionDescriptor_CannotExecuteSynchronously,
                                                ActionName);

            throw new InvalidOperationException(errorMessage);
        }

        internal static AsyncManager GetAsyncManager(ControllerBase controller)
        {
            IAsyncManagerContainer helperContainer = controller as IAsyncManagerContainer;
            if (helperContainer == null)
            {
                throw Error.AsyncCommon_ControllerMustImplementIAsyncManagerContainer(controller.GetType());
            }

            return helperContainer.AsyncManager;
        }
    }
}
