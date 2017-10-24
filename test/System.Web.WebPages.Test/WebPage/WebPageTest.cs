// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Caching;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class WebPageTest
    {
        private const string XmlHttpRequestKey = "X-Requested-With";
        private const string XmlHttpRequestValue = "XMLHttpRequest";

        [Fact]
        public void CreatePageFromVirtualPathAssignsVirtualPathFactory()
        {
            // Arrange
            var path = "~/index.cshtml";
            var page = Utils.CreatePage(null, path);
            var factory = new HashVirtualPathFactory(page);

            // Act
            var result = WebPage.CreateInstanceFromVirtualPath(path, factory);

            // Assert
            Assert.Equal(page, result);
            Assert.Equal(page.VirtualPathFactory, factory);
            Assert.Equal(page.VirtualPath, path);
        }

        [Fact]
        public void NormalizeLayoutPagePathTest()
        {
            var layoutPage = "Layout.cshtml";
            var layoutPath1 = "~/MyApp/Layout.cshtml";
            var page = new Mock<WebPage>() { CallBase = true }.Object;
            page.VirtualPath = "~/MyApp/index.cshtml";

            var mockBuildManager = new Mock<IVirtualPathFactory>();
            mockBuildManager.Setup(c => c.Exists(It.IsAny<string>())).Returns<string>(p => p.Equals(layoutPath1, StringComparison.OrdinalIgnoreCase));
            page.VirtualPathFactory = mockBuildManager.Object;

            Assert.Equal(layoutPath1, page.NormalizeLayoutPagePath(layoutPage));

            mockBuildManager.Setup(c => c.Exists(It.IsAny<string>())).Returns<string>(_ => false);

            Assert.Throws<HttpException>(() => page.NormalizeLayoutPagePath(layoutPage),
                                                  @"The layout page ""Layout.cshtml"" could not be found at the following path: ""~/MyApp/Layout.cshtml"".");
        }

        [Fact]
        public void UrlDataBasicTests()
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            string pathInfo = String.Format("{0}/{1}/{2}/{3}", "one", 2, 3.0, 4.0005);
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", pathInfo);
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.Equal("one", page.UrlData[0]);
            Assert.Equal(2, page.UrlData[1].AsInt());
            Assert.Equal(3.0f, page.UrlData[2].AsFloat());
            Assert.Equal(4.0005m, page.UrlData[3].AsDecimal());
        }

        [Fact]
        public void UrlDataEmptyTests()
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "one///two/");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.Equal("one", page.UrlData[0]);
            Assert.True(page.UrlData[1].IsEmpty());
            Assert.True(page.UrlData[2].IsEmpty());
            Assert.Equal("two", page.UrlData[3]);
            Assert.True(page.UrlData[4].IsEmpty());
        }

        [Fact]
        public void UrlDataReadOnlyTest()
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "one/2/3.0/4.0005");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.Throws<NotSupportedException>(() => { page.UrlData.Add("bogus"); }, "The UrlData collection is read-only.");
            Assert.Throws<NotSupportedException>(() => { page.UrlData.Insert(0, "bogus"); }, "The UrlData collection is read-only.");
            Assert.Throws<NotSupportedException>(() => { page.UrlData.Remove("one"); }, "The UrlData collection is read-only.");
        }

        [Fact]
        public void UrlDataOutOfBoundsTest()
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(context => context.Items).Returns(new Hashtable());
            mockContext.Object.Items[typeof(WebPageMatch)] = new WebPageMatch("~/a.cshtml", "");
            WebPage page = new Mock<WebPage>() { CallBase = true }.Object;
            page.Context = mockContext.Object;

            Assert.Equal(String.Empty, page.UrlData[0]);
            Assert.Equal(String.Empty, page.UrlData[1]);
        }

        [Fact]
        public void NullModelTest()
        {
            var page = CreateMockPageWithPostContext().Object;
            page.PageContext.Model = null;
            Assert.Null(page.Model);
        }

        internal class ModelTestClass
        {
            public string Prop1 { get; set; }

            public string GetProp1()
            {
                return Prop1;
            }

            public override string ToString()
            {
                return Prop1;
            }
        }

        [Fact]
        public void ModelTest()
        {
            var v = "value1";
            var page = CreateMockPageWithPostContext().Object;
            var model = new ModelTestClass() { Prop1 = v };
            page.PageContext.Model = model;
            Assert.NotNull(page.Model);
            Assert.Equal(v, page.Model.Prop1);
            Assert.Equal(v, page.Model.GetProp1());
            Assert.Equal(v, page.Model.ToString());
            Assert.Equal(model, (ModelTestClass)page.Model);
            // No such property
            Assert.Null(page.Model.Prop2);
            // No such method
            Assert.Throws<MissingMethodException>(() => page.Model.DoSomething());
        }

        [Fact]
        public void AnonymousObjectModelTest()
        {
            var v = "value1";
            var page = CreateMockPageWithPostContext().Object;
            var model = new { Prop1 = v };
            page.PageContext.Model = model;
            Assert.NotNull(page.Model);
            Assert.Equal(v, page.Model.Prop1);
            // No such property
            Assert.Null(page.Model.Prop2);
            // No such method
            Assert.Throws<MissingMethodException>(() => page.Model.DoSomething());
        }

        [Fact]
        public void SessionPropertyTest()
        {
            var page = CreateMockPageWithPostContext().Object;
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, page.Session.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public void AppStatePropertyTest()
        {
            var page = CreateMockPageWithPostContext().Object;
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, page.AppState.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public void ExecutePageHierarchyTest()
        {
            var page = new Mock<WebPage>();
            page.Object.TopLevelPage = true;

            var executors = new List<IWebPageRequestExecutor>();

            // First executor returns false
            var executor1 = new Mock<IWebPageRequestExecutor>();
            executor1.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(false);
            executors.Add(executor1.Object);

            // Second executor returns true
            var executor2 = new Mock<IWebPageRequestExecutor>();
            executor2.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(true);
            executors.Add(executor2.Object);

            // Third executor should never get called, since we stop after the first true
            var executor3 = new Mock<IWebPageRequestExecutor>();
            executor3.Setup(exec => exec.Execute(It.IsAny<WebPage>())).Returns(false);
            executors.Add(executor3.Object);

            page.Object.ExecutePageHierarchy(executors);

            // Make sure the first two got called but not the third
            executor1.Verify(exec => exec.Execute(It.IsAny<WebPage>()));
            executor2.Verify(exec => exec.Execute(It.IsAny<WebPage>()));
            executor3.Verify(exec => exec.Execute(It.IsAny<WebPage>()), Times.Never());
        }

        [Fact]
        public void IsPostReturnsTrueWhenMethodIsPost()
        {
            // Arrange
            var page = CreateMockPageWithPostContext();

            // Act and Assert
            Assert.True(page.Object.IsPost);
        }

        [Fact]
        public void IsPostReturnsFalseWhenMethodIsNotPost()
        {
            // Arrange
            var methods = new[] { "GET", "DELETE", "PUT", "RANDOM" };

            // Act and Assert
            Assert.True(methods.All(method => !CreateMockPageWithContext(method).Object.IsPost));
        }

        [Fact]
        public void IsAjaxReturnsTrueWhenRequestContainsAjaxHeader()
        {
            // Arrange
            var headers = new NameValueCollection();
            headers.Add("X-Requested-With", "XMLHttpRequest");
            var context = CreateContext("GET", new NameValueCollection(), headers);
            var page = CreatePage(context);

            // Act and Assert
            Assert.True(page.Object.IsAjax);
        }

        [Fact]
        public void IsAjaxReturnsTrueWhenRequestBodyContainsAjaxHeader()
        {
            // Arrange
            var headers = new NameValueCollection();
            headers.Add("X-Requested-With", "XMLHttpRequest");
            var context = CreateContext("POST", headers, headers);
            var page = CreatePage(context);

            // Act and Assert
            Assert.True(page.Object.IsAjax);
        }

        [Fact]
        public void IsAjaxReturnsFalseWhenRequestDoesNotContainAjaxHeaders()
        {
            // Arrange
            var page = CreateMockPageWithPostContext();

            // Act and Assert
            Assert.True(!page.Object.IsAjax);
        }

        private static Mock<WebPage> CreatePage(Mock<HttpContextBase> context)
        {
            var page = new Mock<WebPage>() { CallBase = true };
            var pageContext = new WebPageContext();
            page.Object.Context = context.Object;
            page.Object.PageContext = pageContext;
            return page;
        }

        private static Mock<WebPage> CreateMockPageWithPostContext()
        {
            return CreateMockPageWithContext("POST");
        }

        private static Mock<WebPage> CreateMockPageWithContext(string httpMethod)
        {
            var context = CreateContext(httpMethod, new NameValueCollection());
            var page = CreatePage(context);
            return page;
        }

        private static Mock<HttpContextBase> CreateContext(string httpMethod, NameValueCollection queryString, NameValueCollection httpHeaders = null)
        {
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.HttpMethod).Returns(httpMethod);
            request.Setup(r => r.QueryString).Returns(queryString);
            request.Setup(r => r.Form).Returns(new NameValueCollection());
            request.Setup(r => r.Files).Returns(new Mock<HttpFileCollectionBase>().Object);
            request.Setup(c => c.Headers).Returns(httpHeaders);
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Items).Returns(new Hashtable());
            context.Setup(c => c.Session).Returns(new Mock<HttpSessionStateBase>().Object);
            context.Setup(c => c.Application).Returns(new Mock<HttpApplicationStateBase>().Object);
            context.Setup(c => c.Cache).Returns(new Cache());
            context.Setup(c => c.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            return context;
        }
    }
}
