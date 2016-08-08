// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.WebPages.Test.Helpers
{
    public class HttpContextExtensionsTest
    {
        class RedirectData
        {
            public string RequestUrl { get; set; }
            public string RedirectUrl { get; set; }
        }

        private static HttpContextBase GetContextForRedirectLocal(RedirectData data)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.Url).Returns(new Uri(data.RequestUrl));
            contextMock.Setup(context => context.Response.Redirect(It.IsAny<string>())).Callback((string url) => data.RedirectUrl = url);
            return contextMock.Object;
        }

        [Fact]
        public void RedirectLocalWithNullGoesToRootTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.Equal("~/", data.RedirectUrl);
        }

        [Fact]
        public void RedirectLocalWithEmptyStringGoesToRootTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.Equal("~/", data.RedirectUrl);
        }

        [Fact]
        public void RedirectLocalWithNonLocalGoesToRootTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("");
            Assert.Equal("~/", data.RedirectUrl);
        }

        [Fact]
        public void RedirectLocalWithDifferentHostGoesToRootTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("http://bar");
            Assert.Equal("~/", data.RedirectUrl);
        }

        [Fact]
        public void RedirectLocalOnSameHostTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("http://foo/bar/baz");
            Assert.Equal("~/", data.RedirectUrl);
            context.RedirectLocal("http://foo/bar/baz/woot.htm");
            Assert.Equal("~/", data.RedirectUrl);
        }

        [Fact]
        public void RedirectLocalRelativeTest()
        {
            RedirectData data = new RedirectData() { RequestUrl = "http://foo" };
            var context = GetContextForRedirectLocal(data);
            context.RedirectLocal("/bar");
            Assert.Equal("/bar", data.RedirectUrl);
            context.RedirectLocal("/bar/hey.you");
            Assert.Equal("/bar/hey.you", data.RedirectUrl);
        }
    }
}
