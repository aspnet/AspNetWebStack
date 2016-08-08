// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Security;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Helper methods for Suppressing Form Authentication Redirect
    /// </summary>
    internal static class SuppressFormsAuthRedirectHelper
    {
        internal static readonly string AppSettingsSuppressFormsAuthenticationRedirectKey = "webapi:EnableSuppressRedirect";

        /// <summary>
        /// Returns whether the module is explicitly enabled or not
        /// </summary>
        internal static bool GetEnabled(NameValueCollection appSettings)
        {
            string disableSuppressRedirect = appSettings.Get(AppSettingsSuppressFormsAuthenticationRedirectKey);

            if (!String.IsNullOrEmpty(disableSuppressRedirect))
            {
                bool enabled;

                // anything but "false" will return true, which is the default behavior
                if (Boolean.TryParse(disableSuppressRedirect, out enabled))
                {
                    if (!enabled)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}