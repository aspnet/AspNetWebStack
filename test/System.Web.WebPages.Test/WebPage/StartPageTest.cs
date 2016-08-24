// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Profile;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class StartPageTest
    {
        // The page ~/_pagestart.cshtml does the following:
        // this is the init page
        //
        // The page ~/index.cshtml does the following:
        // hello world
        // Expected result:
        // this is the init page hello world
        [Fact]
        public void InitPageBasicTest()
        {
            var init = Utils.CreateStartPage(p =>
                                             p.Write("this is the init page "));
            var page = Utils.CreatePage(p =>
                                        p.Write("hello world"));

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("this is the init page hello world", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // this is the init page
        //
        // The page ~/folder1/index.cshtml does the following:
        // hello world
        // Expected result:
        // this is the init page hello world
        [Fact]
        public void InitSubfolderTest()
        {
            var init = Utils.CreateStartPage(p =>
                                             p.Write("this is the init page "));
            var page = Utils.CreatePage(p =>
                                        p.Write("hello world"), "~/folder1/index.cshtml");

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("this is the init page hello world", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // PageData["Title"] = "InitPage";
        // Layout = "Layout.cshtml";
        // this is the init page
        //
        // The page ~/index.cshtml does the following:
        // PageData["Title"] = "IndexCshtmlPage"
        // hello world
        //
        // The layout page ~/Layout.cshtml does the following:
        // layout start
        // @PageData["Title"]
        // @RenderBody()
        // layout end
        //
        // Expected result:
        // layout start IndexCshtmlPage this is the init page hello world layout end
        [Fact]
        public void InitPageLayoutTest()
        {
            var init = Utils.CreateStartPage(p =>
            {
                p.Layout = "Layout.cshtml";
                p.Write(" this is the init page ");
                Assert.Equal("~/Layout.cshtml", p.Layout);
            });
            var page = Utils.CreatePage(p =>
            {
                p.PageData["Title"] = "IndexCshtmlPage";
                p.Write("hello world");
            });
            var layoutPage = Utils.CreatePage(p =>
            {
                p.Write("layout start ");
                p.Write(p.PageData["Title"]);
                p.WriteLiteral(p.RenderBody());
                p.Write(" layout end");
            }, "~/Layout.cshtml");

            init.ChildPage = page;
            Utils.AssignObjectFactoriesAndDisplayModeProvider(init, page, layoutPage);

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("layout start IndexCshtmlPage this is the init page hello world layout end", result);
        }

        // _pagestart.cshtml sets the LayoutPage to be null
        [Fact]
        public void InitPageNullLayoutPageTest()
        {
            var init1 = Utils.CreateStartPage(
                p =>
                {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(
                p =>
                {
                    p.Layout = null;
                    p.WriteLiteral("<init2>");
                    p.RunPage();
                    p.WriteLiteral("</init2>");
                }, init2path);
            var page = Utils.CreatePage(p =>
                                        p.Write("hello world"), "~/folder1/index.cshtml");
            var layoutPage = Utils.CreatePage(p =>
                                              p.Write("layout page"), "~/Layout.cshtml");

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layoutPage, init1, init2);

            init1.ChildPage = init2;
            init2.ChildPage = page;

            var result = Utils.RenderWebPage(page, init1);
            Assert.Equal("<init1><init2>hello world</init2></init1>", result);
        }

        // _pagestart.cshtml sets the LayoutPage, but page sets it to null
        [Fact]
        public void PageSetsNullLayoutPageTest()
        {
            var init1 = Utils.CreateStartPage(
                p =>
                {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var layoutPage = Utils.CreatePage(p =>
                                              p.Write("layout page"), "~/Layout.cshtml");
            var page = Utils.CreatePage(p =>
            {
                p.Layout = null;
                p.Write("hello world");
            });
            Utils.AssignObjectFactoriesAndDisplayModeProvider(init1, layoutPage, page);
            init1.ChildPage = page;
            var result = Utils.RenderWebPage(page, init1);
            Assert.Equal("<init1>hello world</init1>", result);
        }

        [Fact]
        public void PageSetsEmptyLayoutPageTest()
        {
            var init1 = Utils.CreateStartPage(
                p =>
                {
                    p.Layout = "~/Layout.cshtml";
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var layoutPage = Utils.CreatePage(p =>
                                              p.Write("layout page"), "~/Layout.cshtml");
            var page = Utils.CreatePage(p =>
            {
                p.Layout = "";
                p.Write("hello world");
            });
            Utils.AssignObjectFactoriesAndDisplayModeProvider(init1, layoutPage, page);
            init1.ChildPage = page;
            var result = Utils.RenderWebPage(page, init1);
            Assert.Equal("<init1>hello world</init1>", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // init page start
        // @RunPage()
        // init page end
        //
        // The page ~/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // init page start hello world init page end
        [Fact]
        public void RunPageTest()
        {
            var init = Utils.CreateStartPage(
                p =>
                {
                    p.Write("init page start ");
                    p.RunPage();
                    p.Write(" init page end");
                });
            var page = Utils.CreatePage(p =>
                                        p.Write("hello world"));

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("init page start hello world init page end", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // <init1>
        // @RunPage()
        // </init1>
        //
        // The page ~/folder1/_pagestart.cshtml does the following:
        // <init2>
        // @RunPage()
        // </init2>
        //
        // The page ~/folder1/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // <init1><init2>hello world</init2></init1>
        [Fact]
        public void NestedRunPageTest()
        {
            var init1 = Utils.CreateStartPage(
                p =>
                {
                    p.WriteLiteral("<init1>");
                    p.RunPage();
                    p.WriteLiteral("</init1>");
                });
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(
                p =>
                {
                    p.WriteLiteral("<init2>");
                    p.RunPage();
                    p.WriteLiteral("</init2>");
                }, init2path);
            var page = Utils.CreatePage(p =>
                                        p.Write("hello world"), "~/folder1/index.cshtml");

            init1.ChildPage = init2;
            init2.ChildPage = page;

            var result = Utils.RenderWebPage(page, init1);
            Assert.Equal("<init1><init2>hello world</init2></init1>", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // PageData["key1"] = "value1";
        //
        // The page ~/folder1/_pagestart.cshtml does the following:
        // PageData["key2"] = "value2";
        //
        // The page ~/folder1/index.cshtml does the following:
        // @PageData["key1"] @PageData["key2"] @PageData["key3"]
        //
        // Expected result:
        // value1 value2
        [Fact]
        public void PageDataTest()
        {
            var init1 = Utils.CreateStartPage(p => p.PageData["key1"] = "value1");
            var init2path = "~/folder1/_pagestart.cshtml";
            var init2 = Utils.CreateStartPage(p => p.PageData["key2"] = "value2", init2path);
            var page = Utils.CreatePage(
                p =>
                {
                    p.Write(p.PageData["key1"]);
                    p.Write(" ");
                    p.Write(p.PageData["key2"]);
                },
                "~/folder1/index.cshtml");

            init1.ChildPage = init2;
            init2.ChildPage = page;

            var result = Utils.RenderWebPage(page, init1);
            Assert.Equal("value1 value2", result);
        }

        // The page ~/_pagestart.cshtml does the following:
        // init page
        // @RenderPage("subpage.cshtml", "init_data");
        //
        // The page ~/subpage.cshtml does the following:
        // subpage
        // @PageData[0]
        //
        // The page ~/index.cshtml does the following:
        // hello world
        //
        // Expected result:
        // init page subpage init_data hello world
        [Fact]
        public void RenderPageTest()
        {
            var init = Utils.CreateStartPage(
                p =>
                {
                    p.Write("init page ");
                    p.Write(p.RenderPage("subpage.cshtml", "init_data"));
                });
            var subpagePath = "~/subpage.cshtml";
            var subpage = Utils.CreatePage(
                p =>
                {
                    p.Write("subpage ");
                    p.Write(p.PageData[0]);
                }, subpagePath);
            var page = Utils.CreatePage(p =>
                                        p.Write(" hello world"));

            init.ChildPage = page;
            Utils.AssignObjectFactoriesAndDisplayModeProvider(init, page, subpage);

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("init page subpage init_data hello world", result);
        }

        [Fact]
        // The page ~/_pagestart.cshtml does the following:
        // <init>
        // @{
        //     try {
        //         RunPage();
        //     } catch (Exception e) {
        //         Write("Exception: " + e.Message);
        //     }
        // }
        // </init>
        //
        // The page ~/index.cshtml does the following:
        // hello world
        // @{throw new InvalidOperation("exception from index.cshtml");}
        //
        // Expected result:
        // <init>hello world Exception: exception from index.cshtml</init>
        public void InitCatchExceptionTest()
        {
            var init = Utils.CreateStartPage(
                p =>
                {
                    p.WriteLiteral("<init>");
                    try
                    {
                        p.RunPage();
                    }
                    catch (Exception e)
                    {
                        p.Write("Exception: " + e.Message);
                    }
                    p.WriteLiteral("</init>");
                });
            var page = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral("hello world ");
                    throw new InvalidOperationException("exception from index.cshtml");
                });

            init.ChildPage = page;

            var result = Utils.RenderWebPage(page, init);
            Assert.Equal("<init>hello world Exception: exception from index.cshtml</init>", result);
        }

        public class MockInitPage : MockStartPage
        {
            internal object GetBuildManager()
            {
                return typeof(BuildManager).GetField("_theBuildManager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            }
        }

        // Simulate a site that is nested, eg /subfolder1/website1
        [Fact]
        public void ExecuteWithinInitTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                Utils.CreateHttpRuntime("/subfolder1/website1");
                new HostingEnvironment();
                var stringSet = Activator.CreateInstance(typeof(BuildManager).Assembly.GetType("System.Web.Util.StringSet"), true);
                typeof(BuildManager).GetField("_forbiddenTopLevelDirectories", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(new MockInitPage().GetBuildManager(), stringSet);
                ;

                var init = new MockInitPage()
                {
                    VirtualPath = "~/_pagestart.cshtml",
                    ExecuteAction = p => { },
                };
                var page = Utils.CreatePage(p => { });

                Utils.AssignObjectFactoriesAndDisplayModeProvider(page, init);

                var result = Utils.RenderWebPage(page);
            });
        }

        [Fact]
        public void SetGetPropertiesTest()
        {
            var init = new MockInitPage();
            var page = new MockPage();
            init.ChildPage = page;

            // Context
            var context = new Mock<HttpContextBase>().Object;
            init.Context = context;
            Assert.Equal(context, init.Context);
            Assert.Equal(context, page.Context);

            // Profile/Request/Response/Server/Cache/Session/Application
            var profile = new Mock<ProfileBase>().Object;
            var request = new Mock<HttpRequestBase>().Object;
            var response = new Mock<HttpResponseBase>().Object;
            var server = new Mock<HttpServerUtilityBase>().Object;
            var cache = new Cache();
            var app = new Mock<HttpApplicationStateBase>().Object;
            var session = new Mock<HttpSessionStateBase>().Object;

            var contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(c => c.Profile).Returns(profile);
            contextMock.Setup(c => c.Request).Returns(request);
            contextMock.Setup(c => c.Response).Returns(response);
            contextMock.Setup(c => c.Cache).Returns(cache);
            contextMock.Setup(c => c.Server).Returns(server);
            contextMock.Setup(c => c.Application).Returns(app);
            contextMock.Setup(c => c.Session).Returns(session);

            context = contextMock.Object;
            page.Context = context;
            Assert.Same(profile, init.Profile);
            Assert.Same(request, init.Request);
            Assert.Same(response, init.Response);
            Assert.Same(cache, init.Cache);
            Assert.Same(server, init.Server);
            Assert.Same(session, init.Session);
            Assert.Same(app, init.AppState);
        }

        [Fact]
        public void GetDirectoryTest()
        {
            var initPage = new Mock<StartPage>().Object;
            Assert.Equal("/website1/", initPage.GetDirectory("/website1/default.cshtml"));
            Assert.Equal("~/", initPage.GetDirectory("~/default.cshtml"));
            Assert.Equal("/", initPage.GetDirectory("/website1/"));
            Assert.Null(initPage.GetDirectory("/"));
        }

        [Fact]
        public void GetStartPageReturnsStartPageFromCurrentDirectoryIfExists()
        {
            // Arrange
            var initPage = Utils.CreateStartPage(p => p.Write("<init>"), "~/subdir/_pagestart.vbhtml");
            var page = Utils.CreatePage(p => p.Write("test"), "~/subdir/_index.cshtml");
            var objectFactory = Utils.AssignObjectFactoriesAndDisplayModeProvider(page, initPage);

            // Act
            var result = StartPage.GetStartPage(page, objectFactory, null, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });

            // Assert
            Assert.Equal(initPage, result);
        }

        [Fact]
        public void GetStartPageReturnsStartPageFromParentDirectoryIfStartPageDoesNotExistInCurrentDirectory()
        {
            // Arrange
            var initPage = Utils.CreateStartPage(null, "~/subdir/_pagestart.vbhtml");
            var page = Utils.CreatePage(null, "~/subdir/subsubdir/test.cshtml");
            var objectFactory = Utils.AssignObjectFactoriesAndDisplayModeProvider(page, initPage);

            // Act
            var result = StartPage.GetStartPage(page, objectFactory, null, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });

            // Assert
            Assert.Equal(initPage, result);
        }

        [Fact]
        public void GetStartPageCreatesChainOfStartPages()
        {
            // Arrange
            var subInitPage = Utils.CreateStartPage(null, "~/subdir/_pagestart.vbhtml");
            var initPage = Utils.CreateStartPage(null, "~/_pagestart.vbhtml");
            var page = Utils.CreatePage(null, "~/subdir/subsubdir/subsubsubdir/test.cshtml");
            var objectFactory = Utils.AssignObjectFactoriesAndDisplayModeProvider(page, initPage, subInitPage);

            // Act
            var result = StartPage.GetStartPage(page, objectFactory, null, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });

            // Assert
            Assert.Equal(initPage, result);
            Assert.Equal(subInitPage, (result as StartPage).ChildPage);
        }

        [Fact]
        public void GetStartPageReturnsStartPageFromRoot()
        {
            // Arrange
            var initPage = Utils.CreateStartPage(null, "~/_pagestart.vbhtml");
            var page = Utils.CreatePage(null, "~/subdir/subsubdir/subsubsubdir/subsubsubsubdir/why-does-this-remind-me-of-a-movie-title.cshtml");
            var objectFactory = Utils.AssignObjectFactoriesAndDisplayModeProvider(page, initPage);

            // Act
            var result = StartPage.GetStartPage(page, objectFactory, null, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });

            // Assert
            Assert.Equal(initPage, result);
        }

        [Fact]
        public void GetStartPageUsesBothFileNamesAndExtensionsWhenDeterminingStartPage()
        {
            // Arrange
            var subInitPage = Utils.CreateStartPage(null, "~/subdir/_pagestart.jshtml");
            var initPage = Utils.CreateStartPage(null, "~/_pagestart.vbhtml");
            var page = Utils.CreatePage(null, "~/subdir/test.cshtml");
            var objectFactory = Utils.AssignObjectFactoriesAndDisplayModeProvider(page, initPage, subInitPage);

            // Act
            var result = StartPage.GetStartPage(page, objectFactory, null, WebPageHttpHandler.StartPageFileName, new string[] { "cshtml", "vbhtml" });

            // Assert
            Assert.Equal(initPage, result);
        }

        [Fact]
        public void GetStartPage_ThrowsOnNullPage()
        {
            Assert.ThrowsArgumentNull(() => StartPage.GetStartPage(null, "name", new[] { "cshtml" }), "page");
        }

        [Fact]
        public void GetStartPage_ThrowsOnNullFileName()
        {
            var page = Utils.CreatePage(p => p.Write("test"));
            Assert.ThrowsArgumentNullOrEmptyString(() => StartPage.GetStartPage(page, null, new[] { "cshtml" }), "fileName");
        }

        [Fact]
        public void GetStartPage_ThrowsOnEmptyFileName()
        {
            var page = Utils.CreatePage(p => p.Write("test"));
            Assert.ThrowsArgumentNullOrEmptyString(() => StartPage.GetStartPage(page, String.Empty, new[] { "cshtml" }), "fileName");
        }

        [Fact]
        public void GetStartPage_ThrowsOnNullSupportedExtensions()
        {
            var page = Utils.CreatePage(p => p.Write("test"));
            Assert.ThrowsArgumentNull(() => StartPage.GetStartPage(page, "name", null), "supportedExtensions");
        }
    }
}
