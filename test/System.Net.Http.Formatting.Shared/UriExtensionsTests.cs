// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http.Formatting.DataSets;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Net.Http
{
    public class UriExtensionsTests
    {
        private static readonly Uri TestAddress = new Uri("http://www.example.com");
        private static readonly Type TestType = typeof(string);

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(UriExtensions), TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        public void ParseQueryStringThrowsWithNull()
        {
            Assert.ThrowsArgumentNull(() => ((Uri)null).ParseQueryString(), "address");
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "UriTestData")]
        public void ParseQueryStringSucceeds(Uri address)
        {
            var result = address.ParseQueryString();
            Assert.NotNull(result);

            bool addressContainsQuery = address.Query.Contains("?");
            if (!addressContainsQuery)
            {
                Assert.Empty(result);
            }
            else
            {
                Assert.True(result.Count > 0, "Uri with query string should return non-empty set.");
            }
        }

        [Fact]
        public void TryReadQueryAsJsonThrowsWithNull()
        {
            JObject value;
            Assert.ThrowsArgumentNull(() => ((Uri)null).TryReadQueryAsJson(out value), "address");
        }

        [Theory]
        [TestDataSet(typeof(HttpTestData), "UriTestData")]
        public void TryReadQueryAsJsonSucceeds(Uri address)
        {
            JObject value;
            Assert.True(address.TryReadQueryAsJson(out value), "Expected 'true' as result");
            Assert.NotNull(value);
            Assert.IsType<JObject>(value);
        }

        [Fact]
        public void TryReadQueryAsThrowsWithNull()
        {
            object value;
            Assert.ThrowsArgumentNull(() => ((Uri)null).TryReadQueryAs(TestType, out value), "address");
            Assert.ThrowsArgumentNull(() => TestAddress.TryReadQueryAs(null, out value), "type");
        }

        [Fact]
        public void TryReadQueryAsSucceeds()
        {
            object value;
            UriBuilder address = new UriBuilder("http://some.host");

            address.Query = "a=2";
            Assert.True(address.Uri.TryReadQueryAs(typeof(SimpleObject1), out value), "Expected 'true' reading valid data");
            SimpleObject1 so1 = (SimpleObject1)value;
            Assert.NotNull(so1);
            Assert.Equal(2, so1.a);

            address.Query = "b=true";
            Assert.True(address.Uri.TryReadQueryAs(typeof(SimpleObject2), out value), "Expected 'true' reading valid data");
            SimpleObject2 so2 = (SimpleObject2)value;
            Assert.NotNull(so2);
            Assert.True(so2.b, "Value should have been true");

            address.Query = "c=hello";
            Assert.True(address.Uri.TryReadQueryAs(typeof(SimpleObject3), out value), "Expected 'true' reading valid data");
            SimpleObject3 so3 = (SimpleObject3)value;
            Assert.NotNull(so3);
            Assert.Equal("hello", so3.c);

            address.Query = "c=";
            Assert.True(address.Uri.TryReadQueryAs(typeof(SimpleObject3), out value), "Expected 'true' reading valid data");
            so3 = (SimpleObject3)value;
            Assert.NotNull(so3);
            Assert.Equal("", so3.c);

            address.Query = "c=null";
            Assert.True(address.Uri.TryReadQueryAs(typeof(SimpleObject3), out value), "Expected 'true' reading valid data");
            so3 = (SimpleObject3)value;
            Assert.NotNull(so3);
            Assert.Equal("null", so3.c);
        }

        [Fact]
        public void TryReadQueryAsTThrowsWithNull()
        {
            object value;
            Assert.ThrowsArgumentNull(() => ((Uri)null).TryReadQueryAs<object>(out value), "address");
        }

        [Fact]
        public void TryReadQueryAsTSucceeds()
        {
            UriBuilder address = new UriBuilder("http://some.host");
            address.Query = "a=2";
            SimpleObject1 so1;
            Assert.True(address.Uri.TryReadQueryAs<SimpleObject1>(out so1), "Expected 'true' reading valid data");
            Assert.NotNull(so1);
            Assert.Equal(2, so1.a);

            address.Query = "b=true";
            SimpleObject2 so2;
            Assert.True(address.Uri.TryReadQueryAs<SimpleObject2>(out so2), "Expected 'true' reading valid data");
            Assert.NotNull(so2);
            Assert.True(so2.b, "Value should have been true");

            address.Query = "c=hello";
            SimpleObject3 so3;
            Assert.True(address.Uri.TryReadQueryAs<SimpleObject3>(out so3), "Expected 'true' reading valid data");
            Assert.NotNull(so3);
            Assert.Equal("hello", so3.c);

            address.Query = "c=";
            Assert.True(address.Uri.TryReadQueryAs<SimpleObject3>(out so3), "Expected 'true' reading valid data");
            Assert.NotNull(so3);
            Assert.Equal("", so3.c);

            address.Query = "c=null";
            Assert.True(address.Uri.TryReadQueryAs<SimpleObject3>(out so3), "Expected 'true' reading valid data");
            Assert.NotNull(so3);
            Assert.Equal("null", so3.c);
        }


        public class SimpleObject1
        {
            public int a { get; set; }
        }

        public class SimpleObject2
        {
            public bool b { get; set; }
        }

        public class SimpleObject3
        {
            public string c { get; set; }
        }
    }
}
