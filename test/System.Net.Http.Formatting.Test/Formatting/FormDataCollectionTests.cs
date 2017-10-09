// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class FormDataCollectionTests
    {
        [Fact]
        public void CreateFromUri()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&y=2"));

            Assert.Equal("1", form.Get("x"));
            Assert.Equal("2", form.Get("y"));
        }

        [Fact]
        public void IndexerIsEquivalentToGet()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&y=2"));

            Assert.Equal("1", form.Get("x"));
            Assert.Equal(form["x"], form.Get("x"));
            Assert.Equal(form[null], form.Get(null));
        }

        [Fact]
        public void CreateFromEmptyUri()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com"));

            Assert.Empty(form);
        }

        [Fact]
        public void UriConstructorThrowsNull()
        {
            Assert.ThrowsArgumentNull(() => new FormDataCollection((Uri)null), "uri");
        }

        [Fact]
        public void CreateFromEmptyString()
        {
            FormDataCollection form = new FormDataCollection("");

            Assert.Empty(form);
        }

        [Fact]
        public void CreateFromNullString()
        {
            FormDataCollection form = new FormDataCollection((string)null);

            Assert.Empty(form);
        }

        [Fact]
        public void PairConstructorThrowsNull()
        {
            var arg = (IEnumerable<KeyValuePair<string, string>>)null;
            Assert.ThrowsArgumentNull(() => new FormDataCollection(arg), "pairs");
        }

        [Fact]
        public void CreateFromPairs()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>
            {
                { "x",  "1"},
                { "y" , "2"}
            };

            var form = new FormDataCollection(pairs);

            Assert.Equal("1", form.Get("x"));
            Assert.Equal("2", form.Get("y"));
        }

        [Fact]
        public void Enumeration()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&y=2"));

            // Enumeration should be ordered
            String s = "";
            foreach (KeyValuePair<string, string> kv in form)
            {
                s += string.Format("{0}={1};", kv.Key, kv.Value);
            }

            Assert.Equal("x=1;y=2;", s);
        }

        [Fact]
        public void GetValues()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&x=2&x=3"));

            Assert.Equal(new string[] { "1", "2", "3" }, form.GetValues("x"));
        }

        [Fact]
        public void CaseInSensitive()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&Y=2"));

            var nvc = form.ReadAsNameValueCollection();

            Assert.Equal(2, nvc.Count);
            Assert.Equal("1", nvc.Get("x"));
            Assert.Equal("2", nvc.Get("y"));
        }

        [Fact]
        public void ToNameValueCollection()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1a&y=2&x=1b&=ValueOnly&KeyOnly"));

            var nvc = form.ReadAsNameValueCollection();

            // y=2
            // x=1a;x=1b
            // =ValueOnly
            // KeyOnly
            Assert.Equal(4, nvc.Count);
            Assert.Equal(new string[] { "1a", "1b" }, nvc.GetValues("x"));
            Assert.Equal("1a,1b", nvc.Get("x"));
            Assert.Equal("2", nvc.Get("y"));
            Assert.Equal("", nvc.Get("KeyOnly"));
            Assert.Equal("ValueOnly", nvc.Get(""));
        }

        const string SPACE = " "; // single literal space character

        [Theory]
        [InlineData("x=?", "?")] // normal
        [InlineData("x=%3f", "?")] // normal
        [InlineData("x=%3d", "=")] // normal
        [InlineData("x=abc", "abc")] // normal
        [InlineData("x", "")] // key only
        [InlineData("x=", "")] // rhs only
        [InlineData("x=%20", SPACE)] // escaped space
        [InlineData("x=" + SPACE, SPACE)] // literal space
        [InlineData("x=+", SPACE)]
        [InlineData("x=null", "null")] // null literal, not escaped
        [InlineData("x=undefined", "undefined")] // undefined literal, not escaped
        [InlineData("x=\"null\"", "\"null\"")] // quoted null, preserved as is
        public void Whitespace(string queryString, string expected)
        {
            FormDataCollection fd = new FormDataCollection(queryString);

            Assert.Single(fd);
            Assert.Equal(expected, fd.Get("x"));
        }
    }
}
