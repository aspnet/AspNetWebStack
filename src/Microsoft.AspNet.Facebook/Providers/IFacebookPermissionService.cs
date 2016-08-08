// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Facebook.Providers
{
    /// <summary>
    /// Provides an abstraction for retrieving the user permissions.
    /// </summary>
    public interface IFacebookPermissionService
    {
        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The user permissions.</returns>
        IEnumerable<string> GetUserPermissions(string userId, string accessToken);

        /// <summary>
        /// Gets the users permission status.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The user permissions status.</returns>
        PermissionsStatus GetUserPermissionsStatus(string userId, string accessToken);
    }
}