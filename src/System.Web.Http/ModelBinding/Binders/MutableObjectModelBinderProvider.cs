// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class MutableObjectModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (!MutableObjectModelBinder.CanBindType(modelType))
            {
                return null;
            }

            return new MutableObjectModelBinder();
        }
    }
}
