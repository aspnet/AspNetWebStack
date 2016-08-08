// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Returns a binder that can extract a ValueProviderResult.RawValue and return it directly.
    [ModelBinderProviderOptions(FrontOfList = true)]
    public sealed class TypeMatchModelBinderProvider : ModelBinderProvider
    {
        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            return (TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext) != null)
                       ? new TypeMatchModelBinder()
                       : null /* no match */;
        }
    }
}
