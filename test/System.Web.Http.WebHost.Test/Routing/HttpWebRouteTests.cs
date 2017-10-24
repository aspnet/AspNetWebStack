// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Web.Http.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost.Routing
{
    public class HttpWebRouteTests
    {
        [Fact]
        public void GetRouteData_IfHttpRouteGetRouteDataThrows_RoutesToExceptionHandler()
        {
            // Arrange
            Exception expectedException = CreateException();
            IHttpRoute route = CreateThrowingRoute(expectedException);

            HttpWebRoute product = CreateProductUnderTest(route);

            HttpRequestBase requestBase = CreateStubRequestBase("GET");
            HttpContextBase contextBase = CreateStubContextBase(requestBase);

            using (HttpRequestMessage request = CreateRequest())
            {
                contextBase.SetHttpRequestMessage(request);

                // Act
                RouteData routeData = product.GetRouteData(contextBase);

                // Assert
                Assert.NotNull(routeData);
                Assert.Same(product, routeData.Route);
                HttpRouteExceptionRouteHandler typedHandler = Assert.IsType<HttpRouteExceptionRouteHandler>(routeData.RouteHandler);
                ExceptionDispatchInfo exceptionInfo = typedHandler.ExceptionInfo;
                Assert.NotNull(exceptionInfo); // Guard
                Assert.Same(expectedException, exceptionInfo.SourceException);
            }
        }

        private static Exception CreateException()
        {
            return new NotFiniteNumberException();
        }

        private static HttpWebRoute CreateProductUnderTest(IHttpRoute httpRoute)
        {
            return new HttpWebRoute(null, null, null, null, null, httpRoute);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpContextBase CreateStubContextBase(HttpRequestBase request)
        {
            Mock<HttpContextBase> mock = new Mock<HttpContextBase>();
            mock.SetupGet(m => m.Request).Returns(request);
            IDictionary items = new Dictionary<object, object>();
            mock.SetupGet(m => m.Items).Returns(items);
            return mock.Object;
        }

        private static HttpRequestBase CreateStubRequestBase(string httpMethod)
        {
            return new Mock<HttpRequestBase>().Object;
        }

        private static IHttpRoute CreateThrowingRoute(Exception exception)
        {
            Mock<IHttpRoute> mock = new Mock<IHttpRoute>(MockBehavior.Strict);
            mock
                .Setup(m => m.GetRouteData(It.IsAny<string>(), It.IsAny<HttpRequestMessage>()))
                .Throws(exception);
            return mock.Object;
        }
    }
}
