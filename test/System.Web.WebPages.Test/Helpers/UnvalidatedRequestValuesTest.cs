// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.WebPages.Test.Helpers
{
    public class UnvalidatedRequestValuesTest
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            NameValueCollection expectedForm = new NameValueCollection();
            NameValueCollection expectedQueryString = new NameValueCollection();

            Mock<System.Web.UnvalidatedRequestValuesBase> mockUnvalidatedRequestValue = new Mock<System.Web.UnvalidatedRequestValuesBase>();
            mockUnvalidatedRequestValue.SetupGet(u => u.Form).Returns(expectedForm);
            mockUnvalidatedRequestValue.SetupGet(u => u.QueryString).Returns(expectedQueryString);

            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.SetupGet(r => r.Unvalidated).Returns(mockUnvalidatedRequestValue.Object);

            // Act
#pragma warning disable 0618 // Obsolete System.Web.Helpers.UnvalidatedRequestValues
            System.Web.Helpers.UnvalidatedRequestValues unvalidatedValues = new System.Web.Helpers.UnvalidatedRequestValues(mockRequest.Object);
#pragma warning restore

            // Assert
            Assert.Same(expectedForm, unvalidatedValues.Form);
            Assert.Same(expectedQueryString, unvalidatedValues.QueryString);
        }

        [Fact]
        public void Indexer_LooksUpValuesInCorrectOrder()
        {
            // Order should be QueryString, Form, Cookies, ServerVariables

            // Arrange
            NameValueCollection queryString = new NameValueCollection()
            {
                { "foo", "fooQueryString" }
            };

            NameValueCollection form = new NameValueCollection()
            {
                { "foo", "fooInForm" },
                { "bar", "barInForm" },
            };

            HttpCookieCollection cookies = new HttpCookieCollection()
            {
                new HttpCookie("foo", "fooCookie"),
                new HttpCookie("bar", "barCookie"),
                new HttpCookie("baz", "bazCookie")
            };

            NameValueCollection serverVars = new NameValueCollection()
            {
                { "foo", "fooServerVars" },
                { "bar", "barServerVars" },
                { "baz", "bazServerVars" },
                { "quux", "quuxServerVars" },
            };

            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.SetupGet(r => r.ServerVariables).Returns(serverVars);
            mockRequest.SetupGet(r => r.Form).Returns(form);
            mockRequest.SetupGet(r => r.QueryString).Returns(queryString);
            mockRequest.SetupGet(r => r.Cookies).Returns(cookies);

            TestUnvalidatedRequestValues testUnvalidatedRequestValue = new TestUnvalidatedRequestValues(mockRequest.Object);

            mockRequest.SetupGet(r => r.Unvalidated).Returns(testUnvalidatedRequestValue);

#pragma warning disable 0618 // Obsolete System.Web.Helpers.UnvalidatedRequestValues
            System.Web.Helpers.UnvalidatedRequestValues unvalidatedValues = new System.Web.Helpers.UnvalidatedRequestValues(mockRequest.Object);
#pragma warning restore

            // Act
            string fooValue = unvalidatedValues["foo"];
            string barValue = unvalidatedValues["bar"];
            string bazValue = unvalidatedValues["baz"];
            string quuxValue = unvalidatedValues["quux"];
            string notFoundValue = unvalidatedValues["not-found"];

            // Assert
            Assert.Equal("fooQueryString", fooValue);
            Assert.Equal("barInForm", barValue);
            Assert.Equal("bazCookie", bazValue);
            Assert.Equal("quuxServerVars", quuxValue);
            Assert.Null(notFoundValue);
        }

        private sealed class TestUnvalidatedRequestValues : UnvalidatedRequestValuesBase
        {
            HttpRequestBase _request;
            NameValueCollection _queryString;
            NameValueCollection _form;

            public TestUnvalidatedRequestValues(HttpRequestBase request)
            {
                _request = request;
            }

            public override HttpCookieCollection Cookies
            {
                get
                {
                    // HttpCookieCollection copy constructor is not public, so just return it from the request.
                    return _request.Cookies;
                }
            }

            public override NameValueCollection QueryString
            {
                get
                {
                    if (_queryString == null)
                    {
                        _queryString = new NameValueCollection(_request.QueryString);
                    }

                    return _queryString;
                }
            }

            public override NameValueCollection Form
            {
                get
                {
                    if (_form == null)
                    {
                        _form = new NameValueCollection(_request.Form);
                    }

                    return _form;
                }
            }

            public override string this[string key]
            {
                // this item getter follows the same logic as UnvalidatedRequestValues.get_Item
                get
                {
                    string queryStringValue = QueryString[key];
                    if (queryStringValue != null)
                    {
                        return queryStringValue;
                    }

                    string formValue = Form[key];
                    if (formValue != null)
                    {
                        return formValue;
                    }

                    HttpCookie cookie = Cookies[key];
                    if (cookie != null)
                    {
                        return cookie.Value;
                    }

                    string serverVarValue = _request.ServerVariables[key];
                    if (serverVarValue != null)
                    {
                        return serverVarValue;
                    }

                    return null;
                }
            }
        }
    }
}