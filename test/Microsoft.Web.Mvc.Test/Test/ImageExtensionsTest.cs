// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.Test
{
    public class ImageExtensionsTest
    {
        [Fact]
        public void ImageWithEmptyRelativeUrlThrowsArgumentNullException()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            Assert.ThrowsArgumentNullOrEmpty(() => html.Image(null), "imageRelativeUrl");
        }

        [Fact]
        public void ImageStaticWithEmptyRelativeUrlThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNullOrEmpty(() => ImageExtensions.Image((string)null, "alt", null), "imageUrl");
        }

        [Fact]
        public void ImageWithRelativeUrlRendersProperImageTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg");
            // NOTE: Although XHTML requires an alt tag, we don't construct one for you. Specify it yourself.
            Assert.Equal("<img src=\"/system/web/mvc.jpg\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithWithAttributesWithUnderscores()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", new { foo_bar = "baz" });
            Assert.Equal("<img foo-bar=\"baz\" src=\"/system/web/mvc.jpg\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltValueRendersImageWithAltTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", "this is an alt value");
            Assert.Equal("<img alt=\"this is an alt value\" src=\"/system/web/mvc.jpg\" title=\"this is an alt value\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltValueInObjectDictionaryRendersImageWithAltAndTitleTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", new { alt = "this is an alt value" });
            Assert.Equal("<img alt=\"this is an alt value\" src=\"/system/web/mvc.jpg\" title=\"this is an alt value\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltValueHtmlAttributeEncodesAltTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", @"<"">");
            Assert.Equal("<img alt=\"&lt;&quot;>\" src=\"/system/web/mvc.jpg\" title=\"&lt;&quot;>\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltValueInObjectDictionaryHtmlAttributeEncodesAltTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", new { alt = "this is an alt value" });
            Assert.Equal("<img alt=\"this is an alt value\" src=\"/system/web/mvc.jpg\" title=\"this is an alt value\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltSpecifiedAndInDictionaryRendersExplicit()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", "specified-alt", new { alt = "object-dictionary-alt" });
            Assert.Equal("<img alt=\"object-dictionary-alt\" src=\"/system/web/mvc.jpg\" title=\"object-dictionary-alt\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithAltAndAttributesWithUnderscores()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", "specified-alt", new { foo_bar = "baz" });
            Assert.Equal("<img alt=\"specified-alt\" foo-bar=\"baz\" src=\"/system/web/mvc.jpg\" title=\"specified-alt\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithSrcSpecifiedAndInDictionaryRendersExplicit()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", new { src = "explicit.jpg" });
            Assert.Equal("<img src=\"explicit.jpg\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithOtherAttributesRendersThoseAttributesCaseSensitively()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", new { width = 100, Height = 200 });
            Assert.Equal("<img Height=\"200\" src=\"/system/web/mvc.jpg\" width=\"100\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithUrlAndDictionaryRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            var attributes = new RouteValueDictionary(new { width = 125 });
            MvcHtmlString imageResult = html.Image("/system/web/mvc.jpg", attributes);
            Assert.Equal("<img src=\"/system/web/mvc.jpg\" width=\"125\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithTildePathAndAppPathResolvesCorrectly()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary(), "/app");
            MvcHtmlString imageResult = html.Image("~/system/web/mvc.jpg");
            Assert.Equal("<img src=\"/app/system/web/mvc.jpg\" />", imageResult.ToHtmlString());
        }

        [Fact]
        public void ImageWithTildePathWithoutAppPathResolvesCorrectly()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary(), "/");
            MvcHtmlString imageResult = html.Image("~/system/web/mvc.jpg");
            Assert.Equal("<img src=\"/system/web/mvc.jpg\" />", imageResult.ToHtmlString());
        }
    }
}
