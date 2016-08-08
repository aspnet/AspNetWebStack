// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class TypeMatchModelBinder : IExtensibleModelBinder
    {
        public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ValueProviderResult valueProviderResult = GetCompatibleValueProviderResult(bindingContext);
            if (valueProviderResult == null)
            {
                return false; // conversion would have failed
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            object model = valueProviderResult.RawValue;
            ModelBinderUtil.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref model);
            bindingContext.Model = model;

            return true;
        }

        internal static ValueProviderResult GetCompatibleValueProviderResult(ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return null; // the value doesn't exist
            }

            if (!TypeHelpers.IsCompatibleObject(bindingContext.ModelType, valueProviderResult.RawValue))
            {
                return null; // value is of incompatible type
            }

            return valueProviderResult;
        }
    }
}
