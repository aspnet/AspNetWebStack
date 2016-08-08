// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace Microsoft.Web.Helpers.Test
{
    public class GamerCardTest
    {
        [Fact]
        public void RenderThrowsWhenGamertagIsEmpty()
        {
            // Act & Assert 
            Assert.ThrowsArgumentNullOrEmptyString(() => { GamerCard.GetHtml(String.Empty).ToString(); }, "gamerTag");
        }

        [Fact]
        public void RenderThrowsWhenGamertagIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => { GamerCard.GetHtml(null).ToString(); }, "gamerTag");
        }

        [Fact]
        public void RenderGeneratesProperMarkupWithSimpleGamertag()
        {
            // Arrange 
            string expectedHtml = "<iframe frameborder=\"0\" height=\"140\" scrolling=\"no\" src=\"http://gamercard.xbox.com/osbornm.card\" width=\"204\">osbornm</iframe>";

            // Act
            string html = GamerCard.GetHtml("osbornm").ToHtmlString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, html);
        }

        [Fact]
        public void RenderGeneratesProperMarkupWithComplexGamertag()
        {
            // Arrange 
            string expectedHtml = "<iframe frameborder=\"0\" height=\"140\" scrolling=\"no\" src=\"http://gamercard.xbox.com/matthew%20osborn&#39;s.card\" width=\"204\">matthew osborn&#39;s</iframe>";

            // Act
            string html = GamerCard.GetHtml("matthew osborn's").ToHtmlString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, html);
        }

        [Fact]
        public void RenderGeneratesValidXhtml()
        {
            XhtmlAssert.Validate1_0(
                GamerCard.GetHtml("osbornm")
                );
        }
    }
}
