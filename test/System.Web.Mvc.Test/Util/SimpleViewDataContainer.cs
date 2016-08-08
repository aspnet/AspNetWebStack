// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.UnitTestUtil
{
    public class SimpleViewDataContainer : IViewDataContainer
    {
        public SimpleViewDataContainer(ViewDataDictionary viewData)
        {
            ViewData = viewData;
        }

        public ViewDataDictionary ViewData { get; set; }
    }
}
