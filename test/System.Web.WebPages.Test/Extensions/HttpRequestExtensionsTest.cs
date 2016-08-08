// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.WebPages.Test.Helpers
{
    public class HttpRequestExtensionsTest
    {
        private static HttpRequestBase GetRequestForIsUrlLocalToHost(string url)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.Url).Returns(new Uri(url));
            return contextMock.Object.Request;
        }

        [Fact]
        public void IsUrlLocalToHost_ReturnsFalseOnEmpty()
        {
            var request = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(request.IsUrlLocalToHost(null));
            Assert.False(request.IsUrlLocalToHost(String.Empty));
        }

        [Fact]
        public void IsUrlLocalToHost_AcceptsRootedUrls()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.True(helper.IsUrlLocalToHost("/foo.html"));
            Assert.True(helper.IsUrlLocalToHost("/www.hackerz.com"));
            Assert.True(helper.IsUrlLocalToHost("/"));
        }

        [Fact]
        public void IsUrlLocalToHost_AcceptsApplicationRelativeUrls()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.True(helper.IsUrlLocalToHost("~/"));
            Assert.True(helper.IsUrlLocalToHost("~/foobar.html"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsRelativeUrls()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("../foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("fold/foobar.html"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectValidButUnsafeRelativeUrls()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("http:/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("hTtP:foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("http:/www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("HtTpS:/www.hackerz.com"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsOnTheSameHost()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("http://www.mysite.com/appDir/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("http://WWW.MYSITE.COM"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsOnLocalHost()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("http://localhost/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("http://127.0.0.1/foobar.html"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsOnTheSameHostButDifferentScheme()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("https://www.mysite.com/"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsOnDifferentHost()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("http://www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("https://www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("hTtP://www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("HtTpS://www.hackerz.com"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsWithTooManySchemeSeparatorCharacters()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("http://///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("https://///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("HtTpS://///www.hackerz.com/foobar.html"));

            Assert.False(helper.IsUrlLocalToHost("http:///www.hackerz.com/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("http:////www.hackerz.com/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("http://///www.hackerz.com/foobar.html"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsUrlsWithMissingSchemeName()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost("//www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("//www.hackerz.com?"));
            Assert.False(helper.IsUrlLocalToHost("//www.hackerz.com:80"));
            Assert.False(helper.IsUrlLocalToHost("//www.hackerz.com/foobar.html"));
            Assert.False(helper.IsUrlLocalToHost("///www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost("//////www.hackerz.com"));
        }

        [Fact]
        public void IsUrlLocalToHost_RejectsInvalidUrls()
        {
            var helper = GetRequestForIsUrlLocalToHost("http://www.mysite.com/");
            Assert.False(helper.IsUrlLocalToHost(@"http:\\www.hackerz.com"));
            Assert.False(helper.IsUrlLocalToHost(@"http:\\www.hackerz.com\"));
            Assert.False(helper.IsUrlLocalToHost(@"/\"));
            Assert.False(helper.IsUrlLocalToHost(@"/\foo"));
        }
    }
}
