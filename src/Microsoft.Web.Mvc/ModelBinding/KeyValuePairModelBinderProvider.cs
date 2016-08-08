// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class KeyValuePairModelBinderProvider : ModelBinderProvider
    {
        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            string keyFieldName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, "key");
            string valueFieldName = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, "value");

            if (bindingContext.ValueProvider.ContainsPrefix(keyFieldName) && bindingContext.ValueProvider.ContainsPrefix(valueFieldName))
            {
                return ModelBinderUtil.GetPossibleBinderInstance(bindingContext.ModelType, typeof(KeyValuePair<,>) /* supported model type */, typeof(KeyValuePairModelBinder<,>) /* binder type */);
            }
            else
            {
                // 'key' or 'value' missing
                return null;
            }
        }
    }
}
