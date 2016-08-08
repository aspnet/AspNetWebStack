// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Facebook;
using Microsoft.AspNet.Facebook.Client;

namespace Microsoft.AspNet.Facebook.Providers
{
    /// <summary>
    /// Default implementation of <see cref="IFacebookPermissionService"/>.
    /// </summary>
    public class DefaultFacebookPermissionService : IFacebookPermissionService
    {
        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFacebookPermissionService" /> class.
        /// </summary>
        /// <param name="configuration">The <see cref="FacebookConfiguration"/>.</param>
        public DefaultFacebookPermissionService(FacebookConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _config = configuration;
        }

        /// <summary>
        /// Gets the user permissions by calling the Facebook Graph API.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The user permissions.</returns>
        public virtual IEnumerable<string> GetUserPermissions(string userId, string accessToken)
        {
            if (userId == null)
            {
                throw new ArgumentNullException("userId");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            PermissionsStatus permissionsStatus = GetUserPermissionsStatus(userId, accessToken);

            return PermissionHelper.GetGrantedPermissions(permissionsStatus);
        }

        /// <summary>
        /// Gets the users permission status by calling the Facebook graph API.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The users permission status is in the following format { { "permissionName", "granted|declined" } }.</returns>
        public virtual PermissionsStatus GetUserPermissionsStatus(string userId, string accessToken)
        {
            if (userId == null)
            {
                throw new ArgumentNullException("userId");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            FacebookClient client = _config.ClientProvider.CreateClient();
            client.AccessToken = accessToken;

            IList<IDictionary<string, string>> rawPermissionsStatus = client.GetCurrentUserPermissionsStatus();
            PermissionsStatus permissionsStatus = new PermissionsStatus(rawPermissionsStatus);

            return permissionsStatus;
        }
    }
}