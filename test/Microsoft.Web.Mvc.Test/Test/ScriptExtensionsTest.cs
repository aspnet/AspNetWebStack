// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class ScriptExtensionsTest
    {
        [Fact]
        public void ScriptWithoutReleaseFileThrowsArgumentNullException()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Assert
            Assert.ThrowsArgumentNullOrEmpty(() => html.Script(null, "file"), "releaseFile");
        }

        [Fact]
        public void ScriptWithoutDebugFileThrowsArgumentNullException()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Assert
            Assert.ThrowsArgumentNullOrEmpty(() => html.Script("File", null), "debugFile");
        }

        [Fact]
        public void ScriptWithRootedPathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("~/Correct/Path.js", "~/Correct/Debug/Path.js");

            // Assert
            Assert.Equal("<script src=\"/Correct/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithRelativePathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("../../Correct/Path.js", "../../Correct/Debug/Path.js");

            // Assert
            Assert.Equal("<script src=\"../../Correct/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithRelativeCurrentPathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("/Correct/Path.js", "/Correct/Debug/Path.js");

            // Assert
            Assert.Equal("<script src=\"/Correct/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithScriptRelativePathRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("Correct/Path.js", "Correct/Debug/Path.js");

            // Assert
            Assert.Equal("<script src=\"/Scripts/Correct/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithUrlRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("http://ajax.Correct.com/Path.js", "http://ajax.Debug.com/Path.js");

            // Assert
            Assert.Equal("<script src=\"http://ajax.Correct.com/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithSecureUrlRendersProperElement()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());

            // Act
            MvcHtmlString result = html.Script("https://ajax.Correct.com/Path.js", "https://ajax.Debug.com/Path.js");

            // Assert
            Assert.Equal("<script src=\"https://ajax.Correct.com/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }

        [Fact]
        public void ScriptWithDebuggingOnUsesDebugUrl()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            Mock.Get(html.ViewContext.HttpContext).Setup(v => v.IsDebuggingEnabled).Returns(true);

            // Act
            MvcHtmlString result = html.Script("Correct/Path.js", "Correct/Debug/Path.js");

            // Assert
            Assert.Equal("<script src=\"/Scripts/Correct/Debug/Path.js\" type=\"text/javascript\"></script>", result.ToHtmlString());
        }
    }
}
