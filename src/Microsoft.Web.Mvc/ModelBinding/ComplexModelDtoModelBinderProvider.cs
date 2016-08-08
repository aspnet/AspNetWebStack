// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Returns a binder that can bind ComplexModelDto objects.
    public sealed class ComplexModelDtoModelBinderProvider : ModelBinderProvider
    {
        // This is really just a simple binder.
        private static readonly SimpleModelBinderProvider _underlyingProvider = GetUnderlyingProvider();

        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            return _underlyingProvider.GetBinder(controllerContext, bindingContext);
        }

        private static SimpleModelBinderProvider GetUnderlyingProvider()
        {
            return new SimpleModelBinderProvider(typeof(ComplexModelDto), new ComplexModelDtoModelBinder())
            {
                SuppressPrefixCheck = true
            };
        }
    }
}
