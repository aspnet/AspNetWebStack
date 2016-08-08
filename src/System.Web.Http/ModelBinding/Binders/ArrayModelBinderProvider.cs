// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class ArrayModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            if (!modelType.IsArray)
            {
                return null;
            }

            Type elementType = modelType.GetElementType();
            return (IModelBinder)Activator.CreateInstance(typeof(ArrayModelBinder<>).MakeGenericType(elementType));
        }
    }
}
