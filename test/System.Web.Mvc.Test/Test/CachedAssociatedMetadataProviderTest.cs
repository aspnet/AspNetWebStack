// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class CachedAssociatedMetadataProviderTest
    {
        [Fact]
        public void GetMetadataForPropertyInvalidPropertyNameThrows()
        {
            // Arrange
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => provider.GetMetadataForProperty(null, typeof(object), "BadPropertyName"),
                "The property System.Object.BadPropertyName could not be found.");
        }

        [Fact]
        public void GetCacheKey_ResultsForTypesDoNotCollide()
        {
            // Arrange
            var provider = new MockableCachedAssociatedMetadataProvider();
            var keys = new List<string>();

            // Act
            keys.Add(provider.GetCacheKey(typeof(string)));
            keys.Add(provider.GetCacheKey(typeof(int)));
            keys.Add(provider.GetCacheKey(typeof(Nullable<int>)));
            keys.Add(provider.GetCacheKey(typeof(Nullable<bool>)));
            keys.Add(provider.GetCacheKey(typeof(List<string>)));
            keys.Add(provider.GetCacheKey(typeof(List<bool>)));

            // Assert
            Assert.Equal(keys.Distinct().Count(), keys.Count);
        }

        [Fact]
        public void GetCacheKey_ResultsForTypesAndPropertiesDoNotCollide()
        {
            // Arrange
            var provider = new MockableCachedAssociatedMetadataProvider();
            var keys = new List<string>();

            // Act
            keys.Add(provider.GetCacheKey(typeof(string), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(string), "Bar"));
            keys.Add(provider.GetCacheKey(typeof(int), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(Nullable<int>), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(Nullable<bool>), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(List<string>), "Count"));
            keys.Add(provider.GetCacheKey(typeof(List<bool>), "Count"));
            keys.Add(provider.GetCacheKey(typeof(Foo), "BarBaz"));
            keys.Add(provider.GetCacheKey(typeof(FooBar), "Baz"));

            // Assert
            Assert.Equal(keys.Distinct().Count(), keys.Count);
        }

        private class Foo
        {
        }

        private class FooBar
        {
        }

        // GetMetadataForProperty

        [Fact]
        public void GetMetadataForPropertyCreatesPrototypeMetadataAndAddsItToCache()
        {
            // Arrange
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(() => 3, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               typeof(string) /* containerType */,
                                                               typeof(int) /* modelType */,
                                                               "Length" /* propertyName */));
            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), "Length"),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null));
        }

        [Fact]
        public void GetMetadataForPropertyCreatesRealMetadataFromPrototype()
        {
            // Arrange
            Func<object> accessor = () => 3;
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor));
        }

        [Fact]
        public void MetaDataAwareAttributesForPropertyAreAppliedToMetadata()
        {
            // Arrange
            MemoryCache memoryCache = new MemoryCache("testCache");
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider(memoryCache);

            // Act
            ModelMetadata metadata = provider.GetMetadataForProperty(null, typeof(ClassWithMetaDataAwareAttributes), "PropertyWithAdditionalValue");

            // Assert
            Assert.True(metadata.AdditionalValues["baz"].Equals("biz"));
        }

        [Fact]
        public void GetMetadataForPropertyTwiceOnlyCreatesAndCachesPrototypeOnce()
        {
            // Arrange
            Func<object> accessor = () => 3;
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               typeof(string) /* containerType */,
                                                               typeof(int) /* modelType */,
                                                               "Length" /* propertyName */),
                            Times.Once());

            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor),
                            Times.Exactly(2));

            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), "Length"),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null),
                                         Times.Once());
        }

        // GetMetadataForType

        [Fact]
        public void GetMetadataForTypeCreatesPrototypeMetadataAndAddsItToCache()
        {
            // Arrange
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(() => "foo", typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               null /* containerType */,
                                                               typeof(string) /* modelType */,
                                                               null /* propertyName */));
            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), null),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null));
        }

        [Fact]
        public void GetMetadataForTypeCreatesRealMetadataFromPrototype()
        {
            // Arrange
            Func<object> accessor = () => "foo";
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(accessor, typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor));
        }

        [Fact]
        public void MetaDataAwareAttributesForTypeAreAppliedToMetadata()
        {
            // Arrange
            MemoryCache memoryCache = new MemoryCache("testCache");
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider(memoryCache);

            // Act
            ModelMetadata metadata = provider.GetMetadataForType(null, typeof(ClassWithMetaDataAwareAttributes));

            // Assert
            Assert.True(metadata.AdditionalValues["foo"].Equals("bar"));
        }

        [Fact]
        public void GetMetadataForTypeTwiceOnlyCreatesAndCachesPrototypeOnce()
        {
            // Arrange
            Func<object> accessor = () => "foo";
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(accessor, typeof(string));
            provider.Object.GetMetadataForType(accessor, typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               null /* containerType */,
                                                               typeof(string) /* modelType */,
                                                               null /* propertyName */),
                            Times.Once());

            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor),
                            Times.Exactly(2));

            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), null),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null),
                                         Times.Once());
        }

        // Helpers

        public class MockableCachedAssociatedMetadataProvider : CachedAssociatedMetadataProvider<ModelMetadata>
        {
            public Mock<MemoryCache> Cache;
            public ModelMetadata PrototypeMetadata;
            public ModelMetadata RealMetadata;

            public MockableCachedAssociatedMetadataProvider()
                : this(null)
            {
            }

            public MockableCachedAssociatedMetadataProvider(MemoryCache memoryCache = null)
            {
                Cache = new Mock<MemoryCache>("MockMemoryCache", null) { CallBase = true };
                PrototypeMetadata = new ModelMetadata(this, null, null, typeof(string), null);
                RealMetadata = new ModelMetadata(this, null, null, typeof(string), null);

                PrototypeCache = memoryCache ?? Cache.Object;
            }

            public virtual ModelMetadata CreateMetadataPrototypeImpl(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
            {
                return PrototypeMetadata;
            }

            public virtual ModelMetadata CreateMetadataFromPrototypeImpl(ModelMetadata prototype, Func<object> modelAccessor)
            {
                return RealMetadata;
            }

            protected override ModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
            {
                return CreateMetadataPrototypeImpl(attributes, containerType, modelType, propertyName);
            }

            protected override ModelMetadata CreateMetadataFromPrototype(ModelMetadata prototype, Func<object> modelAccessor)
            {
                return CreateMetadataFromPrototypeImpl(prototype, modelAccessor);
            }
        }

        [AdditionalMetadata("foo", "bar")]
        private class ClassWithMetaDataAwareAttributes
        {
            [AdditionalMetadata("baz", "biz")]
            public string PropertyWithAdditionalValue { get; set; }
        }
    }
}
