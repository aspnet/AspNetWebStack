// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using System.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class ApplicationStartPageTest
    {
        [Fact]
        public void StartPageBasicTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var page = new ApplicationStartPageTest().CreateStartPage(p =>
                {
                    p.AppState["x"] = "y";
                    p.WriteLiteral("test");
                });
                page.ExecuteInternal();
                Assert.Equal("y", page.ApplicationState["x"]);
                Assert.Equal("test", ApplicationStartPage.Markup.ToHtmlString());
            });
        }

        [Fact]
        public void StartPageDynamicAppStateBasicTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var page = new ApplicationStartPageTest().CreateStartPage(p =>
                {
                    p.App.x = "y";
                    p.WriteLiteral("test");
                });
                page.ExecuteInternal();
                Assert.Equal("y", page.ApplicationState["x"]);
                Assert.Equal("y", page.App["x"]);
                Assert.Equal("y", page.App.x);
                Assert.Equal("test", ApplicationStartPage.Markup.ToHtmlString());
            });
        }

        [Fact]
        public void ExceptionTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var msg = "This is an error message";
                var e = new InvalidOperationException(msg);
                var page = new ApplicationStartPageTest().CreateStartPage(p => { throw e; });
                var ex = Assert.Throws<HttpException>(() => page.ExecuteStartPage());
                Assert.Equal(msg, ex.InnerException.Message);
                Assert.Equal(e, ApplicationStartPage.Exception);
            });
        }

        [Fact]
        public void HtmlEncodeTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Set HideRequestResponse to true to simulate the condition in IIS 7/7.5
                var context = new HttpContext(new HttpRequest("default.cshtml", "http://localhost/default.cshtml", null), new HttpResponse(new StringWriter(new StringBuilder())));
                var hideRequestResponse = typeof(HttpContext).GetField("HideRequestResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                hideRequestResponse.SetValue(context, true);

                HttpContext.Current = context;
                var page = new ApplicationStartPageTest().CreateStartPage(p => { p.Write("test"); });
                page.ExecuteStartPage();
            });
        }

        [Fact]
        public void GetVirtualPathTest()
        {
            var page = new MockStartPage();
            Assert.Equal(ApplicationStartPage.StartPageVirtualPath, page.VirtualPath);
        }

        [Fact]
        public void SetVirtualPathTest()
        {
            var page = new MockStartPage();
            Assert.Throws<NotSupportedException>(() => { page.VirtualPath = "~/hello.cshtml"; });
        }

        [Fact]
        public void ExecuteStartPageTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var startPage = new MockStartPage() { ExecuteAction = p => p.AppState["x"] = "y" };
                var objectFactory = GetMockVirtualPathFactory(startPage);
                ApplicationStartPage.ExecuteStartPage(new WebPageHttpApplication(),
                                                      p => { },
                                                      objectFactory,
                                                      new string[] { "cshtml", "vbhtml" });
                Assert.Equal("y", startPage.ApplicationState["x"]);
            });
        }

        [Fact]
        public void ExecuteStartPageDynamicAppStateTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var startPage = new MockStartPage() { ExecuteAction = p => p.App.x = "y" };
                var objectFactory = GetMockVirtualPathFactory(startPage);
                ApplicationStartPage.ExecuteStartPage(new WebPageHttpApplication(),
                                                      p => { },
                                                      objectFactory,
                                                      new string[] { "cshtml", "vbhtml" });
                Assert.Equal("y", startPage.ApplicationState["x"]);
                Assert.Equal("y", startPage.App.x);
                Assert.Equal("y", startPage.App["x"]);
            });
        }

        public class MockStartPage : ApplicationStartPage
        {
            public Action<ApplicationStartPage> ExecuteAction { get; set; }
            public HttpApplicationStateBase ApplicationState = new HttpApplicationStateWrapper(Activator.CreateInstance(typeof(HttpApplicationState), true) as HttpApplicationState);

            public override void Execute()
            {
                ExecuteAction(this);
            }

            public override HttpApplicationStateBase AppState
            {
                get { return ApplicationState; }
            }

            public void ExecuteStartPage()
            {
                ExecuteStartPage(new WebPageHttpApplication(),
                                 p => { },
                                 GetMockVirtualPathFactory(this),
                                 new string[] { "cshtml", "vbhtml" });
            }
        }

        public MockStartPage CreateStartPage(Action<ApplicationStartPage> action)
        {
            var startPage = new MockStartPage() { ExecuteAction = action };
            return startPage;
        }

        public sealed class WebPageHttpApplication : HttpApplication
        {
        }

        private static IVirtualPathFactory GetMockVirtualPathFactory(ApplicationStartPage page)
        {
            var mockFactory = new Mock<IVirtualPathFactory>();
            mockFactory.Setup(c => c.Exists(It.IsAny<string>())).Returns<string>(_ => true);
            mockFactory.Setup(c => c.CreateInstance(It.IsAny<string>())).Returns<string>(_ => page);

            return mockFactory.Object;
        }
    }
}
