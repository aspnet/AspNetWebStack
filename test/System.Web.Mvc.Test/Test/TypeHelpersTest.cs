// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class TypeHelpersTest
    {
        [Fact]
        public void CreateDelegateBindsInstanceMethod()
        {
            // Act
            string s = "Hello, world!";
            Func<string, bool> endsWith = TypeHelpers.CreateDelegate<Func<string, bool>>(TypeHelpers.MsCorLibAssembly, "System.String", "EndsWith", s);

            // Assert
            Assert.NotNull(endsWith);
            Assert.True(endsWith("world!"));
        }

        [Fact]
        public void CreateDelegateBindsStaticMethod()
        {
            // Act
            Func<object, object, string> concat = TypeHelpers.CreateDelegate<Func<object, object, string>>(TypeHelpers.MsCorLibAssembly, "System.String", "Concat", null);

            // Assert
            Assert.NotNull(concat);
            Assert.Equal("45", concat(4, 5));
        }

        [Fact]
        public void CreateDelegateReturnsNullIfTypeDoesNotExist()
        {
            // Act
            Action d = TypeHelpers.CreateDelegate<Action>(TypeHelpers.MsCorLibAssembly, "System.xyz.TypeDoesNotExist", "SomeMethod", null);

            // Assert
            Assert.Null(d);
        }

        [Fact]
        public void CreateDelegateReturnsNullIfMethodDoesNotExist()
        {
            // Act
            Action d = TypeHelpers.CreateDelegate<Action>(TypeHelpers.MsCorLibAssembly, "System.String", "MethodDoesNotExist", null);

            // Assert
            Assert.Null(d);
        }

        [Fact]
        public void CreateTryGetValueDelegateReturnsNullForNonDictionaries()
        {
            // Arrange
            object notDictionary = "Hello, world";

            // Act
            TryGetValueDelegate d = TypeHelpers.CreateTryGetValueDelegate(notDictionary.GetType());

            // Assert
            Assert.Null(d);
        }

        [Fact]
        public void CreateTryGetValueDelegateWrapsGenericObjectDictionaries()
        {
            // Arrange
            object dictionary = new Dictionary<object, int>()
            {
                { "theKey", 42 }
            };

            // Act
            TryGetValueDelegate d = TypeHelpers.CreateTryGetValueDelegate(dictionary.GetType());

            object value;
            bool found = d(dictionary, "theKey", out value);

            // Assert
            Assert.True(found);
            Assert.Equal(42, value);
        }

        [Fact]
        public void CreateTryGetValueDelegateWrapsGenericStringDictionaries()
        {
            // Arrange
            object dictionary = new Dictionary<string, int>()
            {
                { "theKey", 42 }
            };

            // Act
            TryGetValueDelegate d = TypeHelpers.CreateTryGetValueDelegate(dictionary.GetType());

            object value;
            bool found = d(dictionary, "theKey", out value);

            // Assert
            Assert.True(found);
            Assert.Equal(42, value);
        }

        [Fact]
        public void CreateTryGetValueDelegateWrapsNonGenericDictionaries()
        {
            // Arrange
            object dictionary = new Hashtable()
            {
                { "foo", 42 }
            };

            // Act
            TryGetValueDelegate d = TypeHelpers.CreateTryGetValueDelegate(dictionary.GetType());

            object fooValue;
            bool fooIsFound = d(dictionary, "foo", out fooValue);

            object barValue;
            bool barIsFound = d(dictionary, "bar", out barValue);

            // Assert
            Assert.True(fooIsFound);
            Assert.Equal(42, fooValue);
            Assert.False(barIsFound);
            Assert.Null(barValue);
        }

        [Fact]
        public void GetDefaultValue_NullableValueType()
        {
            // Act
            object defaultValue = TypeHelpers.GetDefaultValue(typeof(int?));

            // Assert
            Assert.Equal(default(int?), defaultValue);
        }

        [Fact]
        public void GetDefaultValue_ReferenceType()
        {
            // Act
            object defaultValue = TypeHelpers.GetDefaultValue(typeof(object));

            // Assert
            Assert.Equal(default(object), defaultValue);
        }

        [Fact]
        public void GetDefaultValue_ValueType()
        {
            // Act
            object defaultValue = TypeHelpers.GetDefaultValue(typeof(int));

            // Assert
            Assert.Equal(default(int), defaultValue);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfTypeIsNotNullableAndValueIsNull()
        {
            // Act
            bool retVal = TypeHelpers.IsCompatibleObject<int>(null);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsFalseIfValueIsIncorrectType()
        {
            // Arrange
            object value = new string[] { "Hello", "world" };

            // Act
            bool retVal = TypeHelpers.IsCompatibleObject<int>(value);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfTypeIsNullableAndValueIsNull()
        {
            // Act
            bool retVal = TypeHelpers.IsCompatibleObject<int?>(null);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void IsCompatibleObjectReturnsTrueIfValueIsOfCorrectType()
        {
            // Arrange
            object value = new string[] { "Hello", "world" };

            // Act
            bool retVal = TypeHelpers.IsCompatibleObject<IEnumerable<string>>(value);

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
