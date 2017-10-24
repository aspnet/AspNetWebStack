// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.WebPages.Resources;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class LayoutTest
    {
        [Fact]
        public void LayoutBasicTest()
        {
            var layoutPath = "~/Layout.cshtml";
            LayoutBasicTestInternal(layoutPath);
        }

        [Fact]
        public void RelativeLayoutPageTest()
        {
            var pagePath = "~/MyApp/index.cshtml";
            var layoutPath = "~/MyFiles/Layout.cshtml";
            var layoutPage = "../MyFiles/Layout.cshtml";
            LayoutBasicTestInternal(layoutPath, pagePath, layoutPage);
        }

        [Fact]
        public void AppRelativeLayoutPageTest()
        {
            var pagePath = "~/MyApp/index.cshtml";
            var layoutPath = "~/MyFiles/Layout.cshtml";
            var layoutPage = "~/MyFiles/Layout.cshtml";
            LayoutBasicTestInternal(layoutPath, pagePath, layoutPage);
        }

        [Fact]
        public void SourceFileWithLayoutPageTest()
        {
            // Arrange
            var pagePath = "~/MyApp/index.cshtml";
            var layoutPath = "~/MyFiles/Layout.cshtml";
            var layoutPage = "~/MyFiles/Layout.cshtml";
            var content = "hello world";
            var title = "MyPage";
            var page = CreatePageWithLayout(
                p =>
                {
                    p.PageData["Title"] = title;
                    p.WriteLiteral(content);
                },
                p =>
                {
                    p.WriteLiteral(p.PageData["Title"]);
                    p.Write(p.RenderBody());
                }, pagePath, layoutPath, layoutPage);
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(c => c.Path).Returns("/myapp/index.cshtml");
            request.SetupGet(c => c.RawUrl).Returns("http://localhost:8080/index.cshtml");
            request.SetupGet(c => c.IsLocal).Returns(true);
            request.Setup(c => c.MapPath(It.IsAny<string>())).Returns<string>(c => c);
            request.Setup(c => c.Browser.IsMobileDevice).Returns(false);
            request.Setup(c => c.Cookies).Returns(new HttpCookieCollection());

            var result = Utils.RenderWebPage(page, request: request.Object);
            Assert.Equal(2, page.PageContext.SourceFiles.Count);
            Assert.Contains("~/MyApp/index.cshtml", page.PageContext.SourceFiles);
            Assert.Contains("~/MyFiles/Layout.cshtml", page.PageContext.SourceFiles);
        }

        private static void LayoutBasicTestInternal(string layoutPath, string pagePath = "~/index.cshtml", string layoutPage = "Layout.cshtml")
        {
            // The page ~/index.cshtml does the following:
            // PageData["Title"] = "MyPage";
            // Layout = "Layout.cshtml";
            // WriteLiteral("hello world");
            //
            // The layout page ~/Layout.cshtml does the following:
            // WriteLiteral(Title);
            // RenderBody();
            //
            // Expected rendered result is "MyPagehello world"

            var content = "hello world";
            var title = "MyPage";
            var result = RenderPageWithLayout(
                p =>
                {
                    p.PageData["Title"] = title;
                    p.WriteLiteral(content);
                },
                p =>
                {
                    p.WriteLiteral(p.PageData["Title"]);
                    p.Write(p.RenderBody());
                },
                pagePath, layoutPath, layoutPage);

            Assert.Equal(title + content, result);
        }

        [Fact]
        public void LayoutNestedTest()
        {
            // Testing nested layout pages
            //
            // The page ~/index.cshtml does the following:
            // PageData["Title"] = "MyPage";
            // Layout = "Layout1.cshtml";
            // WriteLiteral("hello world");
            //
            // The first layout page ~/Layout1.cshtml does the following:
            // Layout = "Layout2.cshtml";
            // WriteLiteral("<layout1>");
            // RenderBody();
            // WriteLiteral("</layout1>");
            //
            // The second layout page ~/Layout2.cshtml does the following:
            // WriteLiteral(Title);
            // WriteLiteral("<layout2>");
            // RenderBody();
            // WriteLiteral("</layout2>");
            //
            // Expected rendered result is "MyPage<layout2><layout1>hello world</layout1></layout2>"

            var layout2Path = "~/Layout2.cshtml";
            var layout2 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral(p.PageData["Title"]);
                    p.WriteLiteral("<layout2>");
                    p.Write(p.RenderBody());
                    p.WriteLiteral("</layout2>");
                },
                layout2Path);

            var layout1Path = "~/Layout1.cshtml";
            var layout1 = Utils.CreatePage(
                p =>
                {
                    p.Layout = "Layout2.cshtml";
                    p.WriteLiteral("<layout1>");
                    p.Write(p.RenderBody());
                    p.WriteLiteral("</layout1>");
                },
                layout1Path);

            var page = Utils.CreatePage(
                p =>
                {
                    p.PageData["Title"] = "MyPage";
                    p.Layout = "Layout1.cshtml";
                    p.WriteLiteral("hello world");
                });

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layout1, layout2);

            var result = Utils.RenderWebPage(page);
            Assert.Equal("MyPage<layout2><layout1>hello world</layout1></layout2>", result);
        }

        [Fact]
        public void LayoutSectionsTest()
        {
            // Testing nested layout pages with sections
            //
            // The page ~/index.cshtml does the following:
            // PageData["Title"] = "MyPage";
            // Layout = "Layout1.cshtml";
            // DefineSection("header1", () => {
            //     WriteLiteral("index header");
            // });
            // WriteLiteral("hello world");
            // DefineSection("footer1", () => {
            //     WriteLiteral("index footer");
            // });
            //
            // The first layout page ~/Layout1.cshtml does the following:
            // Layout = "Layout2.cshtml";
            // DefineSection("header2", () => {
            //     WriteLiteral("<layout1 header>");
            //     RenderSection("header1");
            //     WriteLiteral("</layout1 header>");
            // });
            // WriteLiteral("<layout1>");
            // RenderBody();
            // WriteLiteral("</layout1>");
            // DefineSection("footer2", () => {
            //     WriteLiteral("<layout1 footer>");
            //     RenderSection("header2");
            //     WriteLiteral("</layout1 footer>");
            // });
            //
            // The second layout page ~/Layout2.cshtml does the following:
            // WriteLiteral(Title);
            // WriteLiteral("\n<layout2 header>");
            // RenderSection("header2");
            // WriteLiteral("</layout2 header>\n");
            // WriteLiteral("<layout2>");
            // RenderBody();
            // WriteLiteral("</layout2>\n");
            // WriteLiteral("<layout2 footer>");
            // RenderSection("footer");
            // WriteLiteral("</layout2 footer>");
            //
            // Expected rendered result is:
            // MyPage
            // <layout2 header><layout1 header>index header</layout1 header></layout2 header>
            // <layout2><layout1>hello world</layout1></layout2>"
            // <layout2 footer><layout1 footer>index footer</layout1 footer></layout2 footer>

            var layout2Path = "~/Layout2.cshtml";
            var layout2 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral(p.PageData["Title"]);
                    p.WriteLiteral("\r\n");
                    p.WriteLiteral("<layout2 header>");
                    p.Write(p.RenderSection("header2"));
                    p.WriteLiteral("</layout2 header>");
                    p.WriteLiteral("\r\n");

                    p.WriteLiteral("<layout2>");
                    p.Write(p.RenderBody());
                    p.WriteLiteral("</layout2>");
                    p.WriteLiteral("\r\n");

                    p.WriteLiteral("<layout2 footer>");
                    p.Write(p.RenderSection("footer2"));
                    p.WriteLiteral("</layout2 footer>");
                },
                layout2Path);

            var layout1Path = "~/Layout1.cshtml";
            var layout1 = Utils.CreatePage(
                p =>
                {
                    p.Layout = "Layout2.cshtml";
                    p.DefineSection("header2", () =>
                    {
                        p.WriteLiteral("<layout1 header>");
                        p.Write(p.RenderSection("header1"));
                        p.WriteLiteral("</layout1 header>");
                    });

                    p.WriteLiteral("<layout1>");
                    p.Write(p.RenderBody());
                    p.WriteLiteral("</layout1>");

                    p.DefineSection("footer2", () =>
                    {
                        p.WriteLiteral("<layout1 footer>");
                        p.Write(p.RenderSection("footer1"));
                        p.WriteLiteral("</layout1 footer>");
                    });
                },
                layout1Path);

            var page = Utils.CreatePage(
                p =>
                {
                    p.PageData["Title"] = "MyPage";
                    p.Layout = "Layout1.cshtml";
                    p.DefineSection("header1", () => { p.WriteLiteral("index header"); });
                    p.WriteLiteral("hello world");
                    p.DefineSection("footer1", () => { p.WriteLiteral("index footer"); });
                });

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layout1, layout2);

            var result = Utils.RenderWebPage(page);
            var expected = "MyPage" + Environment.NewLine
                         + "<layout2 header><layout1 header>index header</layout1 header></layout2 header>" + Environment.NewLine
                         + "<layout2><layout1>hello world</layout1></layout2>" + Environment.NewLine
                         + "<layout2 footer><layout1 footer>index footer</layout1 footer></layout2 footer>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LayoutSectionsNestedNamesTest()
        {
            // Tests nested layout using the same section names at different levels.
            //
            // The page ~/index.cshtml does the following:
            // Layout = "Layout1.cshtml";
            // @section body {
            //     body in index
            // }
            //
            // The page ~/layout1.cshtml does the following:
            // Layout = "Layout2.cshtml";
            // @section body {
            //     body in layout1
            //     @RenderSection("body")
            // }
            //
            // The page ~/layout2.cshtml does the following:
            // body in layout2
            // @RenderSection("body")
            //
            // Expected rendered result is:
            // body in layout2 body in layout1 body in index
            var layout2Path = "~/Layout2.cshtml";
            var layout2 = Utils.CreatePage(
                p =>
                {
                    p.WriteLiteral("body in layout2 ");
                    p.Write(p.RenderSection("body"));
                },
                layout2Path);
            var layout1Path = "~/Layout1.cshtml";
            var layout1 = Utils.CreatePage(
                p =>
                {
                    p.Layout = "Layout2.cshtml";
                    p.DefineSection("body", () =>
                    {
                        p.WriteLiteral("body in layout1 ");
                        p.Write(p.RenderSection("body"));
                    });
                },
                layout1Path);

            var page = Utils.CreatePage(
                p =>
                {
                    p.Layout = "Layout1.cshtml";
                    p.DefineSection("body", () => { p.WriteLiteral("body in index"); });
                });

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layout1, layout2);

            var result = Utils.RenderWebPage(page);
            var expected = "body in layout2 body in layout1 body in index";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CaseInsensitiveSectionNamesTest()
        {
            var page = CreatePageWithLayout(
                p =>
                {
                    p.Write("123");
                    p.DefineSection("abc", () => { p.Write("abc"); });
                    p.DefineSection("XYZ", () => { p.Write("xyz"); });
                    p.Write("456");
                },
                p =>
                {
                    p.Write(p.RenderSection("AbC"));
                    p.Write(p.RenderSection("xyZ"));
                    p.Write(p.RenderBody());
                });
            var result = Utils.RenderWebPage(page);
            var expected = "abcxyz123456";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MissingLayoutPageTest()
        {
            var layoutPage = "Layout.cshtml";
            var page = Utils.CreatePage(
                p =>
                {
                    p.PageData["Title"] = "MyPage";
                    p.Layout = layoutPage;
                });
            var layoutPath1 = "~/Layout.cshtml";

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_LayoutPageNotFound, layoutPage, layoutPath1));
        }

        [Fact]
        public void RenderBodyAlreadyCalledTest()
        {
            // Layout page calls RenderBody more than once.
            var page = CreatePageWithLayout(
                p => { },
                p =>
                {
                    p.Write(p.RenderBody());
                    p.Write(p.RenderBody());
                });

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page), WebPageResources.WebPage_RenderBodyAlreadyCalled);
        }

        [Fact]
        public void RenderBodyNotCalledTest()
        {
            // Page does not define any sections, but layout page does not call RenderBody
            var layoutPath = "~/Layout.cshtml";
            var page = CreatePageWithLayout(
                p => { },
                p => { },
                layoutPath: layoutPath);

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_RenderBodyNotCalled, layoutPath));
        }

        [Fact]
        public void RenderBodyCalledDirectlyTest()
        {
            // A Page that is not a layout page calls the RenderBody method
            var page = Utils.CreatePage(p => { p.RenderBody(); });
            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_CannotRequestDirectly, "~/index.cshtml", "RenderBody"));
        }

        [Fact]
        public void RenderSectionCalledDirectlyTest()
        {
            // A Page that is not a layout page calls the RenderBody method
            var page = Utils.CreatePage(p => { p.RenderSection(""); });
            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_CannotRequestDirectly, "~/index.cshtml", "RenderSection"));
        }

        [Fact]
        public void SectionAlreadyDefinedTest()
        {
            // The page calls DefineSection more than once on the same name
            var sectionName = "header";
            var page = Utils.CreatePage(p =>
            {
                p.Layout = "Layout.cshtml";
                p.DefineSection(sectionName, () => { });
                p.DefineSection(sectionName, () => { });
            });

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.InvariantCulture, WebPageResources.WebPage_SectionAleadyDefined, sectionName));
        }

        [Fact]
        public void SectionAlreadyDefinedCaseInsensitiveTest()
        {
            // The page calls DefineSection more than once on the same name but with different casing
            var name1 = "section1";
            var name2 = "SecTion1";

            var page = Utils.CreatePage(p =>
            {
                p.Layout = "Layout.cshtml";
                p.DefineSection(name1, () => { });
                p.DefineSection(name2, () => { });
            });

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.InvariantCulture, WebPageResources.WebPage_SectionAleadyDefined, name2));
        }

        [Fact]
        public void SectionNotDefinedTest()
        {
            // Layout page calls RenderSection on a name that has not been defined.
            var sectionName = "NoSuchSection";
            var page = CreatePageWithLayout(
                p => { },
                p => { p.Write(p.RenderSection(sectionName)); });

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.InvariantCulture, WebPageResources.WebPage_SectionNotDefined, sectionName));
        }

        [Fact]
        public void SectionAlreadyRenderedTest()
        {
            // Layout page calls RenderSection on the same name more than once.
            var sectionName = "header";
            var page = CreatePageWithLayout(
                p =>
                {
                    p.Layout = "Layout.cshtml";
                    p.DefineSection(sectionName, () => { });
                },
                p =>
                {
                    p.Write(p.RenderSection(sectionName));
                    p.Write(p.RenderSection(sectionName));
                });

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.InvariantCulture, WebPageResources.WebPage_SectionAleadyRendered, sectionName));
        }

        [Fact]
        public void SectionsNotRenderedTest()
        {
            // Layout page does not render all the defined sections.

            var layoutPath = "~/Layout.cshtml";
            var sectionName1 = "section1";
            var sectionName2 = "section2";
            var sectionName3 = "section3";
            var sectionName4 = "section4";
            var sectionName5 = "section5";
            // A dummy section action that does nothing
            SectionWriter sectionAction = () => { };

            // The page defines 5 sections.
            var page = CreatePageWithLayout(
                p =>
                {
                    p.DefineSection(sectionName1, sectionAction);
                    p.DefineSection(sectionName2, sectionAction);
                    p.DefineSection(sectionName3, sectionAction);
                    p.DefineSection(sectionName4, sectionAction);
                    p.DefineSection(sectionName5, sectionAction);
                },
                // The layout page renders only two of the sections
                p =>
                {
                    p.Write(p.RenderSection(sectionName2));
                    p.Write(p.RenderSection(sectionName4));
                },
                layoutPath: layoutPath);

            var sectionsNotRendered = "section1; section3; section5";
            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_SectionsNotRendered, layoutPath, sectionsNotRendered));
        }

        [Fact]
        public void SectionsNotRenderedRenderBodyTest()
        {
            // Layout page does not render all the defined sections, but it calls RenderBody.
            var layoutPath = "~/Layout.cshtml";
            var sectionName1 = "section1";
            var sectionName2 = "section2";
            // A dummy section action that does nothing
            SectionWriter sectionAction = () => { };

            var page = CreatePageWithLayout(
                p =>
                {
                    p.DefineSection(sectionName1, sectionAction);
                    p.DefineSection(sectionName2, sectionAction);
                },
                // The layout page only calls RenderBody
                p => { p.Write(p.RenderBody()); },
                layoutPath: layoutPath);

            var sectionsNotRendered = "section1; section2";
            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_SectionsNotRendered, layoutPath, sectionsNotRendered));
        }

        [Fact]
        public void InvalidPageTypeTest()
        {
            var layoutPath = "~/Layout.js";
            var contents = "hello world";
            var page = Utils.CreatePage(p =>
            {
                p.Layout = layoutPath;
                p.Write(contents);
            });
            var layoutPage = new object();

            var objectFactory = new Mock<IVirtualPathFactory>();
            objectFactory.Setup(c => c.Exists(It.IsAny<string>())).Returns<string>(p => layoutPath.Equals(p, StringComparison.OrdinalIgnoreCase));
            objectFactory.Setup(c => c.CreateInstance(It.IsAny<string>())).Returns<string>(_ => layoutPage as WebPageBase);
            page.VirtualPathFactory = objectFactory.Object;

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_InvalidPageType, layoutPath));

            Assert.Throws<HttpException>(() => Utils.RenderWebPage(page),
                                                  String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_InvalidPageType, layoutPath));
        }

        [Fact]
        public void ValidPageTypeTest()
        {
            var layoutPath = "~/Layout.js";
            var contents = "hello world";
            var page = Utils.CreatePage(p =>
            {
                p.Layout = layoutPath;
                p.Write(contents);
            });
            var layoutPage = Utils.CreatePage(p => p.WriteLiteral(p.RenderBody()), layoutPath);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layoutPage);

            Assert.Equal(contents, Utils.RenderWebPage(page));
        }

        [Fact]
        public void IsSectionDefinedTest()
        {
            // Tests for the IsSectionDefined method

            // Only sections named section1 and section3 are defined.
            var page = CreatePageWithLayout(
                p =>
                {
                    p.DefineSection("section1", () => { });
                    p.DefineSection("section3", () => { });
                },
                p =>
                {
                    p.Write(p.RenderSection("section1"));
                    p.Write(p.RenderSection("section3"));
                    p.Write("section1: " + p.IsSectionDefined("section1") + "; ");
                    p.Write("section2: " + p.IsSectionDefined("section2") + "; ");
                    p.Write("section3: " + p.IsSectionDefined("section3") + "; ");
                    p.Write("section4: " + p.IsSectionDefined("section4") + "; ");
                });
            var result = Utils.RenderWebPage(page);
            var expected = "section1: True; section2: False; section3: True; section4: False; ";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void OptionalSectionsTest()
        {
            // Only sections named section1 and section3 are defined.
            var page = CreatePageWithLayout(
                p =>
                {
                    p.DefineSection("section1", () => { p.Write("section1 "); });
                    p.DefineSection("section3", () => { p.Write("section3"); });
                },
                p =>
                {
                    p.Write(p.RenderSection("section1", required: false));
                    p.Write(p.RenderSection("section2", required: false));
                    p.Write(p.RenderSection("section3", required: false));
                    p.Write(p.RenderSection("section4", required: false));
                });
            var result = Utils.RenderWebPage(page);
            var expected = "section1 section3";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PageDataTest()
        {
            // Layout page uses items in PageData set by content page
            var contents = "my contents";
            var page = CreatePageWithLayout(
                p =>
                {
                    p.PageData["contents"] = contents;
                    p.Write(" body");
                },
                p =>
                {
                    p.Write(p.PageData["contents"]);
                    p.Write(p.RenderBody());
                });
            var result = Utils.RenderWebPage(page);
            var expected = contents + " body";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RenderPageAndLayoutPage()
        {
            //Dev10 bug 928341 - a page that has a layout page, and the page calls RenderPage should not cause an error
            var layoutPagePath = "~/layout.cshtml";
            var page = Utils.CreatePage(p =>
            {
                p.DefineSection("foo", () => { p.Write("This is foo"); });
                p.Write(p.RenderPage("bar.cshtml"));
                p.Layout = layoutPagePath;
            });
            var layoutPage = Utils.CreatePage(p =>
            {
                p.Write(p.RenderBody());
                p.Write(" ");
                p.Write(p.RenderSection("foo"));
            }, layoutPagePath);

            var subPage = Utils.CreatePage(p => p.Write("This is bar"), "~/bar.cshtml");
            Utils.AssignObjectFactoriesAndDisplayModeProvider(page, layoutPage, subPage);

            var result = Utils.RenderWebPage(page);
            var expected = "This is bar This is foo";
            Assert.Equal(expected, result);
        }

        public static string RenderPageWithLayout(Action<WebPage> pageExecuteAction, Action<WebPage> layoutExecuteAction,
                                                  string pagePath = "~/index.cshtml", string layoutPath = "~/Layout.cshtml", string layoutPage = "Layout.cshtml")
        {
            var page = CreatePageWithLayout(pageExecuteAction, layoutExecuteAction, pagePath, layoutPath, layoutPage);
            return Utils.RenderWebPage(page);
        }

        public static MockPage CreatePageWithLayout(Action<WebPage> pageExecuteAction, Action<WebPage> layoutExecuteAction,
                                                    string pagePath = "~/index.cshtml", string layoutPath = "~/Layout.cshtml", string layoutPageName = "Layout.cshtml")
        {
            var page = Utils.CreatePage(
                p =>
                {
                    p.Layout = layoutPageName;
                    pageExecuteAction(p);
                },
                pagePath);
            var layoutPage = Utils.CreatePage(
                p => { layoutExecuteAction(p); },
                layoutPath);

            Utils.AssignObjectFactoriesAndDisplayModeProvider(layoutPage, page);

            return page;
        }
    }
}
