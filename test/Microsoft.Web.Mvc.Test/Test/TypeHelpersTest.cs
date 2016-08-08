// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class TypeHelpersTest
    {
        [Fact]
        public void GetTypeArgumentsIfMatch_ClosedTypeIsGenericAndMatches_ReturnsType()
        {
            // Act
            Type[] typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(typeof(List<int>), typeof(List<>));

            // Assert
            Assert.Equal(new[] { typeof(int) }, typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsIfMatch_ClosedTypeIsGenericButDoesNotMatch_ReturnsNull()
        {
            // Act
            Type[] typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(typeof(int?), typeof(List<>));

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void GetTypeArgumentsIfMatch_ClosedTypeIsNotGeneric_ReturnsNull()
        {
            // Act
            Type[] typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(typeof(int), null);

            // Assert
            Assert.Null(typeArguments);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfTypeIsNotNullableAndValueIsNull()
        {
            // Act
            bool retVal = TypeHelpers.IsCompatibleObject(typeof(int), null);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsFalseIfValueIsIncorrectType()
        {
            // Arrange
            object value = new[] { "Hello", "world" };

            // Act
            bool retVal = TypeHelpers.IsCompatibleObject(typeof(int), value);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfTypeIsNullableAndValueIsNull()
        {
            // Act
            bool retVal = TypeHelpers.IsCompatibleObject(typeof(int?), null);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfValueIsOfCorrectType()
        {
            // Arrange
            object value = new[] { "Hello", "world" };

            // Act
            bool retVal = TypeHelpers.IsCompatibleObject(typeof(IEnumerable<string>), value);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void TypeAllowsNullValueReturnsFalseForNonNullableGenericValueType()
        {
            Assert.False(TypeHelpers.TypeAllowsNullValue(typeof(KeyValuePair<int, string>)));
        }

        [Fact]
        public void TypeAllowsNullValueReturnsFalseForNonNullableGenericValueTypeDefinition()
        {
            Assert.False(TypeHelpers.TypeAllowsNullValue(typeof(KeyValuePair<,>)));
        }

        [Fact]
        public void TypeAllowsNullValueReturnsFalseForNonNullableValueType()
        {
            Assert.False(TypeHelpers.TypeAllowsNullValue(typeof(int)));
        }

        [Fact]
        public void TypeAllowsNullValueReturnsTrueForInterfaceType()
        {
            Assert.True(TypeHelpers.TypeAllowsNullValue(typeof(IDisposable)));
        }

        [Fact]
        public void TypeAllowsNullValueReturnsTrueForNullableType()
        {
            Assert.True(TypeHelpers.TypeAllowsNullValue(typeof(int?)));
        }

        [Fact]
        public void TypeAllowsNullValueReturnsTrueForReferenceType()
        {
            Assert.True(TypeHelpers.TypeAllowsNullValue(typeof(object)));
        }
    }
}
