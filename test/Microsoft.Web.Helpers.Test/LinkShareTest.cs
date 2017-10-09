// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;

namespace Microsoft.Web.Helpers.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class LinkShareTest : IDisposable
    {
        private static LinkShareSite[] _allLinkShareSites = new[]
        {
            LinkShareSite.Delicious, LinkShareSite.Digg,
            LinkShareSite.Facebook, LinkShareSite.Reddit, LinkShareSite.StumbleUpon, LinkShareSite.Twitter
        };

        [Fact]
        public void RenderWithFacebookFirst_ReturnsHtmlWithFacebookAndThenOthersTest()
        {
            string pageTitle = "page1";
            string pageLinkBack = "page link back";
            string twitterUserName = String.Empty;
            string twitterTag = String.Empty;
            string actual;
            actual = LinkShare.GetHtml(pageTitle, pageLinkBack, twitterUserName, twitterTag, LinkShareSite.Facebook, LinkShareSite.All).ToString();
            Assert.Contains("twitter.com", actual);
            int pos = actual.IndexOf("facebook.com");
            Assert.True(pos > 0);
            int pos2 = actual.IndexOf("reddit.com");
            Assert.True(pos2 > pos);
            pos2 = actual.IndexOf("digg.com");
            Assert.True(pos2 > pos);
        }

        [Fact]
        public void BitlyApiKeyThrowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => LinkShare.BitlyApiKey = null, "value");
        }

        [Fact]
        public void BitlyApiKeyUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            LinkShare.BitlyApiKey = value;

            // Assert
            Assert.Equal(LinkShare.BitlyApiKey, value);
            Assert.Equal(ScopeStorage.CurrentScope[LinkShare._bitlyApiKey], value);
        }

        [Fact]
        public void BitlyLoginThrowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => LinkShare.BitlyLogin = null, "value");
        }

        [Fact]
        public void BitlyLoginUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            LinkShare.BitlyLogin = value;

            // Assert
            Assert.Equal(LinkShare.BitlyLogin, value);
            Assert.Equal(ScopeStorage.CurrentScope[LinkShare._bitlyLogin], value);
        }

        [Fact]
        public void RenderWithNullPageTitle_ThrowsException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(
                () => LinkShare.GetHtml(null).ToString(),
                "pageTitle");
        }

        [Fact]
        public void Render_WithFacebook_Works()
        {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: LinkShareSite.Facebook).ToString();
            string expectedHTML =
                "<a href=\"http://www.facebook.com/sharer.php?u=www.foo.com&amp;t=page-title\" target=\"_blank\" title=\"Share on Facebook\"><img alt=\"Share on Facebook\" src=\"http://facebook.com/favicon.ico\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Share on Facebook\" /></a>";
            UnitTestHelper.AssertEqualsIgnoreWhitespace(actualHTML, expectedHTML);
        }

        [Fact]
        public void Render_WithFacebookAndDigg_Works()
        {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: new[] { LinkShareSite.Facebook, LinkShareSite.Digg }).ToString();
            string expectedHTML =
                "<a href=\"http://www.facebook.com/sharer.php?u=www.foo.com&amp;t=page-title\" target=\"_blank\" title=\"Share on Facebook\"><img alt=\"Share on Facebook\" src=\"http://facebook.com/favicon.ico\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Share on Facebook\" /></a><a href=\"http://digg.com/submit?url=www.foo.com&amp;title=page-title\" target=\"_blank\" title=\"Digg!\"><img alt=\"Digg!\" src=\"http://digg.com/favicon.ico\" style=\"border:0; height:16px; width:16px; margin:0 1px;\" title=\"Digg!\" /></a>";
            UnitTestHelper.AssertEqualsIgnoreWhitespace(actualHTML, expectedHTML);
        }

        [Fact]
        public void Render_WithFacebook_RendersAnchorTitle()
        {
            string actualHTML = LinkShare.GetHtml("page-title", "www.foo.com", linkSites: LinkShareSite.Facebook).ToString();
            string expectedHtml = @"<a href=""http://www.facebook.com/sharer.php?u=www.foo.com&amp;t=page-title"" target=""_blank"" title=""Share on Facebook"">
                <img alt=""Share on Facebook"" src=""http://facebook.com/favicon.ico"" style=""border:0; height:16px; width:16px; margin:0 1px;"" title=""Share on Facebook"" />
                </a>";

            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHTML);
        }

        [Fact]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenArgumentIsNull()
        {
            // Act and Assert
            var result = LinkShare.GetSitesInOrder(linkSites: null);

            Assert.Equal(_allLinkShareSites, result.ToArray());
        }

        [Fact]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenArgumentIEmpty()
        {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new LinkShareSite[] { });

            // Assert
            Assert.Equal(_allLinkShareSites, result.ToArray());
        }

        [Fact]
        public void LinkShare_GetSitesInOrderDoesNotReturnsGoogleBuzzForAll() {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new LinkShareSite[] { LinkShareSite.All });

            // Assert
            // 2 is the deprecated value for GoogleBuzz
            Assert.DoesNotContain(((LinkShareSite)2), result.ToArray());
        }

        [Fact]
        public void LinkShare_GetSitesInOrderReturnsAllSitesWhenAllIsFirstItem()
        {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new[] { LinkShareSite.All, LinkShareSite.Reddit });

            // Assert
            Assert.Equal(_allLinkShareSites, result.ToArray());
        }

        [Fact]
        public void LinkShare_GetSitesInOrderReturnsSitesInOrderWhenAllIsNotFirstItem()
        {
            // Act
            var result = LinkShare.GetSitesInOrder(linkSites: new[] { LinkShareSite.Reddit, LinkShareSite.Facebook, LinkShareSite.All });

            // Assert
            Assert.Equal(new[]
            {
                LinkShareSite.Reddit, LinkShareSite.Facebook, LinkShareSite.Delicious, LinkShareSite.Digg, LinkShareSite.StumbleUpon, LinkShareSite.Twitter
            }, result.ToArray());
        }

        [Fact]
        public void LinkShare_EncodesParameters()
        {
            // Arrange
            var expectedHtml =
                @"<a href=""http://reddit.com/submit?url=www.foo.com&amp;title=%26%26"" target=""_blank"" title=""Reddit!"">
                    <img alt=""Reddit!"" src=""http://www.Reddit.com/favicon.ico"" style=""border:0; height:16px; width:16px; margin:0 1px;"" title=""Reddit!"" />
                </a>
                <a href=""http://twitter.com/home/?status=%26%26%3a+www.foo.com%2c+(via+%40%40%3cTweeter+Bot%3e)+I+%3c3+Tweets"" target=""_blank"" title=""Share on Twitter"">
                    <img alt=""Share on Twitter"" src=""http://twitter.com/favicon.ico"" style=""border:0; height:16px; width:16px; margin:0 1px;"" title=""Share on Twitter"" />
                </a>";

            // Act
            var actualHtml = LinkShare.GetHtml("&&", "www.foo.com", "<Tweeter Bot>", "I <3 Tweets", LinkShareSite.Reddit, LinkShareSite.Twitter).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void LinkshareRendersValidXhtml()
        {
            string result = "<html> <head> \n <title> </title> \n </head> \n <body> <div> \n" +
                            LinkShare.GetHtml("any<>title", "my test page <>") +
                            "\n </div> </body> \n </html>";
            HtmlString htmlResult = new HtmlString(result);
            XhtmlAssert.Validate1_0(htmlResult);
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. LinkShare.BitlyApiKey) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
