// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ValueProviderUtilTest
    {
        [Fact]
        public void CollectionContainsPrefix_EmptyCollectionReturnsFalse()
        {
            // Arrange
            string[] collection = new string[0];

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "");

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_ExactMatch()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "Hello");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_MatchIsCaseInsensitive()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "hello");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_MatchIsNotSimpleSubstringMatch()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "He");

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_NonEmptyCollectionReturnsTrueIfPrefixIsEmptyString()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_PrefixBoundaries()
        {
            // Arrange
            string[] collection = new string[] { "Hello.There[0]" };

            // Act
            bool retVal1 = ValueProviderUtil.CollectionContainsPrefix(collection, "hello");
            bool retVal2 = ValueProviderUtil.CollectionContainsPrefix(collection, "hello.there");

            // Assert
            Assert.True(retVal1);
            Assert.True(retVal2);
            Assert.True(ValueProviderUtil.CollectionContainsPrefix(collection, "hello.there[0]"));
            Assert.False(ValueProviderUtil.CollectionContainsPrefix(collection, "hello.there.0"));
        }
    }
}
