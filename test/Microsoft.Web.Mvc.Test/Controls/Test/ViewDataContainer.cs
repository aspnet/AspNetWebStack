// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class ViewDataContainer : Control, IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }
    }
}
