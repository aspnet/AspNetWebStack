// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Internal;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class DictionaryModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            return CollectionModelBinderUtil.GetGenericBinder(typeof(IDictionary<,>), typeof(Dictionary<,>), typeof(DictionaryModelBinder<,>), modelType);
        }
    }
}
