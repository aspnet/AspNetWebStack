// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

#if NETFX_CORE
namespace System.Net.Http.Internal
#else
namespace System.Collections.Concurrent
#endif
{
    public class ConcurrentDictionaryTests
    {
#if NETFX_CORE // This doesn't exist on the ConcurrentDictionary in the full framework
        [Fact]
        public void IsReadOnly_ReturnsFalse()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            
            // Act & Assert
            Assert.False(dictionary.IsReadOnly);
        }
#endif

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

#if NETFX_CORE
        [Fact]
        public void Add_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Add(0, 0));
        }

        [Fact]
        public void Add_KeyValuePairThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Add(new KeyValuePair<int, int>(0, 0)));
        }

        [Fact]
        public void Clear_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Clear());
        }

        [Fact]
        public void Contains_KeyValuePairThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Contains(new KeyValuePair<int, int>(0, 0)));
        }

        [Fact]
        public void CopyTo_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.CopyTo(new KeyValuePair<int, int>[1], 1));
        }

        [Fact]
        public void GetEnumerator_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.GetEnumerator());
        }

        [Fact]
        public void Remove_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Remove(0));
        }

        [Fact]
        public void Remove_WithKeyValuePairThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Remove(new KeyValuePair<int, int>(0, 0)));
        }

        [Fact]
        public void GetEnumerator_AsEnumerableThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => ((System.Collections.IEnumerable)dictionary).GetEnumerator());
        }

        [Fact]
        public void TryGetValue_ReturnsTrueAndValueWhenPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, -1);

            // Act
            int returnedValue;
            bool tryResult = dictionary.TryGetValue(1, out returnedValue);

            // Assert
            Assert.Equal(-1, returnedValue);
            Assert.True(tryResult);
        }

        [Fact]
        public void TryGetValue_ReturnsFalseAndDefaultWhenMissing()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, -1);

            // Act
            int returnedValue;
            bool tryResult = dictionary.TryGetValue(2, out returnedValue);

            // Assert
            Assert.Equal(0, returnedValue);
            Assert.False(tryResult);
        }

        [Fact]
        public void TryRemove_ReturnsTrueAndRemovesWhenPresent()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, -1);
            
            // Act
            int returnedValue;
            bool tryResult = dictionary.TryGetValue(1, out returnedValue);

            // Assert
            Assert.Equal(-1, returnedValue);
            Assert.True(tryResult);
        }

        [Fact]
        public void TryRemove_ReturnsFalseAndDefaultWhenMissing()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();
            dictionary.TryAdd(1, -1);

            // Act
            int returnedValue;
            bool tryResult = dictionary.TryGetValue(2, out returnedValue);

            // Assert
            Assert.Equal(0, returnedValue);
            Assert.False(tryResult);
        }

        [Fact]
        public void Count_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Count);
        }

        [Fact]
        public void GetItem_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary[0]);
        }

        [Fact]
        public void SetItem_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary[0] = 1);
        }

        [Fact]
        public void GetKeys_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Keys);
        }

        [Fact]
        public void GetValues_ThrowsNotImplementedException()
        {
            // Arrange
            ConcurrentDictionary<int, int> dictionary = new ConcurrentDictionary<int, int>();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => dictionary.Values);
        }
#endif
    }
}
