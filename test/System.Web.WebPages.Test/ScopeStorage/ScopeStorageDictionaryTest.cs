// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class ScopeStorageDictionaryTest
    {
        [Fact]
        public void ScopeStorageDictionaryLooksUpLocalValuesFirst()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal("f2", stateStorage["f"]);
        }

        [Fact]
        public void ScopeStorageDictionaryOverridesParentValuesWithLocalValues()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal("a2", stateStorage["a"]);
            Assert.Equal("d2", stateStorage["d"]);
        }

        [Fact]
        public void ScopeStorageDictionaryLooksUpParentValuesWhenNotFoundLocally()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal("c0", stateStorage["c"]);
            Assert.Equal("b1", stateStorage["b"]);
        }

        [Fact]
        public void ScopeStorageDictionaryTreatsNullAsOrdinaryValues()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();
            stateStorage["b"] = null;

            // Act and Assert
            Assert.Null(stateStorage["b"]);
        }

        [Fact]
        public void ContainsKeyReturnsTrueIfItContainsKey()
        {
            // Arrange
            var scopeStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.True(scopeStorage.ContainsKey("f"));
        }

        [Fact]
        public void ContainsKeyReturnsTrueIfBaseContainsKey()
        {
            // Arrange
            var scopeStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.True(scopeStorage.ContainsKey("e"));
        }

        [Fact]
        public void ContainsKeyReturnsFalseIfItDoesNotContainKeyAndBaseIsNull()
        {
            // Arrange
            var scopeStorage = new ScopeStorageDictionary() { { "foo", "bar" } };

            // Act and Assert
            Assert.False(scopeStorage.ContainsKey("baz"));
        }

        [Fact]
        public void CountReturnsCountFromCurrentAndBaseScope()
        {
            // Arrange
            var scopeStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal(6, scopeStorage.Count);
        }

        [Fact]
        public void ScopeStorageDictionaryGetsValuesFromCurrentAndBaseScope()
        {
            // Arrange
            var scopeStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal("a2", scopeStorage["a"]);
            Assert.Equal("b1", scopeStorage["b"]);
            Assert.Equal("c0", scopeStorage["c"]);
            Assert.Equal("d2", scopeStorage["d"]);
            Assert.Equal("e1", scopeStorage["e"]);
            Assert.Equal("f2", scopeStorage["f"]);
        }

        [Fact]
        public void ClearRemovesAllItemsFromCurrentScope()
        {
            // Arrange
            var dictionary = new ScopeStorageDictionary { { "foo", "bar" }, { "foo2", "bar2" } };

            // Act
            dictionary.Clear();

            // Assert
            Assert.Empty(dictionary);
        }

        [Fact]
        public void ScopeStorageDictionaryIsNotReadOnly()
        {
            // Arrange
            var dictionary = new ScopeStorageDictionary();

            // Act and Assert
            Assert.False(dictionary.IsReadOnly);
        }

        [Fact]
        public void CopyToCopiesItemsToArrayAtSpecifiedIndex()
        {
            // Arrange
            var dictionary = GetChainedStorageStateDictionary();
            var array = new KeyValuePair<object, object>[8];

            // Act
            dictionary.CopyTo(array, 2);

            // Assert
            Assert.Equal("a", array[2].Key);
            Assert.Equal("a2", array[2].Value);
            Assert.Equal("f", array[4].Key);
            Assert.Equal("f2", array[4].Value);
            Assert.Equal("c", array[7].Key);
            Assert.Equal("c0", array[7].Value);
        }

        private ScopeStorageDictionary GetChainedStorageStateDictionary()
        {
            var root = new ScopeStorageDictionary();
            root["a"] = "a0";
            root["b"] = "b0";
            root["c"] = "c0";

            var firstGen = new ScopeStorageDictionary(baseScope: root);
            firstGen["a"] = "a1";
            firstGen["b"] = "b1";
            firstGen["d"] = "d1";
            firstGen["e"] = "e1";

            var secondGen = new ScopeStorageDictionary(baseScope: firstGen);
            secondGen["a"] = "a2";
            secondGen["d"] = "d2";
            secondGen["f"] = "f2";

            return secondGen;
        }
    }
}
