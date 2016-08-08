// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public class DictionaryModelBinder<TKey, TValue> : CollectionModelBinder<KeyValuePair<TKey, TValue>>
    {
        protected override bool CreateOrReplaceCollection(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, IList<KeyValuePair<TKey, TValue>> newCollection)
        {
            CollectionModelBinderUtil.CreateOrReplaceDictionary(bindingContext, newCollection, () => new Dictionary<TKey, TValue>());
            return true;
        }
    }
}
