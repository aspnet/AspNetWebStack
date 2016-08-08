// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AcceptVerbsAttributeTest
    {
        private const string _invalidEnumFormatString = @"The enum '{0}' did not produce the correct array.
Expected: {1}
Actual: {2}";

        [Fact]
        public void ConstructorThrowsIfVerbsIsEmpty()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new AcceptVerbsAttribute(new string[0]); }, "verbs");
        }

        [Fact]
        public void ConstructorThrowsIfVerbsIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new AcceptVerbsAttribute((string[])null); }, "verbs");
        }

        [Fact]
        public void EnumToArray()
        {
            // Arrange
            IDictionary<string, HttpVerbs> enumValues = EnumToDictionary<HttpVerbs>();
            var allCombinations = EnumerableToCombinations(enumValues);

            // Act & assert
            foreach (var combination in allCombinations)
            {
                // generate all the names + values in this combination
                List<string> aggrNames = new List<string>();
                HttpVerbs aggrValues = (HttpVerbs)0;
                foreach (var entry in combination)
                {
                    aggrNames.Add(entry.Key);
                    aggrValues |= entry.Value;
                }

                // get the resulting array
                string[] array = AcceptVerbsAttribute.EnumToArray(aggrValues);
                var aggrNamesOrdered = aggrNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
                var arrayOrdered = array.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
                bool match = aggrNamesOrdered.SequenceEqual(arrayOrdered, StringComparer.OrdinalIgnoreCase);

                if (!match)
                {
                    string message = String.Format(_invalidEnumFormatString, aggrValues,
                                                   aggrNames.Aggregate((a, b) => a + ", " + b),
                                                   array.Aggregate((a, b) => a + ", " + b));
                    Assert.True(false, message);
                }
            }
        }

        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotInVerbsCollection()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute("get", "post");
            ControllerContext context = GetControllerContextWithHttpVerb("HEAD");

            // Act
            bool result = attr.IsValidForRequest(context, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsInVerbsCollection()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute("get", "post");
            ControllerContext context = GetControllerContextWithHttpVerb("POST");

            // Act
            bool result = attr.IsValidForRequest(context, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsOverridden()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute("put");
            ControllerContext context = GetControllerContextWithHttpVerb("POST", "PUT", null, null);

            // Act
            bool result = attr.IsValidForRequest(context, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute("get", "post");

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { attr.IsValidForRequest(null, null); }, "controllerContext");
        }

        [Fact]
        public void VerbsPropertyFromEnumConstructor()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute(HttpVerbs.Get | HttpVerbs.Post);

            // Act
            ReadOnlyCollection<string> collection = attr.Verbs as ReadOnlyCollection<string>;

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(2, collection.Count);
            Assert.Equal("GET", collection[0]);
            Assert.Equal("POST", collection[1]);
        }

        [Fact]
        public void VerbsPropertyFromStringArrayConstructor()
        {
            // Arrange
            AcceptVerbsAttribute attr = new AcceptVerbsAttribute("get", "post");

            // Act
            ReadOnlyCollection<string> collection = attr.Verbs as ReadOnlyCollection<string>;

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(2, collection.Count);
            Assert.Equal("get", collection[0]);
            Assert.Equal("post", collection[1]);
        }

        internal static ControllerContext GetControllerContextWithHttpVerb(string httpRequestVerb)
        {
            return GetControllerContextWithHttpVerb(httpRequestVerb, null, null, null);
        }

        internal static ControllerContext GetControllerContextWithHttpVerb(string httpRequestVerb, string httpHeaderVerb, string httpFormVerb, string httpQueryStringVerb)
        {
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.HttpMethod).Returns(httpRequestVerb);

            NameValueCollection headers = new NameValueCollection();
            if (!String.IsNullOrEmpty(httpHeaderVerb))
            {
                headers.Add(HttpRequestExtensions.XHttpMethodOverrideKey, httpHeaderVerb);
            }
            mockControllerContext.Setup(c => c.HttpContext.Request.Headers).Returns(headers);

            NameValueCollection form = new NameValueCollection();
            if (!String.IsNullOrEmpty(httpFormVerb))
            {
                form.Add(HttpRequestExtensions.XHttpMethodOverrideKey, httpFormVerb);
            }
            mockControllerContext.Setup(c => c.HttpContext.Request.Form).Returns(form);

            NameValueCollection queryString = new NameValueCollection();
            if (!String.IsNullOrEmpty(httpQueryStringVerb))
            {
                queryString.Add(HttpRequestExtensions.XHttpMethodOverrideKey, httpQueryStringVerb);
            }
            mockControllerContext.Setup(c => c.HttpContext.Request.QueryString).Returns(queryString);

            return mockControllerContext.Object;
        }

        private static IDictionary<string, TEnum> EnumToDictionary<TEnum>()
        {
            // Arrange
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            return values.ToDictionary(value => Enum.GetName(typeof(TEnum), value), value => value);
        }

        private static IEnumerable<ICollection<T>> EnumerableToCombinations<T>(IEnumerable<T> elements)
        {
            List<T> allElements = elements.ToList();

            int maxCount = 1 << allElements.Count;
            for (int idxCombination = 0; idxCombination < maxCount; idxCombination++)
            {
                List<T> thisCollection = new List<T>();
                for (int idxBit = 0; idxBit < 32; idxBit++)
                {
                    bool bitActive = (((uint)idxCombination >> idxBit) & 1) != 0;
                    if (bitActive)
                    {
                        thisCollection.Add(allElements[idxBit]);
                    }
                }
                yield return thisCollection;
            }
        }
    }
}
