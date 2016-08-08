// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class KeyValuePairModelBinder<TKey, TValue> : IExtensibleModelBinder
    {
        private ModelMetadataProvider _metadataProvider;

        internal ModelMetadataProvider MetadataProvider
        {
            get
            {
                if (_metadataProvider == null)
                {
                    _metadataProvider = ModelMetadataProviders.Current;
                }
                return _metadataProvider;
            }
            set { _metadataProvider = value; }
        }

        public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(KeyValuePair<TKey, TValue>), true /* allowNullModel */);

            TKey key;
            bool keyBindingSucceeded = KeyValuePairModelBinderUtil.TryBindStrongModel(controllerContext, bindingContext, "key", MetadataProvider, out key);

            TValue value;
            bool valueBindingSucceeded = KeyValuePairModelBinderUtil.TryBindStrongModel(controllerContext, bindingContext, "value", MetadataProvider, out value);

            if (keyBindingSucceeded && valueBindingSucceeded)
            {
                bindingContext.Model = new KeyValuePair<TKey, TValue>(key, value);
            }
            return keyBindingSucceeded || valueBindingSucceeded;
        }
    }
}
