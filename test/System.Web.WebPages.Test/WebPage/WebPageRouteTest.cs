// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class WebPageRouteTest
    {
        private class HashyBuildManager : IVirtualPathFactory
        {
            private readonly HashSet<string> _existingFiles;

            public HashyBuildManager(IEnumerable<string> validFilePaths)
            {
                _existingFiles = new HashSet<string>(validFilePaths, StringComparer.InvariantCultureIgnoreCase);
            }

            public bool Exists(string virtualPath)
            {
                return _existingFiles.Contains(virtualPath);
            }

            public object CreateInstance(string virtualPath)
            {
                throw new NotSupportedException();
            }
        }

        // Helper to test smarty route match, null match string is used for no expected match
        private static void ConstraintTest(IEnumerable<string> validFiles, IEnumerable<string> supportedExt, string url, string match, string pathInfo, bool mobileDevice = false)
        {
            var objectFactory = new HashyBuildManager(validFiles);
            var mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Setup(c => c.Request.Browser.IsMobileDevice).Returns(mobileDevice);
            mockContext.Setup(c => c.Request.Cookies).Returns(new HttpCookieCollection());
            mockContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());
            var displayModeProvider = new DisplayModeProvider();

            WebPageMatch smartyMatch = WebPageRoute.MatchRequest(url, supportedExt.ToArray(), objectFactory.Exists, mockContext.Object, displayModeProvider);
            if (match != null)
            {
                Assert.NotNull(smartyMatch);
                Assert.Equal(match, smartyMatch.MatchedPath);
                Assert.Equal(pathInfo, smartyMatch.PathInfo);
            }
            else
            {
                Assert.Null(smartyMatch);
            }
        }

        [Theory,
         InlineData("1.1/2/3", "1.1/2/3.3", ""),
         InlineData("1/2/3/4", "1.one", "2/3/4"),
         InlineData("2/3/4", "2.two", "3/4"),
         InlineData("one/two/3/4/5/6", "one/two/3/4.4", "5/6"),
         InlineData("one/two/3/4/5/6/foo", "one/two/3/4.4", "5/6/foo"),
         InlineData("one/two/3/4/5/6/foo.htm", null, null)]
        public void MultipleExtensionsTest(string url, string match, string pathInfo)
        {
            string[] files = new[] { "~/1.one", "~/2.two", "~/1.1/2/3.3", "~/one/two/3/4.4", "~/one/two/3/4/5/6/foo.htm" };
            string[] extensions = new[] { "aspx", "hao", "one", "two", "3", "4" };

            ConstraintTest(files, extensions, url, match, pathInfo);
        }

        [Theory,
         InlineData("1.1/2/3", "1.1/2/3.Mobile.3", ""),
         InlineData("1/2/3/4", "1.Mobile.one", "2/3/4"),
         InlineData("2/3/4", "2.Mobile.two", "3/4"),
         InlineData("one/two/3/4/5/6", "one/two/3/4.Mobile.4", "5/6"),
         InlineData("one/two/3/4/5/6/foo", "one/two/3/4.Mobile.4", "5/6/foo"),
         InlineData("one/two/3/4/5/6/foo.Mobile.htm", null, null)]
        public void MultipleExtensionsMobileTest(string url, string match, string pathInfo)
        {
            string[] files = new[]
            {
                "~/1.one", "~/2.two", "~/1.1/2/3.3", "~/one/two/3/4.4", "~/one/two/3/4/5/6/foo.htm",
                "~/1.Mobile.one", "~/2.Mobile.two", "~/1.1/2/3.Mobile.3", "~/one/two/3/4.Mobile.4", "~/one/two/3/4/5/6/foo.Mobile.htm"
            };
            string[] extensions = new[] { "aspx", "hao", "one", "two", "3", "4" };

            ConstraintTest(files, extensions, url, match, pathInfo, mobileDevice: true);
        }

        [Fact]
        public void FilesWithLeadingUnderscoresAreNeverServed()
        {
            string[] files = new[] { "~/hi.evil", "~/_hi.evil", "~/_nest/good.evil", "~/_nest/_hide.evil", "~/_ok.good" };
            string[] extensions = new[] { "evil" };

            ConstraintTest(files, extensions, "hi", "hi.evil", "");
            ConstraintTest(files, extensions, "_nest/good/some/extra/path/info", "_nest/good.evil", "some/extra/path/info");
            Assert.Throws<HttpException>(() => { ConstraintTest(files, extensions, "_hi", null, null); }, "Files with leading underscores (\"_\") cannot be served.");
            Assert.Throws<HttpException>(() => { ConstraintTest(files, extensions, "_nest/_hide", null, null); }, "Files with leading underscores (\"_\") cannot be served.");
        }

        [Theory]
        [InlineData(new object[] { "_foo", "_foo/default.cshtml" })]
        [InlineData(new object[] { "_bar/_baz", "_bar/_baz/index.cshtml" })]
        public void DirectoriesWithLeadingUnderscoresAreServed(string requestPath, string expectedPath)
        {
            // Arramge
            var files = new[] { "~/_foo/default.cshtml", "~/_bar/_baz/index.cshtml" };
            var extensions = new[] { "cshtml" };

            // Act
            ConstraintTest(files, extensions, requestPath, expectedPath, "");
        }

        [Fact]
        public void TransformedUnderscoreAreNotServed()
        {
            string[] files = new[] { "~/_ok.Mobile.ext", "~/ok.ext" };
            string[] extensions = new[] { "ext" };

            ConstraintTest(files, extensions, "ok.ext", "ok.ext", "", mobileDevice: true);
            ConstraintTest(files, extensions, "ok/some/extra/path/info", "ok.ext", "some/extra/path/info", mobileDevice: true);

            Assert.Throws<HttpException>(() => { ConstraintTest(files, extensions, "_ok.Mobile.ext", null, null, mobileDevice: true); }, "Files with leading underscores (\"_\") cannot be served.");
        }

        [Fact]
        public void MobileFilesAreReturnedInthePresenceOfUnderscoreFiles()
        {
            string[] files = new[] { "~/_ok.Mobile.ext", "~/ok.ext", "~/ok.mobile.ext" };
            string[] extensions = new[] { "ext" };

            ConstraintTest(files, extensions, "ok.ext", "ok.Mobile.ext", "", mobileDevice: true);
            ConstraintTest(files, extensions, "ok/some/extra/path/info", "ok.Mobile.ext", "some/extra/path/info", mobileDevice: true);
            ConstraintTest(files, extensions, "ok.mobile", "ok.mobile.ext", "", mobileDevice: false);
        }

        [Fact]
        public void UnsupportedExtensionExistingFileTest()
        {
            ConstraintTest(new[] { "~/hao.aspx", "~/hao/hao.txt" }, new[] { "aspx" }, "hao/hao.txt", null, null);
        }

        [Fact]
        public void NullPathValueDoesNotMatchTest()
        {
            ConstraintTest(new[] { "~/hao.aspx", "~/hao/hao.txt" }, new[] { "aspx" }, null, null, null);
        }

        [Fact]
        public void RightToLeftPrecedenceTest()
        {
            ConstraintTest(new[] { "~/one/two/three.aspx", "~/one/two.aspx", "~/one.aspx" }, new[] { "aspx" }, "one/two/three", "one/two/three.aspx", "");
        }

        [Fact]
        public void DefaultPrecedenceTests()
        {
            string[] files = new[] { "~/one/two/default.aspx", "~/one/default.aspx", "~/default.aspx" };
            string[] extensions = new[] { "aspx" };

            // Default only tries to look at the full path level
            ConstraintTest(files, extensions, "one/two/three", null, null);
            ConstraintTest(files, extensions, "one/two", "one/two/default.aspx", "");
            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
            ConstraintTest(files, extensions, "", "default.aspx", "");
            ConstraintTest(files, extensions, "one/two/three/four/five/six/7/8", null, null);
        }

        [Fact]
        public void IndexTests()
        {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/index.aspq" };
            string[] extensions = new[] { "aspx", "aspq" };

            // index only tries to look at the full path level
            ConstraintTest(files, extensions, "one/two/three", null, null);
            ConstraintTest(files, extensions, "one/two", "one/two/index.aspx", "");
            ConstraintTest(files, extensions, "one", "one/index.aspx", "");
            ConstraintTest(files, extensions, "", "index.aspq", "");
            ConstraintTest(files, extensions, "one/two/three/four/five/six/7/8", null, null);
        }

        [Fact]
        public void DefaultVsIndexNestedTest()
        {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/one/default.aspx", "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx", "aspq" };

            ConstraintTest(files, extensions, "one/two", "one/two/index.aspx", "");
            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
            ConstraintTest(files, extensions, "", "default.aspx", "");
        }

        [Fact]
        public void DefaultVsIndexSameExtensionTest()
        {
            string[] files = new[] { "~/one/two/index.aspx", "~/one/index.aspx", "~/one/default.aspx", "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx" };

            ConstraintTest(files, extensions, "one", "one/default.aspx", "");
        }

        [Fact]
        public void DefaultVsIndexDifferentExtensionTest()
        {
            string[] files = new[] { "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspx", "aspq" };

            ConstraintTest(files, extensions, "", "default.aspx", "");
        }

        [Fact]
        public void DefaultVsIndexOnlyOneExtensionTest()
        {
            string[] files = new[] { "~/index.aspq", "~/default.aspx" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "", "index.aspq", "");
        }

        [Fact]
        public void FullMatchNoPathInfoTest()
        {
            ConstraintTest(new[] { "~/hao.aspx" }, new[] { "aspx" }, "hao", "hao.aspx", "");
        }

        [Fact]
        public void MatchFileWithExtensionTest()
        {
            string[] files = new[] { "~/page.aspq" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "page.aspq", "page.aspq", "");
        }

        [Fact]
        public void NoMatchFileWithWrongExtensionTest()
        {
            string[] files = new[] { "~/page.aspx" };
            string[] extensions = new[] { "aspq" };

            ConstraintTest(files, extensions, "page.aspx", null, null);
        }

        [Fact]
        public void WebPageRouteDoesNotPerformMappingIfRootLevelIsExplicitlyDisabled()
        {
            // Arrange
            var webPageRoute = new WebPageRoute { IsExplicitlyDisabled = true };
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.RemapHandler(It.IsAny<IHttpHandler>())).Throws(new Exception("Smarty route should be disabled."));
            context.SetupGet(c => c.Request).Throws(new Exception("We do not need to use the request to identify if the app is disabled."));

            // Act 
            webPageRoute.DoPostResolveRequestCache(context.Object);

            // Assert. 
            // If we've come this far, neither of the setups threw.
            Assert.True(true);
        }

        [Fact]
        public void MatchRequestSetsDisplayModeOfFirstMatchPerContext()
        {
            // Arrange
            var objectFactory = new HashyBuildManager(new string[] { "~/page.Mobile.aspx", "~/nonMobile.aspx" });
            var mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Setup(c => c.Request.Browser.IsMobileDevice).Returns(true);
            mockContext.Setup(c => c.Request.Cookies).Returns(new HttpCookieCollection());
            mockContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());

            var displayModeProvider = new DisplayModeProvider();

            // Act
            WebPageMatch mobileMatch = WebPageRoute.MatchRequest("page.aspx", new string[] { "aspx" }, objectFactory.Exists, mockContext.Object, displayModeProvider);

            // Assert
            Assert.NotNull(mobileMatch.MatchedPath);
            Assert.Equal(DisplayModeProvider.MobileDisplayModeId, DisplayModeProvider.GetDisplayMode(mockContext.Object).DisplayModeId);
        }

        [Fact]
        public void MatchRequestDoesNotSetDisplayModeIfNoMatch()
        {
            // Arrange
            var objectFactory = new HashyBuildManager(new string[] { "~/page.Mobile.aspx" });
            var mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Setup(c => c.Request.Browser.IsMobileDevice).Returns(true);
            mockContext.Setup(c => c.Request.Cookies).Returns(new HttpCookieCollection());
            mockContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());

            var displayModeProvider = new DisplayModeProvider();
            var displayMode = new Mock<IDisplayMode>(MockBehavior.Strict);
            displayMode.Setup(d => d.CanHandleContext(mockContext.Object)).Returns(false);
            displayModeProvider.Modes.Add(displayMode.Object);

            // Act
            WebPageMatch smartyMatch = WebPageRoute.MatchRequest("notThere.aspx", new string[] { "aspx" }, objectFactory.Exists, mockContext.Object, displayModeProvider);

            // Assert
            Assert.Null(DisplayModeProvider.GetDisplayMode(mockContext.Object));
        }
    }
}
