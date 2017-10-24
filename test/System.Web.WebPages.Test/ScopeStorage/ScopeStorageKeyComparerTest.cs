// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class ScopeStorageKeyComparerTest
    {
        [Fact]
        public void ScopeStorageComparerPerformsCaseInsensitiveOrdinalComparisonForStrings()
        {
            // Arrange
            var dictionary = new Dictionary<object, object>(ScopeStorageComparer.Instance) { { "foo", "bar" } };

            // Act and Assert
            Assert.Equal("bar", dictionary["foo"]);
            Assert.Equal(dictionary["foo"], dictionary["FOo"]);
        }

        [Fact]
        public void ScopeStorageComparerPerformsRegularComparisonForOtherTypes()
        {
            // Arrange
            var stateStorage = new Dictionary<object, object> { { 4, "4-value" }, { new Person { ID = 10 }, "person-value" } };

            // Act and Assert
            Assert.Equal("4-value", stateStorage[4]);
            Assert.Equal(stateStorage[(int)8 / 2], stateStorage[4]);
            Assert.Equal("person-value", stateStorage[new Person { ID = 10 }]);
        }

        private class Person
        {
            public int ID { get; set; }

            public override bool Equals(object o)
            {
                var other = o as Person;
                return (other != null) && (other.ID == ID);
            }

            public override int GetHashCode()
            {
                return ID;
            }
        }
    }
}
