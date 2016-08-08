// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Helpers.Test;
using Microsoft.TestCommon;

namespace Microsoft.Web.Helpers.Test
{
    public class GravatarTest
    {
        [Fact]
        public void GetUrlDefaults()
        {
            string url = Gravatar.GetUrl("foo@bar.com");
            Assert.Equal("http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80", url);
        }

        [Fact]
        public void RenderEncodesDefaultImageUrl()
        {
            string render = Gravatar.GetHtml("foo@bar.com", defaultImage: "http://example.com/images/example.jpg").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;d=http%3a%2f%2fexample.com%2fimages%2fexample.jpg\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderLowerCasesEmail()
        {
            string render = Gravatar.GetHtml("FOO@BAR.COM").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RendersValidXhtml()
        {
            XhtmlAssert.Validate1_1(Gravatar.GetHtml("foo@bar.com"));
        }

        [Fact]
        public void RenderThrowsWhenEmailIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Gravatar.GetHtml(String.Empty); }, "email");
        }

        [Fact]
        public void RenderThrowsWhenEmailIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Gravatar.GetHtml(null); }, "email");
        }

        [Fact]
        public void RenderThrowsWhenImageSizeIsLessThanZero()
        {
            Assert.ThrowsArgument(() => { Gravatar.GetHtml("foo@bar.com", imageSize: -1); }, "imageSize", "The Gravatar image size must be between 1 and 512 pixels.");
        }

        [Fact]
        public void RenderThrowsWhenImageSizeIsZero()
        {
            Assert.ThrowsArgument(() => { Gravatar.GetHtml("foo@bar.com", imageSize: 0); }, "imageSize", "The Gravatar image size must be between 1 and 512 pixels.");
        }

        [Fact]
        public void RenderThrowsWhenImageSizeIsGreaterThan512()
        {
            Assert.ThrowsArgument(() => { Gravatar.GetHtml("foo@bar.com", imageSize: 513); }, "imageSize", "The Gravatar image size must be between 1 and 512 pixels.");
        }

        [Fact]
        public void RenderTrimsEmail()
        {
            string render = Gravatar.GetHtml(" \t foo@bar.com\t\r\n").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderUsesDefaultImage()
        {
            string render = Gravatar.GetHtml("foo@bar.com", defaultImage: "wavatar").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;d=wavatar\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderUsesImageSize()
        {
            string render = Gravatar.GetHtml("foo@bar.com", imageSize: 512).ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=512\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderUsesRating()
        {
            string render = Gravatar.GetHtml("foo@bar.com", rating: GravatarRating.G).ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80&amp;r=g\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderWithAttributes()
        {
            string render = Gravatar.GetHtml("foo@bar.com",
                                             attributes: new { id = "gravatar", alT = "<b>foo@bar.com</b>", srC = "ignored" }).ToString();
            // beware of attributes ordering in tests
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alT=\"&lt;b>foo@bar.com&lt;/b>\" id=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderWithDefaults()
        {
            string render = Gravatar.GetHtml("foo@bar.com").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8?s=80\" alt=\"gravatar\" />",
                render);
        }

        [Fact]
        public void RenderWithExtension()
        {
            string render = Gravatar.GetHtml("foo@bar.com", imageExtension: ".png").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8.png?s=80\" alt=\"gravatar\" />",
                render);

            render = Gravatar.GetHtml("foo@bar.com", imageExtension: "xyz").ToString();
            Assert.Equal(
                "<img src=\"http://www.gravatar.com/avatar/f3ada405ce890b6f8204094deb12d8a8.xyz?s=80\" alt=\"gravatar\" />",
                render);
        }
    }
}
