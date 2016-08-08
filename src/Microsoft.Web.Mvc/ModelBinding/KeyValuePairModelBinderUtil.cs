// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    internal static class KeyValuePairModelBinderUtil
    {
        public static bool TryBindStrongModel<TModel>(ControllerContext controllerContext, ExtensibleModelBindingContext parentBindingContext, string propertyName, ModelMetadataProvider metadataProvider, out TModel model)
        {
            ExtensibleModelBindingContext propertyBindingContext = new ExtensibleModelBindingContext(parentBindingContext)
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, typeof(TModel)),
                ModelName = ModelBinderUtil.CreatePropertyModelName(parentBindingContext.ModelName, propertyName)
            };

            IExtensibleModelBinder binder = parentBindingContext.ModelBinderProviders.GetBinder(controllerContext, propertyBindingContext);
            if (binder != null)
            {
                if (binder.BindModel(controllerContext, propertyBindingContext))
                {
                    object untypedModel = propertyBindingContext.Model;
                    model = ModelBinderUtil.CastOrDefault<TModel>(untypedModel);
                    parentBindingContext.ValidationNode.ChildNodes.Add(propertyBindingContext.ValidationNode);
                    return true;
                }
            }

            model = default(TModel);
            return false;
        }
    }
}
