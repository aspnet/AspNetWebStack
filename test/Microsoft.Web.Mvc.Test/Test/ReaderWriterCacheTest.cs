// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class ReaderWriterCacheTest
    {
        [Fact]
        public void PublicFetchOrCreateItemCreatesItemIfNotAlreadyInCache()
        {
            // Arrange
            ReaderWriterCacheHelper<int, string> helper = new ReaderWriterCacheHelper<int, string>();
            Dictionary<int, string> cache = helper.PublicCache;

            // Act
            string item = helper.PublicFetchOrCreateItem(42, () => "new");

            // Assert
            Assert.Equal("new", cache[42]);
            Assert.Equal("new", item);
        }

        [Fact]
        public void PublicFetchOrCreateItemReturnsExistingItemIfFound()
        {
            // Arrange
            ReaderWriterCacheHelper<int, string> helper = new ReaderWriterCacheHelper<int, string>();
            Dictionary<int, string> cache = helper.PublicCache;
            helper.PublicCache[42] = "original";

            // Act
            string item = helper.PublicFetchOrCreateItem(42, () => "new");

            // Assert
            Assert.Equal("original", cache[42]);
            Assert.Equal("original", item);
        }

        [Fact]
        public void PublicFetchOrCreateItemReturnsFirstItemIfTwoThreadsUpdateCacheSimultaneously()
        {
            // Arrange
            ReaderWriterCacheHelper<int, string> helper = new ReaderWriterCacheHelper<int, string>();
            Dictionary<int, string> cache = helper.PublicCache;
            Func<string> creator = delegate
            {
                // fake a second thread coming along when we weren't looking
                string firstItem = helper.PublicFetchOrCreateItem(42, () => "original");

                Assert.Equal("original", cache[42]);
                Assert.Equal("original", firstItem);
                return "new";
            };

            // Act
            string secondItem = helper.PublicFetchOrCreateItem(42, creator);

            // Assert
            Assert.Equal("original", cache[42]);
            Assert.Equal("original", secondItem);
        }

        private class ReaderWriterCacheHelper<TKey, TValue> : ReaderWriterCache<TKey, TValue>
        {
            public Dictionary<TKey, TValue> PublicCache
            {
                get { return Cache; }
            }

            public TValue PublicFetchOrCreateItem(TKey key, Func<TValue> creator)
            {
                return FetchOrCreateItem(key, creator);
            }
        }
    }
}
