// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.TestUtil;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class FacebookTest : IDisposable
    {
        public FacebookTest()
        {
            Facebook.AppId = "myapp'";
            Facebook.AppSecret = "myappsecret";
            Facebook.Language = "french";
        }

        [Fact]
        public void GetFacebookCookieInfoReturnsEmptyStringIfCookieIsNotPresent()
        {
            // Arrange
            var context = CreateHttpContext();

            // Act
            var info = Facebook.GetFacebookCookieInfo(context, "foo");

            // Assert
            Assert.Equal("", info);
        }

        [Fact]
        public void GetFacebookCookieInfoThrowsIfCookieIsNotValid()
        {
            // Arrange

            var context = CreateHttpContext(new Dictionary<string, string>
            {
                {"fbs_myapp'", "sig=malformed-signature&name=foo&val=bar&uid=MyFacebookName"},
                {"fbs_uid", "MyFacebookName"},
            });

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => Facebook.GetFacebookCookieInfo(context, "uid"), "Invalid Facebook cookie.");
        }

        [Fact]
        public void GetFacebookCookieReturnsUserIdIfCookieIsValid()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, string>
            {
                {"fbs_myapp'", "sig=B2E6B3A21D0C9FA72E612BD6C3084807&name=foo&val=bar&uid=MyFacebookName"},
            });

            // Act
            var info = Facebook.GetFacebookCookieInfo(context, "uid");

            // Assert
            Assert.Equal("MyFacebookName", info);
        }

        [Fact]
        public void GetInitScriptsJSEncodesParameters()
        {
            // Arrange
            var expectedText = @"
                <div id=""fb-root""></div>
                <script type=""text/javascript"">
                    window.fbAsyncInit = function () {
                        FB.init({ appId: 'MyApp\u0027', status: true, cookie: true, xfbml: true });
                    };
                    (function () {
                        var e = document.createElement('script'); e.async = true;
                        e.src = document.location.protocol +
                        '//connect.facebook.net/French/all.js';
                        document.getElementById('fb-root').appendChild(e);
                    } ());

                    function loginRedirect(url) { window.location = url; }
                </script>
            ";

            // Act
            var actualText = Facebook.GetInitializationScripts();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void LoginButtonTest()
        {
            // Arrange
            var expected40 = @"<fb:login-button autologoutlink=""True"" size=""extra-small"" length=""extra-long"" onlogin=""loginRedirect(&#39;http://www.test.com/facebook/?registerUrl=http%3a%2f%2fwww.test.com%2f&amp;returnUrl=http%3a%2f%2fww.test.com%2fLogin.cshtml&#39;)"" show-faces=""True"" perms=""email,none&quot;"">Awesome &quot;button text&quot;</fb:login-button>";
            var expected45 = @"<fb:login-button autologoutlink=""True"" size=""extra-small"" length=""extra-long"" onlogin=""loginRedirect(&#39;http://www.test.com/facebook/?registerUrl=http%3a%2f%2fwww.test.com%2f\u0026returnUrl=http%3a%2f%2fww.test.com%2fLogin.cshtml&#39;)"" show-faces=""True"" perms=""email,none&quot;"">Awesome &quot;button text&quot;</fb:login-button>";

            // Act
            var actualText = Facebook.LoginButton("http://www.test.com", "http://ww.test.com/Login.cshtml", "http://www.test.com/facebook/", "Awesome \"button text\"", true, "extra-small", "extra-long", true, "none\"");

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(RuntimeEnvironment.IsVersion45Installed ? expected45 : expected40, actualText.ToString());
        }

        [Fact]
        public void LoginButtonOnlyTagTest()
        {
            // Arrange
            var expectedText = @"<fb:login-button autologoutlink=""True"" size=""small"" length=""medium"" onlogin=""foobar();"" show-faces=""True"" perms=""none&quot;"">&quot;Awesome button text&quot;</fb:login-button>";

            // Act
            var actualText = Facebook.LoginButtonTagOnly("\"Awesome button text\"", true, "small", "medium", "foobar();", true, "none\"");

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void LikeButtonTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/like.php?href=http%3a%2f%2fsomewebsite&amp;layout=modern&amp;show_faces=false&amp;width=300&amp;action=hop&amp;colorscheme=lighter&amp;height=30&amp;font=Comic+Sans&amp;locale=French&amp;ref=foo+bar"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:300px; height:30px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.LikeButton("http://somewebsite", "modern", false, 300, 30, "hop", "Comic Sans", "lighter", "foo bar");

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void CommentsWithNoXidTest()
        {
            // Arrange
            var expectedText = @"<fb:comments numposts=""3"" width=""300"" reverse=""true"" simple=""true"" ></fb:comments>";

            // Act
            var actualText = Facebook.Comments(null, 300, 3, true, true);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void CommentsWithXidTest()
        {
            // Arrange
            var expectedText = @"<fb:comments xid=""bar"" numposts=""3"" width=""300"" reverse=""true"" simple=""true"" ></fb:comments>";

            // Act
            var actualText = Facebook.Comments("bar", 300, 3, true, true);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void RecommendationsTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/recommendations.php?site=http%3a%2f%2fsomesite&amp;width=100&amp;height=200&amp;header=False&amp;colorscheme=blue&amp;font=none&amp;border_color=black&amp;filter=All+posts&amp;ref=ref+label&amp;locale=french"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:100px; height:200px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.Recommendations("http://somesite", 100, 200, false, "blue", "none", "black", "All posts", "ref label");

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void LikeBoxTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/likebox.php?href=http%3a%2f%2fsomesite&amp;width=100&amp;height=200&amp;header=False&amp;colorscheme=blue&amp;connections=5&amp;stream=True&amp;header=False&amp;locale=french"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:100px; height:200px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.LikeBox("http://somesite", 100, 200, "blue", 5, true, false);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void FacepileTest()
        {
            // Arrange
            var expectedText = @"<fb:facepile max-rows=""3"" width=""100""></fb:facepile>";

            // Act
            var actualText = Facebook.Facepile(3, 100);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void LiveStreamWithEmptyXidTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/live_stream_box.php?app_id=myapp%27&amp;width=100&amp;height=100&amp;always_post_to_friends=True&amp;locale=french"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:100px; height:100px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.LiveStream(100, 100, "", "", true);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void LiveStreamWithXidValueTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/live_stream_box.php?app_id=myapp%27&amp;width=100&amp;height=100&amp;always_post_to_friends=True&amp;locale=french&amp;xid=some-val&amp;via_url=http%3a%2f%2fmysite"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:100px; height:100px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.LiveStream(100, 100, "some-val", "http://mysite", true);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void ActivityStreamTest()
        {
            // Arrange
            var expectedText = @"<iframe src=""http://www.facebook.com/plugins/activity.php?site=http%3a%2f%2fmysite&amp;width=100&amp;height=120&amp;header=False&amp;colorscheme=gray&amp;font=Arial&amp;border_color=blue&amp;recommendations=True&amp;locale=french"" scrolling=""no"" frameborder=""0"" style=""border:none; overflow:hidden; width:300px; height:300px;"" allowTransparency=""true""></iframe>";

            // Act
            var actualText = Facebook.ActivityFeed("http://mysite", 100, 120, false, "gray", "Arial", "blue", true);

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedText, actualText.ToString());
        }

        [Fact]
        public void FbmlNamespacesTest()
        {
            // Arrange
            var expectedText = @"xmlns:fb=""http://www.facebook.com/2008/fbml"" xmlns:og=""http://opengraphprotocol.org/schema/""";

            // Act
            var actualText = Facebook.FbmlNamespaces();

            // Assert
            Assert.Equal(expectedText, actualText.ToString());
        }

        private static HttpContextBase CreateHttpContext(IDictionary<string, string> cookieValues = null)
        {
            var context = new Mock<HttpContextBase>();
            var httpRequest = new Mock<HttpRequestBase>();
            var cookies = new HttpCookieCollection();
            httpRequest.Setup(c => c.Cookies).Returns(cookies);

            context.Setup(c => c.Request).Returns(httpRequest.Object);

            if (cookieValues != null)
            {
                foreach (var item in cookieValues)
                {
                    cookies.Add(new HttpCookie(item.Key, item.Value));
                }
            }

            return context.Object;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. Facebook.AppId) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
