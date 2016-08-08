// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.Test
{
    public class CssExtensionsTests
    {
        [Fact]
        public void CssWithoutFileThrowsArgumentNullException()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Assert
            Assert.ThrowsArgumentNullOrEmpty(() => html.Css(null), "file");
        }

        [Fact]
        public void CssWithRootedPathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("~/Correct/Path.css");

            // Assert
            Assert.Equal("<link href=\"/Correct/Path.css\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithRelativePathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("../../Correct/Path.css");

            // Assert
            Assert.Equal("<link href=\"../../Correct/Path.css\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithRelativeCurrentPathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("/Correct/Path.css");

            // Assert
            Assert.Equal("<link href=\"/Correct/Path.css\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithContentRelativePathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("Correct/Path.css");

            // Assert
            Assert.Equal("<link href=\"/Content/Correct/Path.css\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithNullMediaTypeRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("Correct/Path.css", null);

            // Assert
            Assert.Equal("<link href=\"/Content/Correct/Path.css\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithEmptyMediaTypeRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("Correct/Path.css", String.Empty);

            // Assert
            Assert.Equal("<link href=\"/Content/Correct/Path.css\" media=\"\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithMediaTypeRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("Correct/Path.css", "Print");

            // Assert
            Assert.Equal("<link href=\"/Content/Correct/Path.css\" media=\"Print\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithUrlRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("http://ajax.Correct.com/Path.js");

            // Assert
            Assert.Equal("<link href=\"http://ajax.Correct.com/Path.js\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }

        [Fact]
        public void CssWithSecureUrlRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Css("https://ajax.Correct.com/Path.js");

            // Assert
            Assert.Equal("<link href=\"https://ajax.Correct.com/Path.js\" rel=\"stylesheet\" type=\"text/css\" />", result.ToHtmlString());
        }
    }
}
