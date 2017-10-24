// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class NameValueCollectionExtensionsTest
    {
        [Fact]
        public void CopyTo()
        {
            // Arrange
            NameValueCollection collection = GetCollection();
            IDictionary<string, object> dictionary = GetDictionary();

            // Act
            collection.CopyTo(dictionary);

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.Equal("FooDictionary", dictionary["foo"]);
            Assert.Equal("BarDictionary", dictionary["bar"]);
            Assert.Equal("BazCollection", dictionary["baz"]);
        }

        [Fact]
        public void CopyToReplaceExisting()
        {
            // Arrange
            NameValueCollection collection = GetCollection();
            IDictionary<string, object> dictionary = GetDictionary();

            // Act
            collection.CopyTo(dictionary, true /* replaceExisting */);

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.Equal("FooCollection", dictionary["foo"]);
            Assert.Equal("BarDictionary", dictionary["bar"]);
            Assert.Equal("BazCollection", dictionary["baz"]);
        }

        [Fact]
        public void CopyToWithNullCollectionThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { NameValueCollectionExtensions.CopyTo(null /* collection */, null /* destination */); }, "collection");
        }

        [Fact]
        public void CopyToWithNullDestinationThrows()
        {
            // Arrange
            NameValueCollection collection = GetCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.CopyTo(null /* destination */); }, "destination");
        }

        private static NameValueCollection GetCollection()
        {
            return new NameValueCollection
            {
                { "Foo", "FooCollection" },
                { "Baz", "BazCollection" }
            };
        }

        private static IDictionary<string, object> GetDictionary()
        {
            return new RouteValueDictionary(new { Foo = "FooDictionary", Bar = "BarDictionary" });
        }
    }
}
