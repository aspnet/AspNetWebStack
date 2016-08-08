// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Facebook;

namespace Microsoft.AspNet.Facebook.Client
{
    /// <summary>
    /// Extension methods for <see cref="FacebookClient"/>.
    /// </summary>
    public static class FacebookClientExtensions
    {
        private const string PermissionsEndPoint = "me/permissions";

        /// <summary>
        /// Gets the Facebook object located at a given path.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="objectPath">The object path.</param>
        /// <returns>A Facebook object.</returns>
        public static Task<object> GetFacebookObjectAsync(this FacebookClient client, string objectPath)
        {
            return GetFacebookObjectAsync<object>(client, objectPath);
        }

        /// <summary>
        /// Gets the Facebook object located at a given path.
        /// </summary>
        /// <typeparam name="TFacebookObject">The type of the Facebook object.</typeparam>
        /// <param name="client">The client.</param>
        /// <param name="objectPath">The object path.</param>
        /// <returns>A Facebook object.</returns>
        public static Task<TFacebookObject> GetFacebookObjectAsync<TFacebookObject>(this FacebookClient client, string objectPath) where TFacebookObject : class
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (objectPath == null)
            {
                throw new ArgumentNullException("objectPath");
            }

            string path = objectPath + FacebookQueryHelper.GetFields(typeof(TFacebookObject));
            return client.GetTaskAsync<TFacebookObject>(path);
        }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The current user.</returns>
        public static Task<object> GetCurrentUserAsync(this FacebookClient client)
        {
            return GetCurrentUserAsync<object>(client);
        }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        /// <typeparam name="TUser">The type of the user.</typeparam>
        /// <param name="client">The client.</param>
        /// <returns>The current user.</returns>
        public static Task<TUser> GetCurrentUserAsync<TUser>(this FacebookClient client) where TUser : class
        {
            return GetFacebookObjectAsync<TUser>(client, "me");
        }

        /// <summary>
        /// Gets the current user friends information.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A collection of friends.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Using tasks")]
        public static Task<IList<object>> GetCurrentUserFriendsAsync(this FacebookClient client)
        {
            return GetCurrentUserFriendsAsync<object>(client);
        }

        /// <summary>
        /// Gets the current user friends information.
        /// </summary>
        /// <typeparam name="TUserFriend">The type of the user friend.</typeparam>
        /// <param name="client">The client.</param>
        /// <returns>A collection of friends.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Using tasks")]
        public static async Task<IList<TUserFriend>> GetCurrentUserFriendsAsync<TUserFriend>(this FacebookClient client) where TUserFriend : class
        {
            FacebookGroupConnection<TUserFriend> friends = await GetFacebookObjectAsync<FacebookGroupConnection<TUserFriend>>(client, "me/friends");
            return friends != null ?
                friends.Data :
                new TUserFriend[0];
        }

        /// <summary>
        /// Gets the permissions granted by the current user.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A collection of permissions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Using tasks")]
        public static async Task<IList<string>> GetCurrentUserPermissionsAsync(this FacebookClient client)
        {
            FacebookGroupConnection<IDictionary<string, string>> permissionResults = 
                await client.GetTaskAsync<FacebookGroupConnection<IDictionary<string, string>>>(PermissionsEndPoint);
            PermissionsStatus permissionsStatus = new PermissionsStatus(permissionResults.Data);

            return PermissionHelper.GetGrantedPermissions(permissionsStatus).ToList();
        }

        /// <summary>
        /// Gets the current user statuses.
        /// </summary>
        /// <typeparam name="TStatus">The type of the status.</typeparam>
        /// <param name="client">The client.</param>
        /// <returns>A collection of statuses.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Using tasks")]
        public static async Task<IList<TStatus>> GetCurrentUserStatusesAsync<TStatus>(this FacebookClient client) where TStatus : class
        {
            FacebookGroupConnection<TStatus> statuses = await GetFacebookObjectAsync<FacebookGroupConnection<TStatus>>(client, "me/statuses");
            return statuses != null ?
                statuses.Data :
                new TStatus[0];
        }

        /// <summary>
        /// Gets the current user photos.
        /// </summary>
        /// <typeparam name="TPhotos">The type of the photo.</typeparam>
        /// <param name="client">The client.</param>
        /// <returns>A collection of user photos.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Using tasks")]
        public static async Task<IList<TPhotos>> GetCurrentUserPhotosAsync<TPhotos>(this FacebookClient client) where TPhotos : class
        {
            FacebookGroupConnection<TPhotos> photos = await GetFacebookObjectAsync<FacebookGroupConnection<TPhotos>>(client, "me/photos");
            return photos != null ?
                photos.Data :
                new TPhotos[0];
        }

        internal static Uri GetLoginUrl(this FacebookClient client, string redirectUrl, string appId, string permissions)
        {
            if (String.IsNullOrEmpty(redirectUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "redirectUrl");
            }
            if (String.IsNullOrEmpty(appId))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "appId");
            }

            Dictionary<string, object> loginUrlParameters = new Dictionary<string, object>();
            loginUrlParameters["redirect_uri"] = redirectUrl;
            loginUrlParameters["client_id"] = appId;
            if (!String.IsNullOrEmpty(permissions))
            {
                loginUrlParameters["scope"] = permissions;
            }

            return client.GetLoginUrl(loginUrlParameters);
        }

        internal static IList<IDictionary<string, string>> GetCurrentUserPermissionsStatus(this FacebookClient client)
        {
            FacebookGroupConnection<IDictionary<string, string>> permissionResults = 
                client.Get<FacebookGroupConnection<IDictionary<string, string>>>(PermissionsEndPoint);

            return permissionResults.Data;
        }
    }
}