// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Returns a binder that can perform conversions using a .NET TypeConverter.
    public sealed class TypeConverterModelBinderProvider : ModelBinderProvider
    {
        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return null; // no value to convert
            }

            if (!TypeDescriptor.GetConverter(bindingContext.ModelType).CanConvertFrom(typeof(string)))
            {
                return null; // this type cannot be converted
            }

            return new TypeConverterModelBinder();
        }
    }
}
