// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.WebPages;
using System.Web.WebPages.Razor;
using WebMatrix.Data;

namespace WebMatrix.WebData
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        // NOTE: Do not add public fields, methods, or other members to this class.
        // This class does not show up in Intellisense so members on it will not be
        // discoverable by users. Place new members on more appropriate classes that
        // relate to the public API (for example, a LoginUrl property should go on a
        // membership-related class).

        private const string LoginUrlKey = "loginUrl";

        private static bool _startWasCalled;

        public static void Start()
        {
            // Even though ASP.NET will only call each PreAppStart once, we sometimes internally call one PreAppStart from 
            // another PreAppStart to ensure that things get initialized in the right order. ASP.NET does not guarantee the 
            // order so we have to guard against multiple calls.
            // All Start calls are made on same thread, so no lock needed here.

            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            // Summary of Simple Membership startup behavior:
            //  1. If the appSetting enabledSimpleMembership is present and equal to "false", NEITHER SimpleMembership NOR AutoFormsAuth are activated
            //  2. If the appSetting is true, a non-boolean string or not present, BOTH may be activated
            //    a. SimpleMembership ONLY replaces the AspNetSqlMemberhipProvider, but it does replace it even if it isn't the default.  This
            //       means that anything accessing this provider by name will get Simple Membership, but if this provider is no longer the default
            //       then SimpleMembership does not affect the default
            //    b. SimpleMembership delegates to the previous default provider UNLESS WebSecurity.InitializeDatabaseConnection is called.

            // Initialize membership provider
            WebSecurity.PreAppStartInit();

            // Initialize Forms Authentication default configuration
            SetUpFormsAuthentication();

            // Wire up WebMatrix.Data's Database object to the ASP.NET Web Pages resource tracker
            Database.ConnectionOpened += OnConnectionOpened;

            // Auto import the WebMatrix.Data and WebMatrix.WebData namespaces to all apps that are executing.
            WebPageRazorHost.AddGlobalImport("WebMatrix.Data");
            WebPageRazorHost.AddGlobalImport("WebMatrix.WebData");
        }

        private static void OnConnectionOpened(object sender, ConnectionEventArgs e)
        {
            // Register all open connections for disposing at the end of the request
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                HttpContextWrapper httpContextWrapper = new HttpContextWrapper(httpContext);
                httpContextWrapper.RegisterForDispose(e.Connection);
            }
        }

        private static void SetUpFormsAuthentication()
        {
            if (ConfigUtil.SimpleMembershipEnabled)
            {
                NameValueCollection configurationData = new NameValueCollection();

                string appSettingsLoginUrl = ConfigurationManager.AppSettings[FormsAuthenticationSettings.LoginUrlKey];
                if (appSettingsLoginUrl != null)
                {
                    // Allow use of <add key="loginUrl" value="~/MyPath/LogOn" /> as a shortcut to specify
                    // a custom log in url
                    configurationData[LoginUrlKey] = appSettingsLoginUrl;
                }
                else if (!ConfigUtil.ShouldPreserveLoginUrl())
                {
                    // Otherwise, use the default login url, but only if PreserveLoginUrl != true
                    // If PreserveLoginUrl == true, we do not want to override FormsAuthentication's default
                    // behavior because trying to evaluate FormsAuthentication.LoginUrl at this point in a
                    // PreAppStart method would cause app-relative URLs to be evaluated incorrectly.
                    configurationData[LoginUrlKey] = FormsAuthenticationSettings.DefaultLoginUrl;
                }

                FormsAuthentication.EnableFormsAuthentication(configurationData);
            }
        }
    }
}
