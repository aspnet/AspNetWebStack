// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class HttpRouteValueDictionaryTest
    {
        private static readonly Dictionary<string, object> data0 = null;
        private static readonly Dictionary<string, object> data1 = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> data2 = new Dictionary<string, object> 
            {
                { "key1", "value1" },
                { "key2", 2 },
                { "key3", TimeSpan.FromDays(1) },
            };

        public static TheoryDataSet<Dictionary<string, object>, Dictionary<string, object>> DictionaryConstructorData
        {
            get
            {
                // Input, expected output
                return new TheoryDataSet<Dictionary<string, object>, Dictionary<string, object>>
                {
                    { null, data1 },
                    { data0, data1 },
                    { data1, data1 },
                    { data2, data2 },
                };
            }
        }

        public static TheoryDataSet<object, Dictionary<string, object>> ObjectConstructorData
        {
            get
            {
                // Input, expected output
                return new TheoryDataSet<object, Dictionary<string, object>>
                {
                    { null, data1 },
                    { data0, data1 },
                    { data1, data1 },
                    { new { }, data1},
                    { data2, data2 },
                    { new { key1 = "value1", key2 = 2, key3 = TimeSpan.FromDays(1) }, data2},
                };
            }
        }

        [Theory]
        [PropertyData("DictionaryConstructorData")]
        public void Constructor_AcceptsDictionaryValues(Dictionary<string, object> input, Dictionary<string, object> expectedOutput)
        {
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary(input);
            Assert.True(expectedOutput.SequenceEqual(routeValues));
        }

        [Theory]
        [PropertyData("ObjectConstructorData")]
        public void Constructor_AcceptsObjectValues(object input, Dictionary<string, object> expectedOutput)
        {
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary(input);
            Assert.True(expectedOutput.SequenceEqual(routeValues));
        }

        [Fact]
        public void Constructor_IsCaseInsensitive()
        {
            // Arrange
            HttpRouteValueDictionary routeValues = new HttpRouteValueDictionary();

            // Act
            routeValues.Add("KEY", null);

            // Assert
            Assert.True(routeValues.ContainsKey("key"));
        }
    }
}
