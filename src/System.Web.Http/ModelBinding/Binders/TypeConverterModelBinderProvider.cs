// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding.Binders
{
    // Returns a binder that can perform conversions using a .NET TypeConverter.
    public sealed class TypeConverterModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            if (!TypeHelper.HasStringConverter(modelType))
            {
                return null; // this type cannot be converted
            }
            return new TypeConverterModelBinder();
        }
    }
}
