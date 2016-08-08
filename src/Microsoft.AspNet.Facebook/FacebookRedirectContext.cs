// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.Facebook.ModelBinders;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Provides access to the data redirected from <see cref="Microsoft.AspNet.Facebook.Authorization.FacebookAuthorizeFilter"/>.
    /// </summary>
    [FacebookRedirectContextBinder]
    public class FacebookRedirectContext
    {
        /// <summary>
        /// Gets or sets the origin URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "We prefer strings because this is read from query strings")]
        public string OriginUrl { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "We prefer strings because this is read from query strings")]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the required permissions specified on <see cref="FacebookAuthorizeAttribute"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a shipped API")]
        public string[] RequiredPermissions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FacebookConfiguration"/>.
        /// </summary>
        public FacebookConfiguration Configuration { get; set; }
    }
}