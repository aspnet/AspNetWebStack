// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace System.Web.WebPages.Administration
{
    public class SiteAdmin
    {
        internal const string DefaultAdminVirtualPath = "~/_Admin/";
        internal const string AdminSettingsFolder = "~/App_Data/Admin/";

        // Configuration settings
        private const string AdminVirtualPathAppSettingsKey = "asp:AdminFolderVirtualPath";
        private const string AdminEnabledAppSettingsKey = "asp:AdminManagerEnabled";

        private const string ReturnUrlQueryString = "ReturnUrl";

        // Virtual paths excluded from security. These urls are the same as the ones in the PageVirtulPath attribute.
        internal const string LoginVirtualPath = "~/Login.cshtml";
        internal const string LogoutVirtualPath = "~/Logout.cshtml";
        internal const string RegisterVirtualPath = "~/Register.cshtml";
        internal const string EnableInstructionsVirtualPath = "~/EnableInstructions.cshtml";

        private static ConcurrentDictionary<string, SiteAdmin> _adminModules = new ConcurrentDictionary<string, SiteAdmin>();

        // These only needs to be computed once per app domain
        private static readonly Lazy<bool?> _adminEnabled = new Lazy<bool?>(GetAdminEnabledSetting);
        private static readonly Lazy<string> _adminVirtualPath = new Lazy<string>(GetDefaultVirtualPath);

        private SiteAdmin(string startPageVirtualPath, string displayName, string description)
        {
            Debug.Assert(startPageVirtualPath != null, "startPageVirtualPath can't be null");
            Debug.Assert(displayName != null, "displayName can't be null");

            StartPageVirtualPath = startPageVirtualPath;
            DisplayName = displayName;
            Description = description;
        }

        public static string AdminVirtualPath
        {
            get { return _adminVirtualPath.Value; }
        }

        public string StartPageVirtualPath { get; private set; }

        public string DisplayName { get; private set; }

        public string Description { get; private set; }

        internal static bool Available
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.Request.IsLocal)
                {
                    // Check the configuration setting if there is any
                    if (_adminEnabled.Value != null)
                    {
                        return _adminEnabled.Value.Value;
                    }

                    // There was no setting so localhost is enough to verify availability
                    return true;
                }
                // If we're not on localhost then nothing is available
                return false;
            }
        }

        public static IEnumerable<SiteAdmin> Modules
        {
            get { return _adminModules.Values; }
        }

        internal static void RegisterAdminModule()
        {
            // Add a admin module as an application module (precompiled)
            ApplicationPart.Register(new ApplicationPart(typeof(SiteAdmin).Assembly, AdminVirtualPath));
        }

        private static bool? GetAdminEnabledSetting()
        {
            bool enabled;
            if (Boolean.TryParse(ConfigurationManager.AppSettings[AdminEnabledAppSettingsKey], out enabled))
            {
                return enabled;
            }

            return null;
        }

        public static string GetVirtualPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }

            // Don't add the virtual path more than once
            if (virtualPath.StartsWith(AdminVirtualPath, StringComparison.OrdinalIgnoreCase))
            {
                return virtualPath;
            }

            if (virtualPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                virtualPath = virtualPath.Substring(2);
            }

            if (virtualPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                virtualPath = virtualPath.Substring(1);
            }
            return VirtualPathUtility.Combine(AdminVirtualPath, virtualPath);
        }

        public static void Register(string startPageVirtualPath, string displayName, string description)
        {
            if (startPageVirtualPath == null)
            {
                throw new ArgumentNullException("startPageVirtualPath");
            }

            if (displayName == null)
            {
                throw new ArgumentNullException("displayName");
            }

            // Get the virtual path relative to the admin virtual path
            string virtualPath = GetVirtualPath(startPageVirtualPath);

            // Register that under the admin root
            Register(new SiteAdmin(virtualPath, displayName, description));
        }

        private static string GetDefaultVirtualPath()
        {
            string virtualPath = ConfigurationManager.AppSettings[AdminVirtualPathAppSettingsKey];
            if (String.IsNullOrEmpty(virtualPath))
            {
                virtualPath = DefaultAdminVirtualPath;
            }

            if (!virtualPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                virtualPath += "/";
            }
            return virtualPath;
        }

        internal static void RedirectToLogin(HttpResponseBase response)
        {
            response.Redirect(GetVirtualPath(LoginVirtualPath));
        }

        internal static void RedirectToRegister(HttpResponseBase response)
        {
            response.Redirect(GetVirtualPath(RegisterVirtualPath));
        }

        internal static void RedirectToHome(HttpResponseBase response)
        {
            response.Redirect(AdminVirtualPath);
        }

        internal static void Register(SiteAdmin module)
        {
            if (_adminModules.ContainsKey(module.StartPageVirtualPath))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  AdminResources.ModuleAlreadyRegistered, module.StartPageVirtualPath));
            }

            // Add to the list of registered modules
            _adminModules.TryAdd(module.StartPageVirtualPath, module);
        }

        internal static string GetReturnUrl(HttpRequestBase request)
        {
            // REVIEW: FormsAuthentication.GetReturlUrl also checks the form for the return url
            // do we need that?
            string returnUrl = request.QueryString[ReturnUrlQueryString];

            // If the return url query string doesn't exist or is empty
            // return the admin root virtual path
            if (String.IsNullOrEmpty(returnUrl))
            {
                return null;
            }

            // REVIEW: FormsAuthentication.GetReturnUrl checks if the url is dangerous
            // i.e it uses an internal helper in System.Web CrossSiteScriptingValidation.IsDangerousUrl.
            // Should we copy that behavior?

            if (!VirtualPathUtility.IsAppRelative(returnUrl))
            {
                // We only put app relative return urls in the query string (i.e starts with ~/)
                throw new InvalidOperationException(AdminResources.InvalidReturnUrl);
            }

            return returnUrl;
        }

        internal static string GetRedirectUrl(string redirectUrl)
        {
            var request = new HttpRequestWrapper(HttpContext.Current.Request);
            return GetRedirectUrl(request, redirectUrl, VirtualPathUtility.ToAppRelative);
        }

        internal static string GetRedirectUrl(HttpRequestBase request, string redirectUrl, Func<string, string> makeAppRelative)
        {
            // If there's already a return url then use it, otherwise the app relative url of the 
            // current request to redirect to after signing in
            string returnUrl = GetReturnUrl(request) ?? makeAppRelative(request.RawUrl);

            // Get the app relative path to the redirect url
            redirectUrl = GetVirtualPath(redirectUrl);

            // Get the current page with return url
            redirectUrl += "?" + ReturnUrlQueryString + "=" + HttpUtility.UrlEncode(returnUrl);

            return redirectUrl;
        }
    }
}
