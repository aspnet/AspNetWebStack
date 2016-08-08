// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class ModelMetadataProviders
    {
        private static ModelMetadataProviders _instance = new ModelMetadataProviders();
        private ModelMetadataProvider _currentProvider;
        private IResolver<ModelMetadataProvider> _resolver;

        internal ModelMetadataProviders(IResolver<ModelMetadataProvider> resolver = null)
        {
            _resolver = resolver ?? new SingleServiceResolver<ModelMetadataProvider>(
                                        () => _currentProvider,
                                        new CachedDataAnnotationsModelMetadataProvider(),
                                        "ModelMetadataProviders.Current");
        }

        public static ModelMetadataProvider Current
        {
            get { return _instance.CurrentInternal; }
            set { _instance.CurrentInternal = value; }
        }

        internal ModelMetadataProvider CurrentInternal
        {
            get { return _resolver.Current; }
            set { _currentProvider = value ?? new EmptyModelMetadataProvider(); }
        }
    }
}
