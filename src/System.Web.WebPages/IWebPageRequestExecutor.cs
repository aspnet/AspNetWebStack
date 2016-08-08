// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    // An executor is a class that can take over the execution of a WebPage. This can be used to
    // implement features like AJAX callback methods on the page (like WebForms Page Methods)
    public interface IWebPageRequestExecutor
    {
        bool Execute(WebPage page);
    }
}
