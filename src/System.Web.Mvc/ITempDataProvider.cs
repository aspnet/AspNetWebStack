// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public interface ITempDataProvider
    {
        IDictionary<string, object> LoadTempData(ControllerContext controllerContext);
        void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values);
    }
}
