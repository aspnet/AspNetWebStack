// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public interface IExtensibleModelBinder
    {
        bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext);
    }
}
