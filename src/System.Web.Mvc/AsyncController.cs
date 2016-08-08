// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // Controller now supports asynchronous operations.
    // This class only exists 
    // a) for backwards compat for callers that derive from it,
    // b) ActionMethodSelector can detect it to bind to ActionAsync/ActionCompleted patterns. 
    public abstract class AsyncController : Controller
    {
    }
}
