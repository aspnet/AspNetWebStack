// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Returns a user-specified binder for a given type.
    public sealed class SimpleModelBinderProvider : ModelBinderProvider
    {
        private readonly Func<IExtensibleModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public SimpleModelBinderProvider(Type modelType, IExtensibleModelBinder modelBinder)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinder == null)
            {
                throw new ArgumentNullException("modelBinder");
            }

            _modelType = modelType;
            _modelBinderFactory = () => modelBinder;
        }

        public SimpleModelBinderProvider(Type modelType, Func<IExtensibleModelBinder> modelBinderFactory)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinderFactory == null)
            {
                throw new ArgumentNullException("modelBinderFactory");
            }

            _modelType = modelType;
            _modelBinderFactory = modelBinderFactory;
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public bool SuppressPrefixCheck { get; set; }

        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            if (bindingContext.ModelType == ModelType)
            {
                if (SuppressPrefixCheck || bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                {
                    return _modelBinderFactory();
                }
            }

            return null;
        }
    }
}
