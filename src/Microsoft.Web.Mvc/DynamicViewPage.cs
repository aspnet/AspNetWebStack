// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    public class DynamicViewPage : ViewPage
    {
        public new dynamic Model
        {
            get { return DynamicReflectionObject.Wrap(base.Model); }
        }

        public new dynamic ViewData
        {
            get { return DynamicViewDataDictionary.Wrap(base.ViewData); }
        }
    }
}
