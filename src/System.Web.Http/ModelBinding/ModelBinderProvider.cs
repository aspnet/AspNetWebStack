// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    public abstract class ModelBinderProvider
    {
        /// <summary>
        /// Find a binder for the given type
        /// </summary>
        /// <param name="configuration">a configuration object</param>
        /// <param name="modelType">the type of the model to bind against.</param>
        /// <returns>a binder, which can attempt to bind this type. Or null if the binder knows statically that it will never be able to bind the type.</returns>
        public abstract IModelBinder GetBinder(HttpConfiguration configuration, Type modelType);
    }
}
