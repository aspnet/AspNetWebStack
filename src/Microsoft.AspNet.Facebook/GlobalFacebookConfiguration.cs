// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Facebook.Providers;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Provides a global <see cref="FacebookConfiguration"/> for ASP.NET applications.
    /// </summary>
    public static class GlobalFacebookConfiguration
    {
        private static readonly Lazy<FacebookConfiguration> _configuration = new Lazy<FacebookConfiguration>(
        () =>
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            config.PermissionService = new DefaultFacebookPermissionService(config);
            return config;
        });

        /// <summary>
        /// Gets the global <see cref="FacebookConfiguration"/>.
        /// </summary>
        public static FacebookConfiguration Configuration
        {
            get
            {
                return _configuration.Value;
            }
        }
    }
}