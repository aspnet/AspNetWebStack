// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using System.Web.WebPages.TestUtils;
using Moq;

namespace System.Web.WebPages.Test
{
    public static class Utils
    {
        public static string RenderWebPage(WebPage page, StartPage startPage = null, HttpRequestBase request = null)
        {
            var writer = new StringWriter();

            // Create an actual dummy HttpContext that has a request object
            var filename = "default.aspx";
            var url = "http://localhost/default.aspx";

            request = request ?? CreateTestRequest(filename, url).Object;
            var httpContext = CreateTestContext(request);

            var pageContext = new WebPageContext { HttpContext = httpContext.Object };
            page.ExecutePageHierarchy(pageContext, writer, startPage);
            return writer.ToString();
        }

        public static Mock<HttpContextBase> CreateTestContext(HttpRequestBase request = null, HttpResponseBase response = null, IDictionary items = null)
        {
            items = items ?? new Hashtable();
            request = request ?? CreateTestRequest("default.cshtml", "http://localhost/default.cshtml").Object;

            if (response == null)
            {
                var mockResponse = new Mock<HttpResponseBase>();
                mockResponse.Setup(r => r.Cookies).Returns(new HttpCookieCollection());
                response = mockResponse.Object;
            }

            var httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(c => c.Items).Returns(items);
            httpContext.SetupGet(c => c.Request).Returns(request);
            httpContext.SetupGet(c => c.Response).Returns(response);
            return httpContext;
        }

        public static Mock<HttpRequestBase> CreateTestRequest(string filename, string url)
        {
            var mockRequest = new Mock<HttpRequestBase> { CallBase = true };
            mockRequest.SetupGet(r => r.Path).Returns(filename);
            mockRequest.SetupGet(r => r.RawUrl).Returns(url);
            mockRequest.SetupGet(r => r.IsLocal).Returns(false);
            mockRequest.SetupGet(r => r.QueryString).Returns(new NameValueCollection());
            mockRequest.SetupGet(r => r.Browser.IsMobileDevice).Returns(false);
            mockRequest.SetupGet(r => r.Cookies).Returns(new HttpCookieCollection());
            mockRequest.SetupGet(r => r.UserAgent).Returns(String.Empty);

            return mockRequest;
        }

        public static string RenderWebPage(Action<WebPage> pageExecuteAction, string pagePath = "~/index.cshtml")
        {
            var page = CreatePage(pageExecuteAction, pagePath);
            return RenderWebPage(page);
        }

        public static MockPage CreatePage(Action<WebPage> pageExecuteAction, string pagePath = "~/index.cshtml")
        {
            var page = new MockPage()
            {
                VirtualPath = pagePath,
                ExecuteAction = p => { pageExecuteAction(p); }
            };
            page.VirtualPathFactory = new HashVirtualPathFactory(page);
            page.DisplayModeProvider = new DisplayModeProvider();
            return page;
        }

        public static MockStartPage CreateStartPage(Action<StartPage> pageExecuteAction, string pagePath = "~/_pagestart.cshtml")
        {
            var page = new MockStartPage()
            {
                VirtualPath = pagePath,
                ExecuteAction = p => { pageExecuteAction(p); }
            };
            page.VirtualPathFactory = new HashVirtualPathFactory(page);
            page.DisplayModeProvider = new DisplayModeProvider();
            return page;
        }

        public static string RenderWebPageWithSubPage(Action<WebPage> pageExecuteAction, Action<WebPage> subpageExecuteAction,
                                                      string pagePath = "~/index.cshtml", string subpagePath = "~/subpage.cshtml")
        {
            var page = CreatePage(pageExecuteAction);
            var subPage = CreatePage(subpageExecuteAction, subpagePath);
            var virtualPathFactory = new HashVirtualPathFactory(page, subPage);
            subPage.VirtualPathFactory = virtualPathFactory;
            page.VirtualPathFactory = virtualPathFactory;
            return RenderWebPage(page);
        }

        // E.g. "default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"

