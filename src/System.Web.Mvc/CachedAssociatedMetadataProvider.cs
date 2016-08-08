// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;

namespace System.Web.Mvc
{
    public abstract class CachedAssociatedMetadataProvider<TModelMetadata> : AssociatedMetadataProvider
        where TModelMetadata : ModelMetadata
    {
        private static ConcurrentDictionary<Type, string> _typeIds = new ConcurrentDictionary<Type, string>();
        private string _cacheKeyPrefix;
        private CacheItemPolicy _cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(20) };
        private ObjectCache _prototypeCache;

        protected internal CacheItemPolicy CacheItemPolicy
        {
            get { return _cacheItemPolicy; }
            set { _cacheItemPolicy = value; }
        }

        protected string CacheKeyPrefix
        {
            get
            {
                if (_cacheKeyPrefix == null)
                {
                    _cacheKeyPrefix = "MetadataPrototypes::" + GetType().GUID.ToString("B");
                }
                return _cacheKeyPrefix;
            }
        }

        protected internal ObjectCache PrototypeCache
        {
            get { return _prototypeCache ?? MemoryCache.Default; }
            set { _prototypeCache = value; }
        }

        protected sealed override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            // If metadata is being created for a property then containerType != null && propertyName != null
            // If metadata is being created for a type then containerType == null && propertyName == null, so we have to use modelType for the cache key.
            Type typeForCache = containerType ?? modelType;
            string cacheKey = GetCacheKey(typeForCache, propertyName);
            TModelMetadata prototype = PrototypeCache.Get(cacheKey) as TModelMetadata;
            if (prototype == null)
            {
                prototype = CreateMetadataPrototype(attributes, containerType, modelType, propertyName);
                PrototypeCache.Add(cacheKey, prototype, CacheItemPolicy);
            }

            return CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        // New override for creating the prototype metadata (without the accessor)
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName);

        // New override for applying the prototype + modelAccess to yield the final metadata
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype, Func<object> modelAccessor);

        internal string GetCacheKey(Type type, string propertyName = null)
        {
            propertyName = propertyName ?? String.Empty;
            return CacheKeyPrefix + GetTypeId(type) + propertyName;
        }

        public sealed override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            return base.GetMetadataForProperty(modelAccessor, containerType, propertyName);
        }

        protected sealed override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, PropertyDescriptor propertyDescriptor)
        {
            return base.GetMetadataForProperty(modelAccessor, containerType, propertyDescriptor);
        }

        public sealed override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            return base.GetMetadataForProperties(container, containerType);
        }

        public sealed override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            return base.GetMetadataForType(modelAccessor, modelType);
        }

        private static string GetTypeId(Type type)
        {
            // It's fine using a random Guid since we store the mapping for types to guids.
            return _typeIds.GetOrAdd(type, _ => Guid.NewGuid().ToString("B"));
        }
    }
}
