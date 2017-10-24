// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.WebPages.Administration.PackageManager;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Administration.Test
{
    public class PageUtilsTest
    {
        [Fact]
        public void GetFilterValueReturnsNullIfValueWasNotFound()
        {
            // Arrange
            var request = new Mock<HttpRequestBase>();
            request.Setup(c => c.QueryString).Returns(new NameValueCollection());
            request.Setup(c => c.Cookies).Returns(new HttpCookieCollection());

            // Act
            var value = PageUtils.GetFilterValue(request.Object, "foo", "my-key");

            // Assert
            Assert.Null(value);
        }

        [Fact]
        public void GetFilterValueReturnsValueFromCookieIfQueryStringDoesNotContainKey()
        {
            // Arrange
            const string key = "my-key";
            const string value = "my-cookie-value";
            var request = new Mock<HttpRequestBase>();
            request.Setup(c => c.QueryString).Returns(new NameValueCollection());
            var cookies = new HttpCookieCollection();
            var cookie = new HttpCookie("foo");
            cookie[key] = value;
            cookies.Add(cookie);
            request.Setup(c => c.Cookies).Returns(cookies);

            // Act
            var returnedValue = PageUtils.GetFilterValue(request.Object, "foo", key);

            // Assert
            Assert.Equal(value, returnedValue);
        }

        [Fact]
        public void GetFilterValueReturnsValueFromQueryString()
        {
            // Arrange
            const string key = "my-key";
            const string requestValue = "my-request-value";
            const string cookieValue = "my-cookie-value";
            var request = new Mock<HttpRequestBase>();
            var queryString = new NameValueCollection();
            queryString[key] = requestValue;
            request.Setup(c => c.QueryString).Returns(queryString);
            var cookies = new HttpCookieCollection();
            var cookie = new HttpCookie("foo");
            cookie[key] = cookieValue;
            request.Setup(c => c.Cookies).Returns(cookies);

            // Act
            var returnedValue = PageUtils.GetFilterValue(request.Object, "foo", key);

            // Assert
            Assert.Equal(requestValue, returnedValue);
        }

        [Fact]
        public void PersistFilterCreatesCookieIfItDoesNotExist()
        {
            // Arrange
            var cookies = new HttpCookieCollection();
            var response = new Mock<HttpResponseBase>();
            response.Setup(c => c.Cookies).Returns(cookies);

            // Act
            PageUtils.PersistFilter(response.Object, "my-cookie", new Dictionary<string, string>());

            // Assert
            Assert.NotNull(cookies["my-cookie"]);
        }

        [Fact]
        public void PersistFilterUsesExistingCookie()
        {
            // Arrange
            var cookieName = "my-cookie";
            var cookies = new HttpCookieCollection();
            cookies.Add(new HttpCookie(cookieName));
            var response = new Mock<HttpResponseBase>();
            response.Setup(c => c.Cookies).Returns(cookies);

            // Act
            PageUtils.PersistFilter(response.Object, "my-cookie", new Dictionary<string, string>());

            // Assert
            Assert.Single(cookies);
        }

        [Fact]
        public void PersistFilterAddsDictionaryEntriesToCookie()
        {
            // Arrange
            var cookies = new HttpCookieCollection();
            var response = new Mock<HttpResponseBase>();
            response.Setup(c => c.Cookies).Returns(cookies);

            // Act
            PageUtils.PersistFilter(response.Object, "my-cookie", new Dictionary<string, string>() { { "a", "b" }, { "x", "y" } });

            // Assert
            var cookie = cookies["my-cookie"];
            Assert.Equal("b", cookie["a"]);
            Assert.Equal("y", cookie["x"]);
        }

        [Fact]
        public void IsValidLicenseUrlReturnsTrueForHttpUris()
        {
            // Arrange
            var uri = new Uri("http://www.microsoft.com");

            // Act and Assert
            Assert.True(PageUtils.IsValidLicenseUrl(uri));
        }

        [Fact]
        public void IsValidLicenseUrlReturnsTrueForHttpsUris()
        {
            // Arrange
            var uri = new Uri("HTTPs://www.asp.net");

            // Act and Assert
            Assert.True(PageUtils.IsValidLicenseUrl(uri));
        }

        [Fact]
        public void IsValidLicenseUrlReturnsFalseForNonHttpUris()
        {
            // Arrange
            var jsUri = new Uri("javascript:alert('Hello world');");
            var fileShareUri = new Uri(@"c:\windows\system32\notepad.exe");
            var mailToUti = new Uri("mailto:invalid-email@microsoft.com");

            // Act and Assert
            Assert.False(PageUtils.IsValidLicenseUrl(jsUri));
            Assert.False(PageUtils.IsValidLicenseUrl(fileShareUri));
            Assert.False(PageUtils.IsValidLicenseUrl(mailToUti));
        }
    }
}
