// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class TypeConverterModelBinder : IModelBinder
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is recorded to be acted upon later.")]
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Http.ValueProviders.ValueProviderResult.ConvertTo(System.Type)", Justification = "The ValueProviderResult already has the necessary context to perform a culture-aware conversion.")]
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return false; // no entry
            }

            // Provider should have verified this before creating
            Contract.Assert(TypeHelper.HasStringConverter(bindingContext.ModelType));            

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
                    string errorString = ModelBinderConfig.TypeConversionErrorMessageProvider(actionContext, bindingContext.ModelMetadata, valueProviderResult.AttemptedValue);
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

            ModelBindingHelper.ReplaceEmptyStringWithNull(bindingContext.ModelMetadata, ref newModel);
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
