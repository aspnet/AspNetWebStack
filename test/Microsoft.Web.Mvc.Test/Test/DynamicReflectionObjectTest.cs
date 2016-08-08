// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class DynamicReflectionObjectTest
    {
        [Fact]
        public void NoPropertiesThrows()
        {
            // Arrange
            dynamic dro = DynamicReflectionObject.Wrap(new { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => dro.baz,
                "The property baz doesn't exist. There are no public properties on this object.");
        }

        [Fact]
        public void UnknownPropertyThrows()
        {
            // Arrange
            dynamic dro = DynamicReflectionObject.Wrap(new { foo = 3.4, biff = "Two", bar = 1 });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => dro.baz,
                "The property baz doesn't exist. Supported properties are: bar, biff, foo.");
        }

        [Fact]
        public void CanAccessProperties()
        {
            // Arrange
            dynamic dro = DynamicReflectionObject.Wrap(new { foo = "Hello world!", bar = 42 });

            // Act & Assert
            Assert.Equal("Hello world!", dro.foo);
            Assert.Equal(42, dro.bar);
        }

        [Fact]
        public void CanAccessNestedAnonymousProperties()
        {
            // Arrange
            dynamic dro = DynamicReflectionObject.Wrap(new { foo = new { bar = "Hello world!" } });

            // Act & Assert
            Assert.Equal("Hello world!", dro.foo.bar);
        }
    }
}
