// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Configuration;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class BrowserHelpersTest
    {
        [Fact]
        public void GetOverriddenUserAgentGetsUserAgentFromHttpContext()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            string testUserAgent = "testUserAgent";

            // Act
            context.Object.SetOverriddenBrowser(testUserAgent);
            Assert.Equal(testUserAgent, context.Object.GetOverriddenUserAgent());
            context.Object.Response.Cookies.Clear();
            context.Object.Request.Cookies.Clear();

            // Assert
            Assert.Equal(testUserAgent, context.Object.GetOverriddenUserAgent());
        }

        [Fact]
        public void GetOverriddenUserAgentFallsBackToStoreUserAgent()
        {
            // Arrange
            string testUserAgent = "testUserAgent";
            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, testUserAgent);
            HttpContextBase context = CookieBrowserOverrideStoreTest.CreateCookieContext(requestCookie: existingOverrideCookie).Object;

            // Act & Assert
            Assert.Equal(testUserAgent, context.GetOverriddenUserAgent());
        }

        [Fact]
        public void GetOverriddenUserAgentDefaultsToRequestUserAgent()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.UserAgent).Returns("requestUserAgent");

            // Act & Assert
            Assert.Equal("requestUserAgent", context.Object.GetOverriddenUserAgent());
        }

        [Fact]
        public void SetOverriddenBrowserWithBrowserOverrideSetBrowserMobile()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.Browser.IsMobileDevice).Returns(false);

            // Act
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);

            // Assert
            Assert.True(context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory).IsMobileDevice);
        }

        [Fact]
        public void SetOverriddenBrowserWithUnsupportedBrowserOverrideClearsBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> requestBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            context.Setup(c => c.Request.Browser).Returns(requestBrowser.Object);

            // Act & Assert
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            Assert.NotSame(requestBrowser.Object, context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory));

            context.Object.SetOverriddenBrowser((BrowserOverride)(-1));
            Assert.Same(requestBrowser.Object, context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory));
        }

        [Fact]
        public void SetOverriddenBrowserWithBrowserOverrideSetBrowserDesktop()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.Browser.IsMobileDevice).Returns(true);

            // Act
            context.Object.SetOverriddenBrowser(BrowserOverride.Desktop);

            // Assert
            Assert.False(context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory).IsMobileDevice);
        }

        [Fact]
        public void SetOverriddenBrowserWithStringOverrideSetBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.Browser.IsMobileDevice).Returns(false);

            // Act
            context.Object.SetOverriddenBrowser("Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16");

            // Assert
            Assert.True(context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory).IsMobileDevice);
        }

        [Fact]
        public void SetOverriddenBrowserClearsCachedBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.UserAgent).Returns("testUserAgent");

            // Act
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory);

            // If the browser is generated this will throw an exception because we are going through the provider.
            // We must be getting the cached overridden browser.
            context.Object.GetOverriddenBrowser();
            context.Object.SetOverriddenBrowser("testUserAgent");

            // Assert

            // The browser has been cleared from HttpContext and the user agent has been set to the original user agent.
            // Otherwise we will either get the cached browser or an exception when trying to generate the browser from the
            // mobile user agent.
            Assert.Null(context.Object.GetOverriddenBrowser());
        }

        [Fact]
        public void SetOverridenBrowserInSameOverrideClassClearsOverridenBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            context.Setup(c => c.Request.Browser.IsMobileDevice).Returns(true);
            context.Setup(c => c.Request.UserAgent).Returns("sampleUserAgent");

            // Act
            context.Object.SetOverriddenBrowser(BrowserOverride.Desktop);
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);

            // Assert
            Assert.Equal("sampleUserAgent", context.Object.GetOverriddenUserAgent());
        }

        [Fact]
        public void GetOverriddenBrowserGetsBrowserFromHttpContext()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();

            // Act
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory);

            // Assert

            // If the browser is generated this will throw an exception because we are going through the provider.
            // We must be getting the cached overridden browser.
            Assert.True(context.Object.GetOverriddenBrowser().IsMobileDevice);
        }

        [Fact]
        public void GetOverriddenBrowserWithStoredBrowserAndNoBrowserInContext()
        {
            // Arrange
            string mobileUserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16";
            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, mobileUserAgent);
            HttpContextBase context = CookieBrowserOverrideStoreTest.CreateCookieContext(requestCookie: existingOverrideCookie).Object;

            // Act & Assert
            Assert.True(context.GetOverriddenBrowser(CreateBrowserThroughFactory).IsMobileDevice);
        }

        [Fact]
        public void GetOverriddenBrowserDefaultsToRequestBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> currentBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            context.Setup(c => c.Request.Browser).Returns(currentBrowser.Object);

            // Act & Assert
            Assert.Same(currentBrowser.Object, context.Object.GetOverriddenBrowser());
        }

        [Fact]
        public void ClearOverriddenBrowserClearsSetBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> requestBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            context.Setup(c => c.Request.Browser).Returns(requestBrowser.Object);

            // Act & Assert
            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            Assert.NotSame(requestBrowser.Object, context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory));

            context.Object.ClearOverriddenBrowser();
            Assert.Same(requestBrowser.Object, context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory));
        }

        [Fact]
        public void ClearOverriddenBrowserWithNoSetBrowser()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> requestBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            context.Setup(c => c.Request.Browser).Returns(requestBrowser.Object);

            // Act & Assert
            context.Object.ClearOverriddenBrowser();
            Assert.Same(requestBrowser.Object, context.Object.GetOverriddenBrowser(CreateBrowserThroughFactory));
        }

        [Fact]
        public void GetOverriddenBrowserVariesBySetOverriddenBrowserMobile()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> currentBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            currentBrowser.Setup(c => c.IsMobileDevice).Returns(true);
            context.Setup(c => c.Request.Browser).Returns(currentBrowser.Object);

            // Act
            string originalBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            context.Object.SetOverriddenBrowser(BrowserOverride.Desktop);
            string deskTopBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            string mobileBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            // Assert
            Assert.Equal(originalBrowserType, mobileBrowserType);
            Assert.NotEqual(originalBrowserType, deskTopBrowserType);
            Assert.NotEqual(mobileBrowserType, deskTopBrowserType);
        }

        [Fact]
        public void GetOverriddenBrowserVariesBySetOverriddenBrowserDesktop()
        {
            // Arrange
            Mock<HttpContextBase> context = CookieBrowserOverrideStoreTest.CreateCookieContext();
            Mock<HttpBrowserCapabilitiesBase> currentBrowser = new Mock<HttpBrowserCapabilitiesBase>();
            currentBrowser.Setup(c => c.IsMobileDevice).Returns(false);
            context.Setup(c => c.Request.Browser).Returns(currentBrowser.Object);

            // Act
            string originalBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            context.Object.SetOverriddenBrowser(BrowserOverride.Mobile);
            string mobileBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            context.Object.SetOverriddenBrowser(BrowserOverride.Desktop);
            string deskTopBrowserType = GetOverriddenBrowserType(context.Object, CreateBrowserThroughFactory);

            // Assert
            Assert.NotEqual(originalBrowserType, mobileBrowserType);
            Assert.Equal(originalBrowserType, deskTopBrowserType);
            Assert.NotEqual(mobileBrowserType, deskTopBrowserType);
        }

        [Fact]
        public void GetOverriddenBrowser_OverridenUserAgentMatchingIsCaseInsensitive()
        {
            // Arrange
            string mobileUserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16";
            string mobileUserAgentLowerCase = mobileUserAgent.ToLowerInvariant();

            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, mobileUserAgent);
            HttpContextBase context = CookieBrowserOverrideStoreTest.CreateCookieContext(requestCookie: existingOverrideCookie).Object;
            Mock.Get(context).Setup(c => c.Request.UserAgent).Returns(mobileUserAgent);
            Mock.Get(context).Setup(c => c.Request.Browser).Returns(new HttpBrowserCapabilitiesWrapper(new HttpBrowserCapabilities()));

            // this will set the overriden user agent
            context.SetOverriddenBrowser(mobileUserAgentLowerCase);

            // overriden browser is only created if the overriden userAgent is different from the request's user agent
            bool overridenBrowserWasCreated = false;

            // Act
            var overriden = context.GetOverriddenBrowser(x => { overridenBrowserWasCreated = true; return null; });

            // Assert
            Assert.False(overridenBrowserWasCreated);
            Assert.Same(context.Request.Browser, overriden);
        }

        // We need to call the .ctor of SimpleWorkerRequest that depends on HttpRuntime so for unit testing
        // simply create the browser capabilities by going directly through the factory.
        private static HttpBrowserCapabilitiesBase CreateBrowserThroughFactory(string userAgent)
        {
            HttpBrowserCapabilities browser = new HttpBrowserCapabilities
            {
                Capabilities = new Dictionary<string, string>
                {
                    { String.Empty, userAgent }
                }
            };

            BrowserCapabilitiesFactory factory = new BrowserCapabilitiesFactory();
            factory.ConfigureBrowserCapabilities(new NameValueCollection(), browser);

            return new HttpBrowserCapabilitiesWrapper(browser);
        }

        private static string GetOverriddenBrowserType(HttpContextBase context, Func<string, HttpBrowserCapabilitiesBase> factory)
        {
            return context.GetOverriddenBrowser(factory).Type;
        }
    }
}
