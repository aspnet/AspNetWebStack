// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class CompositeModelBinderProvider : ModelBinderProvider
    {
        private ModelBinderProvider[] _providers;

        public CompositeModelBinderProvider()
        {
        }

        public CompositeModelBinderProvider(IEnumerable<ModelBinderProvider> providers)
        {
            if (providers == null)
            {
                throw Error.ArgumentNull("providers");
            }

            _providers = providers.ToArray();
        }

        public IEnumerable<ModelBinderProvider> Providers
        {
            get { return _providers; }
        }

        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            IEnumerable<ModelBinderProvider> providers = _providers ?? configuration.Services.GetModelBinderProviders();

            // Pre-filter out any binders that we know can't match. 
            IEnumerable<IModelBinder> binders = from provider in providers 
                                                let binder = provider.GetBinder(configuration, modelType) 
                                                where binder != null 
                                                select binder;
            return new CompositeModelBinder(binders);
        }
    }
}
