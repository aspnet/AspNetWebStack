// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    public class FormUrlEncodedJsonTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(FormUrlEncodedJson), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        public void ParseThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => FormUrlEncodedJson.Parse(null), null);
        }

        [Fact]
        public void ParseThrowsInvalidMaxDepth()
        {
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => FormUrlEncodedJson.Parse(CreateQuery(), -1), "maxDepth", "1", -1);
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => FormUrlEncodedJson.Parse(CreateQuery(), 0), "maxDepth", "1", 0);
        }

        [Fact]
        public void ParseThrowsMaxDepthExceeded()
        {
            // Depth of 'a[b]=1' is 3
            IEnumerable<KeyValuePair<string, string>> query = CreateQuery(new KeyValuePair<string, string>("a[b]", "1"));
            Assert.ThrowsArgument(() => { FormUrlEncodedJson.Parse(query, 2); }, null);

            // This should succeed
            Assert.NotNull(FormUrlEncodedJson.Parse(query, 3));
        }

        [Fact]
        public void TryParseThrowsOnNull()
        {
            JObject value;
            Assert.ThrowsArgumentNull(() => FormUrlEncodedJson.TryParse(null, out value), null);
        }

        [Fact]
        public void TryParseThrowsInvalidMaxDepth()
        {
            JObject value;
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => FormUrlEncodedJson.TryParse(CreateQuery(), -1, out value), "maxDepth", "1", -1);
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => FormUrlEncodedJson.TryParse(CreateQuery(), 0, out value), "maxDepth", "1", 0);
        }

        [Fact]
        public void TryParseReturnsFalseMaxDepthExceeded()
        {
            JObject value;

            // Depth of 'a[b]=1' is 3
            IEnumerable<KeyValuePair<string, string>> query = CreateQuery(new KeyValuePair<string, string>("a[b]", "1"));
            Assert.False(FormUrlEncodedJson.TryParse(query, 2, out value), "Parse should have failed due to too high depth.");

            // This should succeed
            Assert.True(FormUrlEncodedJson.TryParse(query, 3, out value), "Expected non-null JsonObject instance");
            Assert.NotNull(value);
        }

        private static IEnumerable<KeyValuePair<string, string>> CreateQuery(params KeyValuePair<string, string>[] namevaluepairs)
        {
            return new List<KeyValuePair<string, string>>(namevaluepairs);
        }
    }
}