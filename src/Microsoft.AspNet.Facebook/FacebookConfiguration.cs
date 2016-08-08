// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNet.Facebook.Providers;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Configuration for the Facebook application.
    /// </summary>
    public class FacebookConfiguration
    {
        private const string FacebookAppBaseUrl = "https://apps.facebook.com";
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();
        private string _appUrl;
        private string _authorizationRedirectPath;
        private string _cannotCreateCookieRedirectPath;

        /// <summary>
        /// Gets or sets the App ID.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the App Secret.
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the App Namespace.
        /// </summary>
        public string AppNamespace { get; set; }

        /// <summary>
        /// Gets or sets the URL path that the <see cref="Microsoft.AspNet.Facebook.Authorization.FacebookAuthorizeFilter"/> will 
        /// redirect to when the user did not grant the required permissions. If value is not set it will result in a redirection
        /// to Facebook's home page.
        /// </summary>
        public string AuthorizationRedirectPath
        {
            get
            {
                return _authorizationRedirectPath;
            }
            set
            {
                EnsureRedirectPath(value, "AuthorizationRedirectPath");
                _authorizationRedirectPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL path that the <see cref="Microsoft.AspNet.Facebook.Authorization.FacebookAuthorizeFilter"/> will 
        /// redirect to when the we determine that we are unable to create cookies. If value is not set it will result in a 
        /// redirection to Facebook's home page.
        /// </summary>
        public string CannotCreateCookieRedirectPath
        {
            get
            {
                return _cannotCreateCookieRedirectPath;
            }
            set
            {
                EnsureRedirectPath(value, "CannotCreateCookieRedirectPath");

                _cannotCreateCookieRedirectPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the absolute URL for the Facebook App.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "We prefer strings because this is read from appSettings")]
        public string AppUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_appUrl))
                {
                    _appUrl = GetAppUrl();
                }
                return _appUrl;
            }
            set
            {
                _appUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IFacebookClientProvider"/>.
        /// </summary>
        public IFacebookClientProvider ClientProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFacebookPermissionService"/>.
        /// </summary>
        public IFacebookPermissionService PermissionService { get; set; }

        /// <summary>
        /// Gets the additional properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Loads the configuration properties from app settings.
        /// </summary>
        /// <remarks>
        /// It will map the following keys from appSettings to the corresponding properties:
        /// Facebook:AppId = AppId,
        /// Facebook:AppSecret = AppSecret,
        /// Facebook:AppNamespace = AppNamespace,
        /// Facebook:AppUrl = AppUrl,
        /// Facebook:AuthorizationRedirectPath = AuthorizationRedirectPath.
        /// </remarks>
        public virtual void LoadFromAppSettings()
        {
            AppId = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppId];
            if (String.IsNullOrEmpty(AppId))
            {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AppSettingIsRequired,
                    FacebookAppSettingKeys.AppId));
            }

            AppSecret = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppSecret];
            if (String.IsNullOrEmpty(AppSecret))
            {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AppSettingIsRequired,
                    FacebookAppSettingKeys.AppSecret));
            }

            AppNamespace = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppNamespace];
            AppUrl = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppUrl];
            AuthorizationRedirectPath = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AuthorizationRedirectPath];
            CannotCreateCookieRedirectPath = 
                ConfigurationManager.AppSettings[FacebookAppSettingKeys.CannotCreateCookiesRedirectPath];
        }

        private static void EnsureRedirectPath(string value, string redirectParameterName)
        {
            // Check for '~/' prefix while allowing null or empty value to be set.
            if (!String.IsNullOrEmpty(value) && !value.StartsWith("~/", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.InvalidRedirectPath,
                        redirectParameterName),
                    "value");
            }
        }

        private string GetAppUrl()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}",
                FacebookAppBaseUrl,
                String.IsNullOrEmpty(AppNamespace) ? AppId : AppNamespace);
        }
    }
}