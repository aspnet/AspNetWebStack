// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Web.Mvc.ModelBinding
{
    public static class ModelBinderProviders
    {
        private static readonly ModelBinderProviderCollection _providers = CreateDefaultCollection();

        public static ModelBinderProviderCollection Providers
        {
            get { return _providers; }
        }

        private static ModelBinderProviderCollection CreateDefaultCollection()
        {
            return new ModelBinderProviderCollection
            {
                new TypeMatchModelBinderProvider(),
                new BinaryDataModelBinderProvider(),
                new KeyValuePairModelBinderProvider(),
                new ComplexModelDtoModelBinderProvider(),
                new ArrayModelBinderProvider(),
                new DictionaryModelBinderProvider(),
                new CollectionModelBinderProvider(),
                new TypeConverterModelBinderProvider(),
                new MutableObjectModelBinderProvider()
            };
        }
    }
}
