// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Async
{
    public interface IAsyncActionInvoker : IActionInvoker
    {
        IAsyncResult BeginInvokeAction(ControllerContext controllerContext, string actionName, AsyncCallback callback, object state);
        bool EndInvokeAction(IAsyncResult asyncResult);
    }
}
