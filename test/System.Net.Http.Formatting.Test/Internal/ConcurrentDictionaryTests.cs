// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Collections.Concurrent
{
    public class ConcurrentDictionaryTests
    {
        [Fact]
        public void ContainsKey_ReturnsFalseWhenKeyIsNotPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act & Assert
            Assert.False(dictionary.ContainsKey(3));
        }

        [Fact]
        public void ContainsKey_ReturnsTrueWhenKeyIsPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act
            dictionary.TryAdd(1, 2);

            // Assert
            Assert.True(dictionary.ContainsKey(1));
        }

        [Fact]
        public void GetOrAdd_AddsNewValueWhenKeyIsNotPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act
            int returnedValue = dictionary.GetOrAdd(1, (key) => { return ++key; });

            // Assert
            Assert.Equal(2, returnedValue);
        }

        [Fact]
        public void GetOrAdd_ReturnsExistingValueWhenKeyIsPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, -1);

            // Act
            int returnedValue = dictionary.GetOrAdd(1, (key) => { return ++key; });

            // Assert
            Assert.Equal(-1, returnedValue);
        }

        [Fact]
        public void TryAdd_ReturnsTrueWhenKeyIsNotPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act
            bool result = dictionary.TryAdd(1, 2);

            // Assert
            Assert.True(result);
            Assert.True(dictionary.ContainsKey(1));
        }

        [Fact]
        public void TryAdd_ReturnsFalseWhenKeyIsPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, 2);

            // Act
            bool result = dictionary.TryAdd(1, 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AddOrUpdate_AddsValueWhenKeyIsNotPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act
            int result = dictionary.AddOrUpdate(1, 2, (key, current) => { return ++current; });

            // Assert
            Assert.Equal(2, result);
            Assert.Equal(2, dictionary.GetOrAdd(1, (key) => { return -1; }));
        }

        [Fact]
        public void AddOrUpdate_UpdatesValueWhenKeyIsPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, 2);

            // Act
            int result = dictionary.AddOrUpdate(1, 2, (key, current) => { return ++current; });

            // Assert
            Assert.Equal(3, result);
        }
    }
}
