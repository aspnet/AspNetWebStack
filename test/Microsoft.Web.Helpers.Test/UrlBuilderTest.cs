// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    public class UrlBuilderTest
    {
        private static TestVirtualPathUtility _virtualPathUtility = new TestVirtualPathUtility();

        [Fact]
        public void UrlBuilderUsesPathAsIsIfPathIsAValidUri()
        {
            // Arrange
            var pagePath = "http://www.test.com/page-path";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal("http://www.test.com/page-path", builder.Path);
        }

        [Fact]
        public void UrlBuilderUsesQueryComponentsIfPathIsAValidUri()
        {
            // Arrange
            var pagePath = "http://www.test.com/page-path.vbhtml?param=value&baz=biz";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal("http://www.test.com/page-path.vbhtml", builder.Path);
            Assert.Equal("?param=value&baz=biz", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderUsesUsesObjectParameterAsQueryString()
        {
            // Arrange
            var pagePath = "http://www.test.com/page-path.vbhtml?param=value&baz=biz";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal("http://www.test.com/page-path.vbhtml", builder.Path);
            Assert.Equal("?param=value&baz=biz", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderAppendsObjectParametersToPathWithQueryString()
        {
            // Arrange
            var pagePath = "http://www.test.com/page-path.vbhtml?param=value&baz=biz";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, new { param2 = "param2val" });

            // Assert
            Assert.Equal("http://www.test.com/page-path.vbhtml", builder.Path);
            Assert.Equal("?param=value&baz=biz&param2=param2val", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderWithVirtualPathUsesVirtualPathUtility()
        {
            // Arrange
            var pagePath = "~/page";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal("page", builder.Path);
        }

        [Fact]
        public void UrlBuilderDoesNotResolvePathIfContextIsNull()
        {
            // Arrange
            var pagePath = "~/foo/bar";

            // Act
            var builder = new UrlBuilder(null, _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal(pagePath, builder.Path);
        }

        [Fact]
        public void UrlBuilderSetsPathToNullIfContextIsNullAndPagePathIsNotSpecified()
        {
            // Act
            var builder = new UrlBuilder(null, null, null, null);

            // Assert
            Assert.Null(builder.Path);
            Assert.True(String.IsNullOrEmpty(builder.ToString()));
        }

        [Fact]
        public void UrlBuilderWithVirtualPathAppendsObjectAsQueryString()
        {
            // Arrange
            var pagePath = "~/page";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, new { Foo = "bar", baz = "qux" });

            // Assert
            Assert.Equal("page", builder.Path);
            Assert.Equal("?Foo=bar&baz=qux", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderWithVirtualPathExtractsQueryStringParameterFromPath()
        {
            // Arrange
            var pagePath = "~/dir/page?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);

            // Assert
            Assert.Equal("dir/page", builder.Path);
            Assert.Equal("?someparam=value", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderWithVirtualPathAndQueryStringAppendsObjectAsQueryStringParams()
        {
            // Arrange
            var pagePath = "~/dir/page?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, new { someotherparam = "value2" });

            // Assert
            Assert.Equal("dir/page", builder.Path);
            Assert.Equal("?someparam=value&someotherparam=value2", builder.QueryString);
        }

        [Fact]
        public void AddPathAddsPathPortionToRelativeUrl()
        {
            // Arrange
            var pagePath = "~/dir/page?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo").AddPath("bar/baz");

            // Assert
            Assert.Equal("dir/page/foo/bar/baz", builder.Path);
            Assert.Equal("?someparam=value", builder.QueryString);
        }

        [Fact]
        public void AddPathEncodesPathParams()
        {
            // Arrange
            var pagePath = "~/dir/page?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo bar").AddPath("baz biz", "qux");

            // Assert
            Assert.Equal("dir/page/foo%20bar/baz%20biz/qux", builder.Path);
        }

        [Fact]
        public void AddPathAddsPathPortionToAbsoluteUrl()
        {
            // Arrange
            var pagePath = "http://some-site/dir/page?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo").AddPath("bar/baz");

            // Assert
            Assert.Equal("http://some-site/dir/page/foo/bar/baz", builder.Path);
            Assert.Equal("?someparam=value", builder.QueryString);
        }

        [Fact]
        public void AddPathWithParamsArrayAddsPathPortionToRelativeUrl()
        {
            // Arrange
            var pagePath = "~/dir/page/?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo", "bar", "baz").AddPath("qux");

            // Assert
            Assert.Equal("dir/page/foo/bar/baz/qux", builder.Path);
            Assert.Equal("?someparam=value", builder.QueryString);
        }

        [Fact]
        public void AddPathEnsuresSlashesAreNotRepeated()
        {
            // Arrange
            var pagePath = "~/dir/page/";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo").AddPath("/bar/").AddPath("/baz");

            // Assert
            Assert.Equal("dir/page/foo/bar/baz", builder.Path);
        }

        [Fact]
        public void AddPathWithParamsEnsuresSlashAreNotRepeated()
        {
            // Arrange
            var pagePath = "~/dir/page/";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo", "/bar/", "/baz").AddPath("qux/");

            // Assert
            Assert.Equal("dir/page/foo/bar/baz/qux/", builder.Path);
        }

        [Fact]
        public void AddPathWithParamsArrayAddsPathPortionToAbsoluteUrl()
        {
            // Arrange
            var pagePath = "http://www.test.com/dir/page/?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddPath("foo", "bar", "baz").AddPath("qux");

            // Assert
            Assert.Equal("http://www.test.com/dir/page/foo/bar/baz/qux", builder.Path);
            Assert.Equal("?someparam=value", builder.QueryString);
        }

        [Fact]
        public void UrlBuilderEncodesParameters()
        {
            // Arrange
            var pagePath = "~/dir/page/?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, new { Λ = "λ" });
            builder.AddParam(new { π = "is not a lie" }).AddParam("Π", "maybe a lie");
            // Assert
            Assert.Equal("?someparam=value&%ce%9b=%ce%bb&%cf%80=is+not+a+lie&%ce%a0=maybe+a+lie", builder.QueryString);
        }

        [Fact]
        public void AddParamAddsParamToQueryString()
        {
            // Arrange
            var pagePath = "http://www.test.com/dir/page/?someparam=value";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddParam("foo", "bar");

            // Assert
            Assert.Equal("?someparam=value&foo=bar", builder.QueryString);
        }

        [Fact]
        public void AddParamAddsQuestionMarkToQueryStringIfFirstParam()
        {
            // Arrange
            var pagePath = "~/dir/page";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddParam("foo", "bar").AddParam(new { baz = "qux", biz = "quark" });

            // Assert
            Assert.Equal("?foo=bar&baz=qux&biz=quark", builder.QueryString);
        }

        [Fact]
        public void AddParamIgnoresParametersWithEmptyKey()
        {
            // Arrange
            var pagePath = "~/dir/page";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddParam("", "bar").AddParam(new { baz = "", biz = "quark" }).AddParam("qux", null).AddParam(null, "somevalue");

            // Assert
            Assert.Equal("?baz=&biz=quark&qux=", builder.QueryString);
        }

        [Fact]
        public void ToStringConcatsPathAndQueryString()
        {
            // Arrange
            var pagePath = "~/dir/page";

            // Act
            var builder = new UrlBuilder(GetContext(), _virtualPathUtility, pagePath, null);
            builder.AddParam("foo", "bar").AddParam(new { baz = "qux", biz = "quark" });

            // Assert
            Assert.Equal("dir/page?foo=bar&baz=qux&biz=quark", builder.ToString());
        }

        [Fact]
        public void UrlBuilderWithRealVirtualPathUtilityTest()
        {
            // Arrange
            var pagePath = "~/world/test.aspx";
            try
            {
                // Act
                CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx");
                CreateHttpRuntime("/WebSite1/");
                var builder = new UrlBuilder(pagePath, null);

                // Assert
                Assert.Equal(@"/WebSite1/world/test.aspx", builder.Path);
            }
            finally
            {
                RestoreHttpRuntime();
            }
        }

        private static HttpContextBase GetContext(params string[] virtualPaths)
        {
            var httpContext = new Mock<HttpContextBase>();
            var table = new Hashtable();
            httpContext.SetupGet(c => c.Items).Returns(table);

            foreach (var item in virtualPaths)
            {
                var page = new Mock<ITemplateFile>();
                page.SetupGet(p => p.TemplateInfo).Returns(new TemplateFileInfo(item));
                TemplateStack.Push(httpContext.Object, page.Object);
            }

            return httpContext.Object;
        }

        private class TestVirtualPathUtility : VirtualPathUtilityBase
        {
            public override string Combine(string basePath, string relativePath)
            {
                return basePath + '/' + relativePath.TrimStart('~', '/');
            }

            public override string ToAbsolute(string virtualPath)
            {
                return virtualPath.TrimStart('~', '/');
            }
        }

        internal static void CreateHttpRuntime(string appVPath)
        {
            var runtime = new HttpRuntime();
            var appDomainAppVPathField = typeof(HttpRuntime).GetField("_appDomainAppVPath", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            appDomainAppVPathField.SetValue(runtime, CreateVirtualPath(appVPath));
            GetTheRuntime().SetValue(null, runtime);
            var appDomainIdField = typeof(HttpRuntime).GetField("_appDomainId", BindingFlags.NonPublic | BindingFlags.Instance);
            appDomainIdField.SetValue(runtime, "test");
        }

        internal static FieldInfo GetTheRuntime()
        {
            return typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static);
        }

        internal static void RestoreHttpRuntime()
        {
            GetTheRuntime().SetValue(null, null);
        }

        // E.g. "default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"
        internal static void CreateHttpContext(string filename, string url)
        {
            var request = new HttpRequest(filename, url, null);
            var httpContext = new HttpContext(request, new HttpResponse(new StringWriter(new StringBuilder())));
            HttpContext.Current = httpContext;
        }

        internal static void RestoreHttpContext()
        {
            HttpContext.Current = null;
        }

        internal static object CreateVirtualPath(string path)
        {
            var vPath = typeof(Page).Assembly.GetType("System.Web.VirtualPath");
            var method = vPath.GetMethod("CreateNonRelativeTrailingSlash", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return method.Invoke(null, new object[] { path });
        }
    }
}
