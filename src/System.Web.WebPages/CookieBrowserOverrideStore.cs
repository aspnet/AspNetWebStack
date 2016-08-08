// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// The default BrowserOverrideStore. Gets overridden user agent for a request from a cookie.
    /// Creates a cookie to set the overridden user agent.
    /// </summary>
    public class CookieBrowserOverrideStore : BrowserOverrideStore
    {
        internal static readonly string BrowserOverrideCookieName = ".ASPXBrowserOverride";
        private readonly int _daysToExpire;

        /// <summary>
        /// Creates the BrowserOverrideStore setting any browser override cookie to expire in 7 days.
        /// </summary>
        public CookieBrowserOverrideStore()
            : this(daysToExpire: 7)
        {
        }

        /// <summary>
        /// Constructor to control the expiration of the browser override cookie.
        /// </summary>
        public CookieBrowserOverrideStore(int daysToExpire)
        {
            _daysToExpire = daysToExpire;
        }

        /// <summary>
        /// Looks for a user agent by searching for the browser override cookie. If no cookie is found
        /// returns null.
        /// </summary>
        public override string GetOverriddenUserAgent(HttpContextBase httpContext)
        {
            // Check the response to see if the cookie has been set somewhere in the current request.
            HttpCookieCollection responseCookies = httpContext.Response.Cookies;
            // NOTE: Only look for the key (via AllKeys) so a new cookie is not automatically created.
            string[] cookieNames = responseCookies.AllKeys;
            // NOTE: use a simple for loop since it performs an order of magnitude faster than .Contains()
            // and this is a hot path that gets executed for every request.
            for (int i = 0; i < cookieNames.Length; i++)
            {
                // HttpCookieCollection uses OrdinalIgnoreCase comparison for its keys
                if (String.Equals(cookieNames[i], BrowserOverrideCookieName, StringComparison.OrdinalIgnoreCase))
                {
                    HttpCookie currentOverriddenBrowserCookie = responseCookies[BrowserOverrideCookieName];

                    if (currentOverriddenBrowserCookie.Value != null)
                    {
                        return currentOverriddenBrowserCookie.Value;
                    }
                    else
                    {
                        // The cookie has no value. It was cleared on the current request so return null.
                        return null;
                    }
                }
            }

            // If there was no cookie found in the response check the request.
            var requestOverrideCookie = httpContext.Request.Cookies[BrowserOverrideCookieName];
            if (requestOverrideCookie != null)
            {
                return requestOverrideCookie.Value;
            }

            return null;
        }

        /// <summary>
        /// Adds a browser override cookie with the set user agent to the response of the current request.
        /// If the user agent is null the browser override cookie is set to expire, otherwise its expiration is set
        /// to daysToExpire, specified when CookieBasedOverrideStore is created.
        /// </summary>
        public override void SetOverriddenUserAgent(HttpContextBase httpContext, string userAgent)
        {
            HttpCookie browserOverrideCookie = new HttpCookie(BrowserOverrideCookieName, HttpUtility.UrlEncode(userAgent));

            if (userAgent == null)
            {
                browserOverrideCookie.Expires = DateTime.Now.AddDays(-1);
            }
            else
            {
                // Only set expiration if the cookie should live longer than the current session
                if (_daysToExpire > 0)
                {
                    browserOverrideCookie.Expires = DateTime.Now.AddDays(_daysToExpire);
                }
            }

            httpContext.Response.Cookies.Remove(BrowserOverrideCookieName);
            httpContext.Response.Cookies.Add(browserOverrideCookie);
        }
    }
}
