// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;

namespace System.Web.Http.ModelBinding.Binders
{
    public class DictionaryModelBinder<TKey, TValue> : CollectionModelBinder<KeyValuePair<TKey, TValue>>
    {
        protected override bool CreateOrReplaceCollection(HttpActionContext actionContext, ModelBindingContext bindingContext, IList<KeyValuePair<TKey, TValue>> newCollection)
        {
            CollectionModelBinderUtil.CreateOrReplaceDictionary(bindingContext, newCollection, () => new Dictionary<TKey, TValue>());
            return true;
        }
    }
}
