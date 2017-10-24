// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class WebConfigScopeStorageTest
    {
        [Fact]
        public void WebConfigScopeStorageReturnsConfigValue()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.Equal("bar1", stateStorage["foo1"]);
            Assert.Equal("bar2", stateStorage["foo2"]);
        }

        [Fact]
        public void WebConfigScopeStoragePerformsCaseInsensitiveKeyCompares()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.Equal("bar1", stateStorage["FOO1"]);
            Assert.Equal("bar2", stateStorage["FoO2"]);
        }

        [Fact]
        public void WebConfigScopeStorageThrowsWhenWriting()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Act and Assert
            Assert.Throws<NotSupportedException>(() => stateStorage["foo"] = "some value", "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Add("foo", "value"), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Remove("foo"), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Clear(), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Remove(new KeyValuePair<object, object>("foo", "bar")), "Storage scope is read only.");
        }

        [Fact]
        public void WebConfigStateAllowsEnumeratingOverConfigItems()
        {
            // Arrange
            var dictionary = new Dictionary<string, string> { { "a", "b" }, { "c", "d" }, { "x12", "y34" } };
            var stateStorage = GetWebConfigScopeStorage(dictionary);

            // Act and Assert
            Assert.True(dictionary.All(item => item.Value == stateStorage[item.Key] as string));
        }

        [Fact]
        public void WebConfigScopeStorage_WithDuplicatesInUnderlyingSettings_ResolveToTheFirst()
        {
            // Arrange
            // Start with a normal NameValueCollection
            var values = new NameValueCollection();
            values.Add("foo1", "bar1");

            // Now mess it up
            // We are simulating what might happen in rare cases (probably during app-start and high load) - 
            // the AppSettings NameValueCollection gets messed up internally, however keep functioning normally.
            // The duplication in AllKeys cause the bug https://aspnetwebstack.codeplex.com/workitem/912, 
            var entryType = typeof(NameObjectCollectionBase).GetNestedType("NameObjectEntry", BindingFlags.NonPublic);
            var entry = entryType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0].Invoke(new object[] { "foo1", "bar2" });
            var entriesArray = (ArrayList)typeof(NameObjectCollectionBase).GetField("_entriesArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(values);
            entriesArray.Add(entry);
            Assert.Equal(2, values.AllKeys.Length);

            // Act - should not throw;
            var stateStorage = new WebConfigScopeDictionary(values);

            // Assert
            Assert.Equal("bar1", stateStorage["foo1"]);
        }

        private WebConfigScopeDictionary GetWebConfigScopeStorage(IDictionary<string, string> values = null)
        {
            NameValueCollection collection = new NameValueCollection();
            if (values == null)
            {
                collection.Add("foo1", "bar1");
                collection.Add("foo2", "bar2");
            }
            else
            {
                foreach (var item in values)
                {
                    collection.Add(item.Key, item.Value);
                }
            }

            return new WebConfigScopeDictionary(collection);
        }
    }
}
