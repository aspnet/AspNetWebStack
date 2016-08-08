// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class HttpRouteTest
    {
        [Theory]
        [InlineData("123 456")]
        [InlineData("123 {456")]
        [InlineData("123 [45]6")]
        [InlineData("123 (4)56")]
        [InlineData("abc+56")]
        [InlineData("abc.56")]
        [InlineData("abc*56")]
        [InlineData(@"hello12.1[)]*^$=!@23}")]
        public void GetRouteData_HandlesUrlEncoding(string id)
        {
            HttpRoute route = new HttpRoute("{controller}/{id}");
            Uri uri = new Uri("http://localhost/test/" + id + "/");
            IHttpRouteData routeData = route.GetRouteData("", new HttpRequestMessage(HttpMethod.Get, uri));
            Assert.Equal("test", routeData.Values["controller"]);
            Assert.Equal(id, routeData.Values["id"]);
        }
    }
}
