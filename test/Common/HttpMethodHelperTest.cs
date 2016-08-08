// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class HttpMethodHelperTest
    {
        public static TheoryDataSet<string, HttpMethod> CommonHttpMethods
        {
            get
            {
                return new TheoryDataSet<string, HttpMethod>
                {
                    { "Get", HttpMethod.Get },
                    { "Post", HttpMethod.Post },
                    { "Put", HttpMethod.Put },
                    { "Delete", HttpMethod.Delete },
                    { "Head", HttpMethod.Head },
                    { "Options", HttpMethod.Options },
                    { "Trace", HttpMethod.Trace },
                };
            }
        }

        public static TheoryDataSet<string> UncommonHttpMethods
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "Debug",
                    "Patch",
                    "Connect",
                    "Random",
                    "M-Get",
                };
            }
        }

        [Fact]
        public void GetHttpMethod_ReturnsNullOnNullorEmpty()
        {
            Assert.Null(HttpMethodHelper.GetHttpMethod(null));
            Assert.Null(HttpMethodHelper.GetHttpMethod(String.Empty));
        }

        [Theory]
        [PropertyData("CommonHttpMethods")]
        public void GetHttpMethod_RetunsStaticResult(string method, HttpMethod expectedMethod)
        {
            Assert.Same(expectedMethod, HttpMethodHelper.GetHttpMethod(method));
            Assert.Same(expectedMethod, HttpMethodHelper.GetHttpMethod(method.ToLowerInvariant()));
            Assert.Same(expectedMethod, HttpMethodHelper.GetHttpMethod(method.ToUpperInvariant()));
        }

        [Theory]
        [PropertyData("UncommonHttpMethods")]
        public void GetHttpMethod_RetunsNonStaticResult(string method)
        {
            Assert.Equal(method, HttpMethodHelper.GetHttpMethod(method).ToString());
        }
    }
}
