// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;
using System.Web.WebPages.Html;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class HtmlHelperTest
    {
        [Fact]
        public void ValidationInputCssClassNameThrowsWhenAssignedNull()
        {
            // Act and Assert
            Assert.ThrowsArgumentNull(() => HtmlHelper.ValidationInputCssClassName = null, "value");
        }

        [Fact]
        public void ValidationSummaryClassNameThrowsWhenAssignedNull()
        {
            // Act and Assert
            Assert.ThrowsArgumentNull(() => HtmlHelper.ValidationSummaryClass = null, "value");
        }

        [Fact]
        public void EncodeObject()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            object text = "<br />" as object;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal("&lt;br /&gt;", encodedHtml);
        }

        [Fact]
        public void EncodeObjectNull()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            object text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal(String.Empty, encodedHtml);
        }

        [Fact]
        public void EncodeString()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            var text = "<br />";

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal("&lt;br /&gt;", encodedHtml);
        }

        [Fact]
        public void EncodeStringNull()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            string text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        [Fact]
        public void RawAllowsNullValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(null);

            // Assert
            Assert.Null(markupHtml.ToString());
            Assert.Null(markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsNullObjectValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw((object)null);

            // Assert
            Assert.Null(markupHtml.ToString());
            Assert.Null(markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsEmptyValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw("");

            // Assert
            Assert.Equal("", markupHtml.ToString());
            Assert.Equal("", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawReturnsWrapperMarkup()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            string markup = "<b>bold</b>";

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(markup);

            // Assert
            Assert.Equal("<b>bold</b>", markupHtml.ToString());
            Assert.Equal("<b>bold</b>", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawReturnsWrapperMarkupOfObject()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            ObjectWithWrapperMarkup obj = new ObjectWithWrapperMarkup();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(obj);

            // Assert
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToString());
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToHtmlString());
        }

        [Fact]
        public void ConvertsUnderscoresInNamesToDashes()
        {
            // Arrange
            var attributes = GetAttributes();

            // Act
            RouteValueDictionary result = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);

            // Assert
            Assert.Equal(7, result.Count);
            Assert.Equal("Bar", result["foo"]);
            Assert.Equal("pow_wow", result["baz-bif"]);
        }

        [Fact]
        public void ObjectToDictionaryWithAnonymousTypeLooksUpProperties()
        {
            // Arrange
            object obj = new { _test = "value", oth_er = 1 };

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(obj);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(2, dictValues.Count);
            Assert.Equal("value", dictValues["_test"]);
            Assert.Equal(1, dictValues["oth_er"]);
        }

        private static object GetAttributes()
        {
            return new { foo = "Bar",
                         baz_bif = "pow_wow",
                         other1 = "xx",
                         other2 = "yy",
                         other3 = "zz",
                         other4 = "aa",
                         other5 = "bb",
                       };
        }

        /// <summary>
        /// Will invoke a helper with overload that accepts custom attribute with a name containing
        /// and underscore as an anonymous object, and will then assert that the resulted html
        /// has the attribute name underscore correctly transformed to a dash
        /// </summary>
        /// <param name="helperInvocation"></param>
        internal static void AssertHelperTransformsAttributesUnderscoresToDashs(Func<HtmlHelper, object, IHtmlString> helperInvocation)
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();
            const string expected = @"data-name=""value""";
            const string unexpected = @"data_name=""value""";
            var attributes = new { data_name = "value" };

            // Act
            var htmlString = helperInvocation(helper, attributes).ToHtmlString();

            // Assert
            Assert.DoesNotContain(unexpected, htmlString);
            Assert.Contains(expected, htmlString);
        }

        private class ObjectWithWrapperMarkup
        {
            public override string ToString()
            {
                return "<b>boldFromObject</b>";
            }
        }
    }
}
