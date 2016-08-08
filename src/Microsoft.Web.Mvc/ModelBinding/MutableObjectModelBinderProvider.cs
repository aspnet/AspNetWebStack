// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class MutableObjectModelBinderProvider : ModelBinderProvider
    {
        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (!bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                // no values to bind
                return null;
            }

            if (bindingContext.ModelType == typeof(ComplexModelDto))
            {
                // forbidden type - will cause a stack overflow if we try binding this type
                return null;
            }

            return new MutableObjectModelBinder();
        }
    }
}
