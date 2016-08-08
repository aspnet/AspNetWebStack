// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class BindAttributeTest
    {
        [Fact]
        public void PrefixProperty()
        {
            // Arrange
            BindAttribute attr = new BindAttribute { Prefix = "somePrefix" };

            // Act & assert
            Assert.Equal("somePrefix", attr.Prefix);
        }

        [Fact]
        public void PrefixPropertyDefaultsToNull()
        {
            // Arrange
            BindAttribute attr = new BindAttribute();

            // Act & assert
            Assert.Null(attr.Prefix);
        }

        [Fact]
        public void IncludePropertyDefaultsToEmptyString()
        {
            // Arrange
            BindAttribute attr = new BindAttribute { Include = null };

            // Act & assert
            Assert.Equal(String.Empty, attr.Include);
        }

        [Fact]
        public void ExcludePropertyDefaultsToEmptyString()
        {
            // Arrange
            BindAttribute attr = new BindAttribute { Exclude = null };

            // Act & assert
            Assert.Equal(String.Empty, attr.Exclude);
        }

        [Fact]
        public void IsPropertyAllowedReturnsFalseForBlacklistedPropertiesIfBindPropertiesIsExclude()
        {
            // Setup
            BindAttribute attr = new BindAttribute { Exclude = "FOO,BAZ" };

            // Act & assert
            Assert.False(attr.IsPropertyAllowed("foo"));
            Assert.True(attr.IsPropertyAllowed("bar"));
            Assert.False(attr.IsPropertyAllowed("baz"));
        }

        [Fact]
        public void IsPropertyAllowedReturnsTrueAlwaysIfBindPropertiesIsAll()
        {
            // Setup
            BindAttribute attr = new BindAttribute();

            // Act & assert
            Assert.True(attr.IsPropertyAllowed("foo"));
            Assert.True(attr.IsPropertyAllowed("bar"));
            Assert.True(attr.IsPropertyAllowed("baz"));
        }

        [Fact]
        public void IsPropertyAllowedReturnsTrueForWhitelistedPropertiesIfBindPropertiesIsInclude()
        {
            // Setup
            BindAttribute attr = new BindAttribute { Include = "FOO,BAR" };

            // Act & assert
            Assert.True(attr.IsPropertyAllowed("foo"));
            Assert.True(attr.IsPropertyAllowed("bar"));
            Assert.False(attr.IsPropertyAllowed("baz"));
        }

        [Fact]
        public void IsPropertyAllowedReturnsFalseForBlacklistOverridingWhitelistedProperties()
        {
            // Setup
            BindAttribute attr = new BindAttribute { Include = "FOO,BAR", Exclude = "bar,QUx" };

            // Act & assert
            Assert.True(attr.IsPropertyAllowed("foo"));
            Assert.False(attr.IsPropertyAllowed("bar"));
            Assert.False(attr.IsPropertyAllowed("baz"));
            Assert.False(attr.IsPropertyAllowed("qux"));
        }
    }
}