        /// <summary>
        /// Creates an instance of HttpContext and assigns it to HttpContext.Current. Ensure that the returned value is disposed at the end of the test.
        /// </summary>
        /// <returns>Returns an IDisposable that restores the original HttpContext.</returns>
        internal static IDisposable CreateHttpContext(string filename, string url)
        {
            var request = new HttpRequest(filename, url, null);
            var httpContext = new HttpContext(request, new HttpResponse(new StringWriter(new StringBuilder())));
            HttpContext.Current = httpContext;

            return new DisposableAction(RestoreHttpContext);
        }

        internal static void RestoreHttpContext()
        {
            HttpContext.Current = null;
        }

        internal static IDisposable CreateHttpRuntime(string appVPath)
        {
            return WebUtils.CreateHttpRuntime(appVPath);
        }

        public static void SetupVirtualPathInAppDomain(string vpath, string contents)
        {
            var file = new Mock<VirtualFile>(vpath);
            file.Setup(f => f.Open()).Returns(new MemoryStream(ASCIIEncoding.Default.GetBytes(contents)));
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(p => p.FileExists(vpath)).Returns(true);
            vpp.Setup(p => p.GetFile(vpath)).Returns(file.Object);
            AppDomainUtils.SetAppData();
            var env = new HostingEnvironment();

            var register = typeof(HostingEnvironment).GetMethod("RegisterVirtualPathProviderInternal", BindingFlags.Static | BindingFlags.NonPublic);
            register.Invoke(null, new object[] { vpp.Object });
        }

        /// <summary>
        /// Assigns a common object factory to the pages.
        /// </summary>
        internal static IVirtualPathFactory AssignObjectFactoriesAndDisplayModeProvider(params WebPageExecutingBase[] pages)
        {
            var objectFactory = new HashVirtualPathFactory(pages);
            var displayModeProvider = new DisplayModeProvider();
            foreach (var item in pages)
            {
                item.VirtualPathFactory = objectFactory;
                var webPageRenderingBase = item as WebPageRenderingBase;
                if (webPageRenderingBase != null)
                {
                    webPageRenderingBase.DisplayModeProvider = displayModeProvider;
                }
            }

            return objectFactory;
        }

        internal static DisplayModeProvider AssignDisplayModeProvider(params WebPageRenderingBase[] pages)
        {
            var displayModeProvider = new DisplayModeProvider();
            foreach (var item in pages)
            {
                item.DisplayModeProvider = displayModeProvider;
            }
            return displayModeProvider;
        }
    }

    public class MockPageHelper
    {
        internal static string GetDirectory(string virtualPath)
        {
            var dir = Path.GetDirectoryName(virtualPath);
            if (dir == "~")
            {
                return null;
            }
            return dir;
        }
    }

    // This is a mock implementation of WebPage mainly to make the Render method work and
    // generate a string.
    // The Execute method simulates what is typically generated based on markup by the parsers.
    public class MockPage : WebPage
    {
        public Action<WebPage> ExecuteAction { get; set; }

        internal override string GetDirectory(string virtualPath)
        {
            return MockPageHelper.GetDirectory(virtualPath);
        }

        public override void Execute()
        {
            ExecuteAction(this);
        }
    }

    public class MockStartPage : StartPage
    {
        public Action<StartPage> ExecuteAction { get; set; }

        internal override string GetDirectory(string virtualPath)
        {
            return MockPageHelper.GetDirectory(virtualPath);
        }

        public override void Execute()
        {
            ExecuteAction(this);
        }

        public Dictionary<string, object> CombinedPageInstances
        {
            get
            {
                var combinedInstances = new Dictionary<string, object>();
                var instances = new Dictionary<string, object>();
                WebPageRenderingBase childPage = this;
                while (childPage != null)
                {
                    if (childPage is MockStartPage)
                    {
                        var initPage = childPage as MockStartPage;
                        childPage = initPage.ChildPage;
                    }
                    else if (childPage is MockPage)
                    {
                        childPage = null;
                    }
                    foreach (var kvp in instances)
                    {
                        combinedInstances.Add(kvp.Key, kvp.Value);
                    }
                }
                return combinedInstances;
            }
        }
    }

    public class MockHttpRuntime
    {
        public static Version RequestValidationMode { get; set; }
    }

    public class MockHttpApplication
    {
        public static Type ModuleType { get; set; }

        public static void RegisterModule(Type moduleType)
        {
            ModuleType = moduleType;
        }
    }
}
