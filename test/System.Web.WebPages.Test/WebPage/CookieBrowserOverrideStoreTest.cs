// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class CookieBrowserOverrideStoreTest
    {
        [Fact]
        public void GetOverriddenUserAgentReturnsNullIfNoResponseOrRequestCookieIsSet()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();

            // Act & Assert
            Assert.Null(store.GetOverriddenUserAgent(CreateCookieContext().Object));
        }

        [Fact]
        public void GetOverriddenUserAgentReturnsUserAgentFromRequestCookieIfNoResponseCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, "existingRequestAgent");
            HttpContextBase context = CreateCookieContext(requestCookie: existingOverrideCookie).Object;

            // Act & Assert
            Assert.Equal("existingRequestAgent", store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserAgentWithNoExistingCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpContextBase context = CreateCookieContext().Object;

            // Act
            store.SetOverriddenUserAgent(context, "setUserAgent");

            // Assert
            Assert.Equal("setUserAgent", store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserWithExistingRequestCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, "existingRequestAgent");
            HttpContextBase context = CreateCookieContext(requestCookie: existingOverrideCookie).Object;

            // Act
            store.SetOverriddenUserAgent(context, "setUserAgent");

            // Assert
            Assert.Equal("setUserAgent", store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserWithExistingResponseCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpContextBase context = CreateCookieContext().Object;

            // Act & Assert
            store.SetOverriddenUserAgent(context, "testUserAgent");
            Assert.Equal("testUserAgent", store.GetOverriddenUserAgent(context));

            store.SetOverriddenUserAgent(context, "subsequentTestUserAgent");
            Assert.Equal("subsequentTestUserAgent", store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserAgentNullWithRequestCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpCookie existingOverrideCookie = new HttpCookie(CookieBrowserOverrideStore.BrowserOverrideCookieName, "setUserAgent");
            HttpContextBase context = CreateCookieContext(requestCookie: existingOverrideCookie).Object;

            // Act
            store.SetOverriddenUserAgent(context, null);

            // Assert
            Assert.Null(store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserAgentNullWithNoExistingCookie()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            HttpContextBase context = CreateCookieContext().Object;

            // Act
            store.SetOverriddenUserAgent(context, null);

            // Assert
            Assert.Null(store.GetOverriddenUserAgent(context));
        }

        [Fact]
        public void SetOverriddenUserAgentSetsExpiration()
        {
            // Arrange
            CookieBrowserOverrideStore store = new CookieBrowserOverrideStore();
            CookieBrowserOverrideStore sessionStore = new CookieBrowserOverrideStore(daysToExpire: 0);
            CookieBrowserOverrideStore longTermStore = new CookieBrowserOverrideStore(daysToExpire: 100);
            CookieBrowserOverrideStore negativeTermStore = new CookieBrowserOverrideStore(daysToExpire: -1);

            HttpContextBase context = CreateCookieContext().Object;

            // Act & Assert
            store.SetOverriddenUserAgent(context, "testUserAgent");
            Assert.True(DateTime.Now.AddDays(6.5) < context.Response.Cookies[CookieBrowserOverrideStore.BrowserOverrideCookieName].Expires &&
                        context.Response.Cookies[CookieBrowserOverrideStore.BrowserOverrideCookieName].Expires < DateTime.Now.AddDays(7.5));

            sessionStore.SetOverriddenUserAgent(context, "testUserAgent");
            Assert.True(context.Response.Cookies[CookieBrowserOverrideStore.BrowserOverrideCookieName].Expires < DateTime.Now);

            longTermStore.SetOverriddenUserAgent(context, "testUserAgent");
            Assert.True(context.Response.Cookies[CookieBrowserOverrideStore.BrowserOverrideCookieName].Expires > DateTime.Now.AddDays(99));

            negativeTermStore.SetOverriddenUserAgent(context, "testUserAgent");
            Assert.True(context.Response.Cookies[CookieBrowserOverrideStore.BrowserOverrideCookieName].Expires < DateTime.Now);
        }

        internal static Mock<HttpContextBase> CreateCookieContext(HttpCookie requestCookie = null, HttpCookie responseCookie = null)
        {
            Mock<HttpContextBase> context = new Mock<HttpContextBase>();

            HttpCookieCollection requestCookies = new HttpCookieCollection();
            if (requestCookie != null)
            {
                requestCookies.Add(requestCookie);
            }

            HttpCookieCollection responseCookies = new HttpCookieCollection();
            if (responseCookie != null)
            {
                responseCookies.Add(responseCookie);
            }

            context.Setup(c => c.Request.Cookies).Returns(requestCookies);
            context.Setup(c => c.Response.Cookies).Returns(responseCookies);
            context.Setup(c => c.Items).Returns(new Hashtable());

            return context;
        }
    }
}
