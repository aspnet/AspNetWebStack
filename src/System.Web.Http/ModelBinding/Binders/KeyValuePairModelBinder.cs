// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class KeyValuePairModelBinder<TKey, TValue> : IModelBinder
    {
        internal ModelMetadataProvider MetadataProvider { private get; set; }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelMetadataProvider metadataProvider = MetadataProvider ?? actionContext.GetMetadataProvider();
            ModelBindingHelper.ValidateBindingContext(bindingContext, typeof(KeyValuePair<TKey, TValue>), true /* allowNullModel */);

            TKey key;
            bool keyBindingSucceeded = actionContext.TryBindStrongModel(bindingContext, "key", metadataProvider, out key);

            TValue value;
            bool valueBindingSucceeded = actionContext.TryBindStrongModel(bindingContext, "value", metadataProvider, out value);

            if (keyBindingSucceeded && valueBindingSucceeded)
            {
                bindingContext.Model = new KeyValuePair<TKey, TValue>(key, value);
            }
            return keyBindingSucceeded || valueBindingSucceeded;
        }
    }
}
