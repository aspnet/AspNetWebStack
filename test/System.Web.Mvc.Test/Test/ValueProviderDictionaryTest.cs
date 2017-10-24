// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using System.Web.Routing;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

#pragma warning disable 0618    // ValueProviderDictionary is now obsolete

namespace System.Web.Mvc.Test
{
    public class ValueProviderDictionaryTest
    {
        [Fact]
        public void ConstructorCreatesEmptyDictionaryIfControllerContextIsNull()
        {
            // Act
            ValueProviderDictionary dict = new ValueProviderDictionary(null);

            // Assert
            Assert.Empty(dict);
        }

        [Fact]
        public void ControllerContextProperty()
        {
            // Arrange
            ControllerContext expected = GetControllerContext();
            ValueProviderDictionary dict = new ValueProviderDictionary(expected);

            // Act
            ControllerContext returned = dict.ControllerContext;

            // Assert
            Assert.Equal(expected, returned);
        }

        [Fact]
        public void DictionaryInterface()
        {
            // Arrange
            DictionaryHelper<string, ValueProviderResult> helper = new DictionaryHelper<string, ValueProviderResult>()
            {
                Creator = () => new ValueProviderDictionary(null),
                Comparer = StringComparer.OrdinalIgnoreCase,
                SampleKeys = new string[] { "foo", "bar", "baz", "quux", "QUUX" },
                SampleValues = new ValueProviderResult[]
                {
                    new ValueProviderResult(null, null, null),
                    new ValueProviderResult(null, null, null),
                    new ValueProviderResult(null, null, null),
                    new ValueProviderResult(null, null, null),
                    new ValueProviderResult(null, null, null)
                },
                ThrowOnKeyNotFound = false
            };

            // Act & assert
            helper.Execute();
        }

        [Fact]
        public void AddWithRawValueUsesInvariantCulture()
        {
            // Arrange
            ValueProviderDictionary dict = new ValueProviderDictionary(null);

            // Act
            dict.Add("foo", 42);

            // Assert
            ValueProviderResult vpResult = dict["foo"];
            Assert.Equal("42", vpResult.AttemptedValue);
            Assert.Equal(42, vpResult.RawValue);
            Assert.Equal(CultureInfo.InvariantCulture, vpResult.Culture);
        }

        [Fact]
        public void NullAndEmptyKeysAreIgnored()
        {
            // DevDiv Bugs #216667: Exception thrown when querystring contains name without value

            // Arrange
            ValueProviderDictionary dict = GetAndPopulateDictionary();

            // Act
            bool emptyKeyFound = dict.ContainsKey(String.Empty);

            // Assert
            Assert.False(emptyKeyFound);
        }

        [Fact]
        public void ValueFromForm()
        {
            // Arrange
            ValueProviderDictionary dict;

            // Act
            using (new CultureReplacer("fr-FR"))
            {
                dict = GetAndPopulateDictionary();
            }
            ValueProviderResult result = dict["foo"];

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fooFromForm", result.AttemptedValue);
            string[] stringValues = Assert.IsType<string[]>(result.RawValue);
            string stringValue = Assert.Single(stringValues);
            Assert.Equal("fooFromForm", stringValue);
            Assert.Equal(CultureInfo.GetCultureInfo("fr-FR"), result.Culture);
        }

        [Fact]
        public void ValueFromQueryString()
        {
            // Arrange
            ValueProviderDictionary dict;

            // Act
            using (new CultureReplacer("fr-FR"))
            {
                dict = GetAndPopulateDictionary();
            }
            ValueProviderResult result = dict["baz"];

            // Assert
            Assert.NotNull(result);
            Assert.Equal("bazFromQueryString", result.AttemptedValue);
            string[] stringValues = Assert.IsType<string[]>(result.RawValue);
            string stringValue = Assert.Single(stringValues);
            Assert.Equal("bazFromQueryString", stringValue);
            Assert.Equal(CultureInfo.InvariantCulture, result.Culture);
        }

        [Fact]
        public void ValueFromRoute()
        {
            // Arrange
            ValueProviderDictionary dict;

            // Act
            using (new CultureReplacer("fr-FR"))
            {
                dict = GetAndPopulateDictionary();
            }
            ValueProviderResult result = dict["bar"];

            // Assert
            Assert.NotNull(result);
            Assert.Equal("barInRoute", result.AttemptedValue);
            Assert.Equal("barInRoute", result.RawValue);
            Assert.Equal(CultureInfo.InvariantCulture, result.Culture);
        }

        private static ValueProviderDictionary GetAndPopulateDictionary()
        {
            return new ValueProviderDictionary(GetControllerContext());
        }

        private static ControllerContext GetControllerContext()
        {
            NameValueCollection form = new NameValueCollection() { { "foo", "fooFromForm" } };

            RouteData rd = new RouteData();
            rd.Values["foo"] = "fooFromRoute";
            rd.Values["bar"] = "barInRoute";

            NameValueCollection queryString = new NameValueCollection()
            {
                { "foo", "fooFromQueryString" },
                { "bar", "barInQueryString" },
                { "baz", "bazFromQueryString" },
                { null, "nullValue" },
                { "", "emptyStringValue" }
            };

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Form).Returns(form);
            mockControllerContext.Setup(c => c.HttpContext.Request.QueryString).Returns(queryString);
            mockControllerContext.Setup(c => c.RouteData).Returns(rd);
            return mockControllerContext.Object;
        }
    }
}
