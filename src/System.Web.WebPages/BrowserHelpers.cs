// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Hosting;

namespace System.Web.WebPages
{
    /// <summary>
    /// Extension methods used to determine what browser a visitor wants to be seen as using.
    /// </summary>
    public static class BrowserHelpers
    {
        /// <summary>
        /// Stock IE6 user agent string
        /// </summary>
        private const string DesktopUserAgent = "Mozilla/4.0 (compatible; MSIE 6.1; Windows XP)";

        /// <summary>
        /// Stock Windows Mobile 6.0 user agent string
        /// </summary>
        private const string MobileUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 8.12; MSIEMobile 6.0)";

        private static readonly object _browserOverrideKey = new object();
        private static readonly object _userAgentKey = new object();

        /// <summary>
        /// Clears the set browser for the request. After clearing the browser the overridden browser will be the browser for the request.
        /// </summary>
        public static void ClearOverriddenBrowser(this HttpContextBase httpContext)
        {
            SetOverriddenBrowser(httpContext, userAgent: null);
        }

        // Default implementation to generate an HttpBrowserCapabilities object using the current HttpCapabilitiesProvider
        private static HttpBrowserCapabilitiesBase CreateOverriddenBrowser(string userAgent)
        {
            HttpBrowserCapabilities overriddenBrowser = new HttpContext(new UserAgentWorkerRequest(userAgent)).Request.Browser;
            return new HttpBrowserCapabilitiesWrapper(overriddenBrowser);
        }

        /// <summary>
        /// Gets the overridden browser for the request based on the overridden user agent.
        /// If no overridden user agent is set, returns the browser for the request.
        /// </summary>
        public static HttpBrowserCapabilitiesBase GetOverriddenBrowser(this HttpContextBase httpContext)
        {
            return GetOverriddenBrowser(httpContext, createBrowser: null);
        }

        /// <summary>
        /// Internal GetOverriddenBrowser overload to allow the browser creation function to changed. Defaults to CreateOverridenBrowser if createBrowser is null.
        /// </summary>
        internal static HttpBrowserCapabilitiesBase GetOverriddenBrowser(this HttpContextBase httpContext, Func<string, HttpBrowserCapabilitiesBase> createBrowser)
        {
            HttpBrowserCapabilitiesBase overriddenBrowser = (HttpBrowserCapabilitiesBase)httpContext.Items[_browserOverrideKey];

            if (overriddenBrowser == null)
            {
                string overriddenUserAgent = GetOverriddenUserAgent(httpContext);

                if (!String.Equals(overriddenUserAgent, httpContext.Request.UserAgent, StringComparison.OrdinalIgnoreCase))
                {
                    if (createBrowser != null)
                    {
                        overriddenBrowser = createBrowser(overriddenUserAgent);
                    }
                    else
                    {
                        overriddenBrowser = CreateOverriddenBrowser(overriddenUserAgent);
                    }
                }
                else
                {
                    overriddenBrowser = httpContext.Request.Browser;
                }

                httpContext.Items[_browserOverrideKey] = overriddenBrowser;
            }

            return overriddenBrowser;
        }

        /// <summary>
        /// Gets the overridden user agent for the request. If no overridden user agent is set, returns the user agent for the request.
        /// </summary>
        public static string GetOverriddenUserAgent(this HttpContextBase httpContext)
        {
            return (string)httpContext.Items[_userAgentKey] ??
                   BrowserOverrideStores.Current.GetOverriddenUserAgent(httpContext) ??
                   httpContext.Request.UserAgent;
        }

        /// <summary>
        /// Gets a string that varies based upon the type of the browser. Can be used to override
        /// System.Web.HttpApplication.GetVaryByCustomString to differentiate cache keys based on
        /// the overridden browser.
        /// </summary>
        public static string GetVaryByCustomStringForOverriddenBrowser(this HttpContext httpContext)
        {
            return GetVaryByCustomStringForOverriddenBrowser(new HttpContextWrapper(httpContext));
        }

        /// <summary>
        /// Gets a string that varies based upon the type of the browser. Can be used to override
        /// System.Web.HttpApplication.GetVaryByCustomString to differentiate cache keys based on
        /// the overridden browser.
        /// </summary>
        public static string GetVaryByCustomStringForOverriddenBrowser(this HttpContextBase httpContext)
        {
            return GetOverriddenBrowser(httpContext, createBrowser: null).Type;
        }

        /// <summary>
        /// Sets the overridden user agent for the request using a BrowserOverride.
        /// </summary>
        public static void SetOverriddenBrowser(this HttpContextBase httpContext, BrowserOverride browserOverride)
        {
            string userAgent = null;

            switch (browserOverride)
            {
                case BrowserOverride.Desktop:
                    // bug:262389 override only if the request was not made from a browser or the browser is not of a desktop device
                    if (httpContext.Request.Browser == null || httpContext.Request.Browser.IsMobileDevice)
                    {
                        userAgent = DesktopUserAgent;
                    }
                    break;
                case BrowserOverride.Mobile:
                    if (httpContext.Request.Browser == null || !httpContext.Request.Browser.IsMobileDevice)
                    {
                        userAgent = MobileUserAgent;
                    }
                    break;
            }

            if (userAgent != null)
            {
                SetOverriddenBrowser(httpContext, userAgent);
            }
            else
            {
                ClearOverriddenBrowser(httpContext);
            }
        }

        /// <summary>
        /// Sets the overridden user agent for the request using a string
        /// </summary>
        public static void SetOverriddenBrowser(this HttpContextBase httpContext, string userAgent)
        {
            // Set the overridden user agent and clear the overridden browser
            // so that it can be generated from the new overridden user agent.
            httpContext.Items[_userAgentKey] = userAgent;
            httpContext.Items[_browserOverrideKey] = null;

            BrowserOverrideStores.Current.SetOverriddenUserAgent(httpContext, userAgent);
        }

        private sealed class UserAgentWorkerRequest : SimpleWorkerRequest
        {
            private readonly string _userAgent;

            public UserAgentWorkerRequest(string userAgent)
                : base(String.Empty, String.Empty, output: null)
            {
                _userAgent = userAgent;
            }

            public override string GetKnownRequestHeader(int index)
            {
                return index == HeaderUserAgent ? _userAgent : null;
            }
        }
    }
}
