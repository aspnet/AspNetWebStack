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
            Assert.Equal(stateStorage["f"], "f2");
        }

        [Fact]
        public void ScopeStorageDictionaryOverridesParentValuesWithLocalValues()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal(stateStorage["a"], "a2");
            Assert.Equal(stateStorage["d"], "d2");
        }

        [Fact]
        public void ScopeStorageDictionaryLooksUpParentValuesWhenNotFoundLocally()
        {
            // Arrange
            var stateStorage = GetChainedStorageStateDictionary();

            // Act and Assert
            Assert.Equal(stateStorage["c"], "c0");
            Assert.Equal(stateStorage["b"], "b1");
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
            Assert.Equal(scopeStorage["a"], "a2");
            Assert.Equal(scopeStorage["b"], "b1");
            Assert.Equal(scopeStorage["c"], "c0");
            Assert.Equal(scopeStorage["d"], "d2");
            Assert.Equal(scopeStorage["e"], "e1");
            Assert.Equal(scopeStorage["f"], "f2");
        }

        [Fact]
        public void ClearRemovesAllItemsFromCurrentScope()
        {
            // Arrange
            var dictionary = new ScopeStorageDictionary { { "foo", "bar" }, { "foo2", "bar2" } };

            // Act
            dictionary.Clear();

            // Assert
            Assert.Equal(0, dictionary.Count);
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
            Assert.Equal(array[2].Key, "a");
            Assert.Equal(array[2].Value, "a2");
            Assert.Equal(array[4].Key, "f");
            Assert.Equal(array[4].Value, "f2");
            Assert.Equal(array[7].Key, "c");
            Assert.Equal(array[7].Value, "c0");
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
