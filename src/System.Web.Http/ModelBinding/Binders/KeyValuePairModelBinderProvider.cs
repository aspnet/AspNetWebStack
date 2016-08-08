// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class KeyValuePairModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            return ModelBindingHelper.GetPossibleBinderInstance(
                closedModelType: modelType, 
                openModelType: typeof(KeyValuePair<,>), 
                openBinderType: typeof(KeyValuePairModelBinder<,>));
        }
    }
}
