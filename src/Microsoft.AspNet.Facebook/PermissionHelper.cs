// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.AspNet.Facebook
{
    internal static class PermissionHelper
    {
        // This cookie name is used to track permissions we've requested of the user to determine skipped permissions.
        public const string RequestedPermissionCookieName = "_fb_requested_permissions";

        public static IEnumerable<string> GetDeclinedPermissions(PermissionsStatus permissionsStatus)
        {
            return GetPermissionsWithStatus(permissionsStatus, PermissionStatus.Declined);
        }

        public static IEnumerable<string> GetGrantedPermissions(PermissionsStatus permissionsStatus)
        {
            return GetPermissionsWithStatus(permissionsStatus, PermissionStatus.Granted);
        }

        public static IEnumerable<string> GetPreviouslyRequestedPermissions(HttpRequestBase request)
        {
            HttpCookie existingCookie = request.Cookies.Get(RequestedPermissionCookieName);

            // If there's no cookie or an empty cookie then don't return a 1 element enumerable, return an empty one.
            if (existingCookie == null || String.IsNullOrEmpty(existingCookie.Value))
            {
                return Enumerable.Empty<string>();
            }

            return existingCookie.Value.Split(',');
        }

        public static HashSet<string> GetRequiredPermissions(IEnumerable<FacebookAuthorizeAttribute> facebookAuthorizeAttributes)
        {
            var requiredPermissions = new HashSet<string>(
                facebookAuthorizeAttributes.SelectMany(attribute => attribute.Permissions),
                StringComparer.Ordinal);

            return requiredPermissions;
        }

        public static IEnumerable<string> GetSkippedPermissions(HttpRequestBase request,
                                                                IEnumerable<string> missingPermissions,
                                                                IEnumerable<string> declinedPermissions)
        {
            IEnumerable<string> previouslyRequestedPermissions = PermissionHelper.GetPreviouslyRequestedPermissions(request);
            IEnumerable<string> previouslyRequestedMissingPermissions = missingPermissions.Where((permission) =>
                previouslyRequestedPermissions.Contains(permission));
            IEnumerable<string> skippedPermissions = previouslyRequestedMissingPermissions.Except(declinedPermissions);

            return skippedPermissions;
        }

        public static void PersistRequestedPermissions(AuthorizationContext context, IEnumerable<string> requestedPermissions)
        {
            HttpRequestBase request = context.HttpContext.Request;
            IEnumerable<string> existingRequestedPermissions = GetPreviouslyRequestedPermissions(request);
            IEnumerable<string> combinedRequestedPermissions = existingRequestedPermissions.Concat(requestedPermissions);

            // No need for duplicates
            combinedRequestedPermissions = combinedRequestedPermissions.Distinct(StringComparer.Ordinal);

            string newCookieValue = String.Join(",", combinedRequestedPermissions);

            HttpCookieCollection responseCookies = context.HttpContext.Response.Cookies;
            HttpCookie cookie = new HttpCookie(RequestedPermissionCookieName, newCookieValue);
            responseCookies.Add(cookie);
        }

        public static bool RequestedPermissionsCookieExists(HttpRequestBase request)
        {
            return request.Cookies.Get(RequestedPermissionCookieName) != null;
        }

        private static IEnumerable<string> GetPermissionsWithStatus(PermissionsStatus permissionsStatus, 
                                                                    PermissionStatus status)
        {
            return permissionsStatus.Status.Where(kvp => kvp.Value == status)
                                           .Select(kvp => kvp.Key);
        }
    }
}
