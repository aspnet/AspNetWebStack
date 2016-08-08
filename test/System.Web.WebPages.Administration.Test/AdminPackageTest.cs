// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Security;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Administration.Test
{
    public class AdminPackageTest
    {
        [Fact]
        public void GetAdminVirtualPathThrowsIfPathIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => SiteAdmin.GetVirtualPath(null), "virtualPath");
        }

        [Fact]
        public void GetAdminVirtualPathDoesNotAppendAdminVirtualPathIfPathStartsWithAdminVirtualPath()
        {
            // Act
            string vpath = SiteAdmin.GetVirtualPath("~/_Admin/Foo");

            // Assert
            Assert.Equal("~/_Admin/Foo", vpath);
        }

        [Fact]
        public void GetAdminVirtualPathAppendsAdminVirtualPath()
        {
            // Act
            string vpath = SiteAdmin.GetVirtualPath("~/Foo");

            // Assert
            Assert.Equal("~/_Admin/Foo", vpath);
        }

        [Fact]
        public void SetAuthCookieAddsAuthCookieToResponseCollection()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponseBase>();
            var cookies = new HttpCookieCollection();
            mockResponse.Setup(m => m.Cookies).Returns(cookies);

            // Act
            AdminSecurity.SetAuthCookie(mockResponse.Object);

            // Assert
            Assert.NotNull(cookies[".ASPXADMINAUTH"]);
        }

        [Fact]
        public void GetAuthAdminCookieCreatesAnAuthTicketWithUserDataSetToAdmin()
        {
            // Arrange
            var cookie = AdminSecurity.GetAuthCookie();

            // Act
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

            // Assert
            Assert.Equal(".ASPXADMINAUTH", cookie.Name);
            Assert.True(cookie.HttpOnly);
            Assert.Equal(2, ticket.Version);
            Assert.Equal("ADMIN", ticket.UserData);
        }

        [Fact]
        public void IsAuthenticatedReturnsFalseIfAuthCookieNotInCollection()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequestBase>();
            var cookies = new HttpCookieCollection();
            mockRequest.Setup(m => m.Cookies).Returns(cookies);

            // Act
            bool authorized = AdminSecurity.IsAuthenticated(mockRequest.Object);

            // Assert
            Assert.False(authorized);
        }

        [Fact]
        public void IsAuthenticatedReturnsFalseIfAuthCookieInCollectionAndIsNotAValidAdminAuthCookie()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequestBase>();
            var cookies = new HttpCookieCollection();
            mockRequest.Setup(m => m.Cookies).Returns(cookies);
            cookies.Add(new HttpCookie(".ASPXADMINAUTH", "test"));

            // Act
            bool authorized = AdminSecurity.IsAuthenticated(mockRequest.Object);

            // Assert
            Assert.False(authorized);
        }

        [Fact]
        public void IsAuthenticatedReturnsTrueIfAuthCookieIsValid()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequestBase>();
            var cookies = new HttpCookieCollection();
            mockRequest.Setup(m => m.Cookies).Returns(cookies);
            cookies.Add(AdminSecurity.GetAuthCookie());

            // Act
            bool authorized = AdminSecurity.IsAuthenticated(mockRequest.Object);

            // Assert
            Assert.True(authorized);
        }

        [Fact]
        public void GetRedirectUrlAppendsAppRelativePathAsReturnUrl()
        {
            // Arrange            
            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(m => m.RawUrl).Returns("~/_Admin/foo/bar/baz");
            mockRequest.Setup(m => m.QueryString).Returns(new NameValueCollection());

            // Act
            string redirectUrl = SiteAdmin.GetRedirectUrl(mockRequest.Object, "register", MakeAppRelative);

            // Assert
            Assert.Equal("~/_Admin/register?ReturnUrl=%7e%2f_Admin%2ffoo%2fbar%2fbaz", redirectUrl);
        }

        [Fact]
        public void GetRedirectUrlDoesNotAppendsAppRelativePathAsReturnUrlIfAlreadyExists()
        {
            // Arrange            
            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(m => m.RawUrl).Returns("~/_Admin/foo/bar/baz?ReturnUrl=~/foo");
            var queryString = new NameValueCollection();
            queryString["ReturnUrl"] = "~/foo";
            mockRequest.Setup(m => m.QueryString).Returns(queryString);

            // Act
            string redirectUrl = SiteAdmin.GetRedirectUrl(mockRequest.Object, "register", MakeAppRelative);

            // Assert
            Assert.Equal("~/_Admin/register?ReturnUrl=%7e%2ffoo", redirectUrl);
        }

        [Fact]
        public void GetReturnUrlReturnsNullIfNotSet()
        {
            // Arrange            
            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(m => m.QueryString).Returns(new NameValueCollection());

            // Act
            string returlUrl = SiteAdmin.GetReturnUrl(mockRequest.Object);

            // Assert
            Assert.Null(returlUrl);
        }

        [Fact]
        public void GetReturnUrlThrowsIfReturnUrlIsNotAppRelative()
        {
            // Arrange            
            var mockRequest = new Mock<HttpRequestBase>();
            var queryString = new NameValueCollection();
            queryString["ReturnUrl"] = "http://www.bing.com";
            mockRequest.Setup(m => m.QueryString).Returns(queryString);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => SiteAdmin.GetReturnUrl(mockRequest.Object), "The return URL specified for request redirection is invalid.");
        }

        [Fact]
        public void GetReturnUrlReturnsReturlUrlQueryStringParameterIfItIsAppRelative()
        {
            // Arrange            
            var mockRequest = new Mock<HttpRequestBase>();
            var queryString = new NameValueCollection();
            queryString["ReturnUrl"] = "~/_Admin/bar?foo=1";
            mockRequest.Setup(m => m.QueryString).Returns(queryString);

            // Act
            string returnUrl = SiteAdmin.GetReturnUrl(mockRequest.Object);

            // Assert
            Assert.Equal("~/_Admin/bar?foo=1", returnUrl);
        }

        [Fact]
        public void SaveAdminPasswordUsesCryptoToWritePasswordAndSalt()
        {
            // Arrange
            var password = "some-random-password";
            MemoryStream ms = new MemoryStream();

            // Act
            bool passwordSaved = AdminSecurity.SaveTemporaryPassword(password, () => ms);

            // Assert
            Assert.True(passwordSaved);
            string savedPassword = Encoding.Default.GetString(ms.ToArray());
            // Trim everything after the new line. Cannot use the properties from the stream since it is already closed by the writer.
            savedPassword = savedPassword.Substring(0, savedPassword.IndexOf(Environment.NewLine));

            Assert.True(Crypto.VerifyHashedPassword(savedPassword, password));
        }

        [Fact]
        public void SaveAdminPasswordReturnsFalseIfGettingStreamThrowsUnauthorizedAccessException()
        {
            // Act
            bool passwordSaved = AdminSecurity.SaveTemporaryPassword("password", () => { throw new UnauthorizedAccessException(); });

            // Assert
            Assert.False(passwordSaved);
        }

        [Fact]
        public void CheckPasswordReturnsTrueIfPasswordIsValid()
        {
            // Arrange
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.WriteLine(Crypto.HashPassword("password"));
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            // Act
            bool passwordIsValid = AdminSecurity.CheckPassword("password", () => ms);

            // Assert
            Assert.True(passwordIsValid);

            writer.Close();
        }

        [Fact]
        public void HasAdminPasswordReturnsTrueIfAdminPasswordFileExists()
        {
            // Arrange
            Mock<VirtualPathProvider> mockVpp = new Mock<VirtualPathProvider>();
            mockVpp.Setup(m => m.FileExists("~/App_Data/Admin/Password.config")).Returns(true);

            // Act
            bool hasPassword = AdminSecurity.HasAdminPassword(mockVpp.Object);

            // Assert
            Assert.True(hasPassword);
        }

        [Fact]
        public void HasAdminPasswordReturnsFalseIfAdminPasswordFileDoesNotExists()
        {
            // Arrange
            Mock<VirtualPathProvider> mockVpp = new Mock<VirtualPathProvider>();
            mockVpp.Setup(m => m.FileExists("~/App_Data/Admin/Password.config")).Returns(false);

            // Act
            bool hasPassword = AdminSecurity.HasAdminPassword(mockVpp.Object);

            // Assert
            Assert.False(hasPassword);
        }

        [Fact]
        public void HasTemporaryPasswordReturnsTrueIfAdminPasswordFileExists()
        {
            // Arrange
            Mock<VirtualPathProvider> mockVpp = new Mock<VirtualPathProvider>();
            mockVpp.Setup(m => m.FileExists("~/App_Data/Admin/_Password.config")).Returns(true);

            // Act
            bool hasPassword = AdminSecurity.HasTemporaryPassword(mockVpp.Object);

            // Assert
            Assert.True(hasPassword);
        }

        [Fact]
        public void HasTemporaryPasswordReturnsFalseIfAdminPasswordFileDoesNotExists()
        {
            // Arrange
            Mock<VirtualPathProvider> mockVpp = new Mock<VirtualPathProvider>();
            mockVpp.Setup(m => m.FileExists("~/App_Data/Admin/_Password.config")).Returns(false);

            // Act
            bool hasPassword = AdminSecurity.HasTemporaryPassword(mockVpp.Object);

            // Assert
            Assert.False(hasPassword);
        }

        [Fact]
        public void NoPasswordOrTemporaryPasswordRedirectsToRegisterPage()
        {
            AssertSecure(requestUrl: "~/",
                         passwordExists: false,
                         temporaryPasswordExists: false,
                         expectedUrl: "~/_Admin/Register.cshtml?ReturnUrl=%7e%2f");
        }

        [Fact]
        public void IfPasswordExistsRedirectsToLoginPage()
        {
            AssertSecure(requestUrl: "~/",
                         passwordExists: true,
                         temporaryPasswordExists: false,
                         expectedUrl: "~/_Admin/Login.cshtml?ReturnUrl=%7e%2f");
        }

        [Fact]
        public void IfPasswordExistsRedirectsToLoginPageEvenIfTemporaryPasswordFileExists()
        {
            AssertSecure(requestUrl: "~/",
                         passwordExists: true,
                         temporaryPasswordExists: true,
                         expectedUrl: "~/_Admin/Login.cshtml?ReturnUrl=%7e%2f");
        }

        [Fact]
        public void IfTemporaryPasswordExistsRedirectsToInstructionsPage()
        {
            AssertSecure(requestUrl: "~/",
                         passwordExists: false,
                         temporaryPasswordExists: true,
                         expectedUrl: "~/_Admin/EnableInstructions.cshtml?ReturnUrl=%7e%2f");
        }

        [Fact]
        public void NoRedirectIfAlreadyGoingToRedirectPage()
        {
            AssertSecure(requestUrl: "~/_Admin/Register.cshtml",
                         passwordExists: false,
                         temporaryPasswordExists: false,
                         expectedUrl: null);

            AssertSecure(requestUrl: "~/_Admin/Login.cshtml",
                         passwordExists: true,
                         temporaryPasswordExists: false,
                         expectedUrl: null);

            AssertSecure(requestUrl: "~/_Admin/EnableInstructions.cshtml",
                         passwordExists: false,
                         temporaryPasswordExists: true,
                         expectedUrl: null);
        }

        private static void AssertSecure(string requestUrl, bool passwordExists, bool temporaryPasswordExists, string expectedUrl)
        {
            // Arrange
            var vpp = new Mock<VirtualPathProvider>();
            if (temporaryPasswordExists)
            {
                vpp.Setup(m => m.FileExists("~/App_Data/Admin/_Password.config")).Returns(true);
            }

            if (passwordExists)
            {
                vpp.Setup(m => m.FileExists("~/App_Data/Admin/Password.config")).Returns(true);
            }

            string redirectUrl = null;
            var response = new Mock<HttpResponseBase>();
            response.Setup(m => m.Redirect(It.IsAny<string>())).Callback<string>(url => redirectUrl = url);
            var request = new Mock<HttpRequestBase>();
            request.Setup(m => m.QueryString).Returns(new NameValueCollection());
            request.Setup(m => m.RawUrl).Returns(requestUrl);
            var cookies = new HttpCookieCollection();
            request.Setup(m => m.Cookies).Returns(cookies);
            var context = new Mock<HttpContextBase>();
            context.Setup(m => m.Request).Returns(request.Object);
            context.Setup(m => m.Response).Returns(response.Object);
            var startPage = new Mock<StartPage>() { CallBase = true };
            var page = new Mock<WebPageRenderingBase>();
            page.Setup(m => m.VirtualPath).Returns(requestUrl);
            startPage.Object.ChildPage = page.Object;
            page.Setup(m => m.Context).Returns(context.Object);

            // Act
            AdminSecurity.Authorize(startPage.Object, vpp.Object, MakeAppRelative);

            // Assert
            Assert.Equal(expectedUrl, redirectUrl);
        }

        private static string MakeAppRelative(string path)
        {
            return path;
        }
    }
}
