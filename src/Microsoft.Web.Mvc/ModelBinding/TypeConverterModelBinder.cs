// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class TypeConverterModelBinder : IExtensibleModelBinder
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is recorded to be acted upon later.")]
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "The ValueProviderResult already has the necessary context to perform a culture-aware conversion.")]
        public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return false; // no entry
            }

            object newModel;
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            try
            {
                newModel = valueProviderResult.ConvertTo(bindingContext.ModelType);
            }
            catch (Exception ex)
            {
                if (IsFormatException(ex))
                {
                    // there was a type conversion failure
                    string errorString = ModelBinderConfig.TypeConversionErrorMessageProvider(controllerContext, bindingContext.ModelMetadata, valueProviderResult.AttemptedValue);
                    if (errorString != null)
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, errorString);
                    }
                }
                else
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                }
                return false;
            }

            ModelBinderUtil.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref newModel);
            bindingContext.Model = newModel;
            return true;
        }

        private static bool IsFormatException(Exception ex)
        {
            for (; ex != null; ex = ex.InnerException)
            {
                if (ex is FormatException)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
