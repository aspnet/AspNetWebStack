// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class DictionaryHelpersTest
    {
        [Fact]
        public void DoesAnyKeyHavePrefixFailure()
        {
            // Arrange
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "FOOBAR", 42 }
            };

            // Act
            bool wasPrefixFound = DictionaryHelpers.DoesAnyKeyHavePrefix(dict, "foo");

            // Assert
            Assert.False(wasPrefixFound);
        }

        [Fact]
        public void DoesAnyKeyHavePrefixSuccess()
        {
            // Arrange
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "FOO.BAR", 42 }
            };

            // Act
            bool wasPrefixFound = DictionaryHelpers.DoesAnyKeyHavePrefix(dict, "foo");

            // Assert
            Assert.True(wasPrefixFound);
        }

        [Fact]
        public void FindKeysWithPrefix()
        {
            // Arrange
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FOO", "fooValue" },
                { "FOOBAR", "foobarValue" },
                { "FOO.BAR", "foo.barValue" },
                { "FOO[0]", "foo[0]Value" },
                { "BAR", "barValue" }
            };

            // Act
            var matchingEntries = DictionaryHelpers.FindKeysWithPrefix(dict, "foo");

            // Assert
            var matchingEntriesList = matchingEntries.OrderBy(entry => entry.Key).ToList();
            Assert.Equal(3, matchingEntriesList.Count);
            Assert.Equal("foo", matchingEntriesList[0].Key);
            Assert.Equal("fooValue", matchingEntriesList[0].Value);
            Assert.Equal("FOO.BAR", matchingEntriesList[1].Key);
            Assert.Equal("foo.barValue", matchingEntriesList[1].Value);
            Assert.Equal("FOO[0]", matchingEntriesList[2].Key);
            Assert.Equal("foo[0]Value", matchingEntriesList[2].Value);
        }

        [Fact]
        public void GetOrDefaultMissing()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            int @default = 15;

            int value = dict.GetOrDefault("two", @default);

            Assert.Equal(15, @default);
        }

        [Fact]
        public void GetOrDefaultPresent()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>() { { "one", 1 } };

            int value = dict.GetOrDefault("one", -999);

            Assert.Equal(1, 1);
        }
    }
}
