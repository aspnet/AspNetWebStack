// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Mvc.Test
{
    public class ContentTypeAttributeTest
    {
        [Fact]
        public void ContentTypeSetInCtor()
        {
            var attr = new ContentTypeAttribute("text/html");
            Assert.Equal("text/html", attr.ContentType);
        }

        [Fact]
        public void ContentTypeCtorThrowsArgumentExceptionWhenContentTypeIsNull()
        {
            Assert.ThrowsArgumentNullOrEmpty(() => new ContentTypeAttribute(null), "contentType");
        }

        [Fact]
        public void ExecuteResultSetsContentType()
        {
            var mockHttpResponse = new Mock<HttpResponseBase>();
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Response).Returns(mockHttpResponse.Object);

            var mockController = new Mock<Controller>();
            var controllerContext = new ControllerContext(new RequestContext(mockHttpContext.Object, new RouteData()), mockController.Object);
            var result = new ContentResult { Content = "blah blah" };
            var filterContext = new ResultExecutingContext(controllerContext, result);

            var filter = new ContentTypeAttribute("text/xml");
            filter.OnResultExecuting(filterContext);

            mockHttpResponse.VerifySet(r => r.ContentType = "text/xml");
        }
    }
}
