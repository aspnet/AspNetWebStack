// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class DynamicViewDataDictionaryTest
    {
        // Property-style accessor

        [Fact]
        public void Property_UnknownItemReturnsEmptyString()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd.Foo;

            // Assert
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void Property_CanAccessViewDataValues()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Foo"] = "Value for Foo";
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd.Foo;

            // Assert
            Assert.Equal("Value for Foo", result);
        }

        [Fact]
        public void Property_CanAccessModelProperties()
        {
            ViewDataDictionary vdd = new ViewDataDictionary(new { Foo = "Value for Foo" });
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd.Foo;

            // Assert
            Assert.Equal("Value for Foo", result);
        }

        // Index-style accessor

        [Fact]
        public void Indexer_GuardClauses()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => { var x = dvdd["foo", "bar"]; },
                "DynamicViewDataDictionary only supports single indexers.");

            Assert.Throws<ArgumentException>(
                () => { var x = dvdd[42]; },
                "DynamicViewDataDictionary only supports string-based indexers.");
        }

        [Fact]
        public void Indexer_UnknownItemReturnsEmptyString()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd["Foo"];

            // Assert
            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void Indexer_CanAccessViewDataValues()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Foo"] = "Value for Foo";
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd["Foo"];

            // Assert
            Assert.Equal("Value for Foo", result);
        }

        [Fact]
        public void Indexer_CanAccessModelProperties()
        {
            ViewDataDictionary vdd = new ViewDataDictionary(new { Foo = "Value for Foo" });
            dynamic dvdd = DynamicViewDataDictionary.Wrap(vdd);

            // Act
            object result = dvdd["Foo"];

            // Assert
            Assert.Equal("Value for Foo", result);
        }
    }
}
