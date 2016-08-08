// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.TestCommon;

namespace System.Collections.Generic
{
    public class DictionaryExtensionsTest
    {
        public static TheoryDataSet<object> DictionaryValues
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    "test",
                    new string[] { "A", "B", "C" },
                    8,
                    new List<int> {1, 2, 3},
                    1D,
                    (IEnumerable<double>)new List<double> { 1D, 2D, 3D },
                    new Uri("http://some.host"),
                    Guid.NewGuid(),
                    HttpStatusCode.NotImplemented,
                    new HttpStatusCode[] { HttpStatusCode.Accepted, HttpStatusCode.Ambiguous, HttpStatusCode.BadGateway }
                };
            }
        }

        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(DictionaryExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void RemoveFromDictionary_Args0_EvensRemoved()
        {
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            object object1 = new object();
            object object2 = new object();
            object object3 = new object();
            object object4 = new object();
            dictionary.Add(object1, 1);
            dictionary.Add(object2, 2);
            dictionary.Add(object3, 3);
            dictionary.Add(object4, 4);

            Func<KeyValuePair<object, int>, bool> removeAction = (KeyValuePair<object, int> entry) =>
            {
                // remove even values
                return (entry.Value % 2) == 0;
            };
            dictionary.RemoveFromDictionary(removeAction);

            Assert.Equal(2, dictionary.Count);
            Assert.True(dictionary.ContainsKey(object1));
            Assert.False(dictionary.ContainsKey(object2));
            Assert.True(dictionary.ContainsKey(object3));
            Assert.False(dictionary.ContainsKey(object4));
        }

        [Fact]
        public void RemoveFromDictionary_Args1_EvensRemoved()
        {
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            object object1 = new object();
            object object2 = new object();
            object object3 = new object();
            object object4 = new object();
            dictionary.Add(object1, 1);
            dictionary.Add(object2, 2);
            dictionary.Add(object3, 3);
            dictionary.Add(object4, 4);
            object expectedArgument = new object();

            Func<KeyValuePair<object, int>, object, bool> removeAction = (KeyValuePair<object, int> entry, object arg) =>
            {
                Assert.Equal(expectedArgument, arg);
                // remove even values
                return (entry.Value % 2) == 0;
            };
            dictionary.RemoveFromDictionary(removeAction, expectedArgument);

            Assert.Equal(2, dictionary.Count);
            Assert.True(dictionary.ContainsKey(object1));
            Assert.False(dictionary.ContainsKey(object2));
            Assert.True(dictionary.ContainsKey(object3));
            Assert.False(dictionary.ContainsKey(object4));
        }

        [Fact]
        public void TryGetValueThrowsOnNullKey()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            string value;
            Assert.ThrowsArgumentNull(() => dict.TryGetValue<string>(null, out value), "key");
        }

        [Fact]
        public void TryGetValueReturnsFalse()
        {
            // Arrange
            IDictionary<string, object> dict = new Dictionary<string, object>();

            // Act
            string resultValue = null;
            bool result = dict.TryGetValue("notfound", out resultValue);

            // Assert
            Assert.False(result);
            Assert.Null(resultValue);
        }

        [Theory]
        [PropertyData("DictionaryValues")]
        public void TryGetValueReturnsTrue<T>(T value)
        {
            // Arrange
            IDictionary<string, object> dict = new Dictionary<string, object>()
            {
                { "key", value }
            };


            // Act
            T resultValue;
            bool result = DictionaryExtensions.TryGetValue(dict, "key", out resultValue);

            // Assert
            Assert.True(result);
            Assert.Equal(typeof(T), resultValue.GetType());
            Assert.Equal(value, resultValue);
        }

        [Fact]
        public void FindKeysWithPrefixRecognizesRootChilden()
        {
            // Arrange
            IDictionary<string, int> dict = new Dictionary<string, int>()
            {
                { "[0]", 1 },
                { "Name", 2 },
                { "Address.Street", 3 },
                { "", 4 }
            };

            // Act
            List<int> results = DictionaryExtensions.FindKeysWithPrefix<int>(dict, "").Select(kvp => kvp.Value).ToList();

            // Assert
            Assert.Equal(4, results.Count);
            Assert.Contains(1, results);
            Assert.Contains(2, results);
            Assert.Contains(3, results);
            Assert.Contains(4, results);
        }
    }
}
