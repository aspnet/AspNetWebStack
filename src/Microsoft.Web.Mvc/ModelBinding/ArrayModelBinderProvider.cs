// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class ArrayModelBinderProvider : ModelBinderProvider
    {
        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (!bindingContext.ModelMetadata.IsReadOnly && bindingContext.ModelType.IsArray &&
                bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                Type elementType = bindingContext.ModelType.GetElementType();
                return (IExtensibleModelBinder)Activator.CreateInstance(typeof(ArrayModelBinder<>).MakeGenericType(elementType));
            }

            return null;
        }
    }
}
