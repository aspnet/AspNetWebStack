// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Web.WebPages.OAuth.Properties;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Represents an OAuth &amp; OpenID account.
    /// </summary>
    public class OAuthAccount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthAccount"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public OAuthAccount(string provider, string providerUserId)
        {
            if (String.IsNullOrEmpty(provider))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "provider"),
                    "provider");
            }

            if (String.IsNullOrEmpty(providerUserId))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "providerUserId"),
                    "providerUserId");
            }

            Provider = provider;
            ProviderUserId = providerUserId;
        }

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string Provider { get; private set; }

        /// <summary>
        /// Gets the provider user id.
        /// </summary>
        public string ProviderUserId { get; private set; }
    }
}
