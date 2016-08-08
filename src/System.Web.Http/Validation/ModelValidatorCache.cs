// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal class ModelValidatorCache : IModelValidatorCache
    {
        private ConcurrentDictionary<EfficientTypePropertyKey<Type, string>, ModelValidator[]> _validatorCache = new ConcurrentDictionary<EfficientTypePropertyKey<Type, string>, ModelValidator[]>();
        private Lazy<IEnumerable<ModelValidatorProvider>> _validatorProviders;

        public ModelValidatorCache(Lazy<IEnumerable<ModelValidatorProvider>> validatorProviders)
        {
            _validatorProviders = validatorProviders;
        }

        public ModelValidator[] GetValidators(ModelMetadata metadata)
        {
            ModelValidator[] validators;
            if (!_validatorCache.TryGetValue(metadata.CacheKey, out validators))
            {
                // Compute validators
                // There are no side-effects if the same validators are created more than once
                validators = metadata.GetValidators(_validatorProviders.Value).ToArray();
                _validatorCache.TryAdd(metadata.CacheKey, validators);
            }
            return validators;
        }
    }
}
