// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Provides access to a users permissions status and the raw data returned from an API.
    /// </summary>
    public class PermissionsStatus
    {
        // Values representing permission statuses returned by the facebook graph API.
        private const string FacebookPermissionGranted = "granted";
        private const string FacebookPermissionDeclined = "declined";

        /// <summary>
        /// Constructs a more useable object than the provided api result.
        /// </summary>
        /// <param name="apiResult">The raw data returned by the queried API.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This type is the raw type of data that Facebook returns to us.")]
        public PermissionsStatus(IList<IDictionary<string, string>> apiResult)
        {
            ApiResult = apiResult;
            Status = ConvertApiResult(apiResult);
        }

        /// <summary>
        /// A parsed, easier to use version of the <see cref="ApiResult"/>.
        /// </summary>
        public IDictionary<string, PermissionStatus> Status { get; private set; }

        /// <summary>
        /// The raw data returned by the queried API.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", 
            Justification = "This type is the raw type of data that Facebook returns to us.")]
        public IList<IDictionary<string, string>> ApiResult { get; private set; }

        /// <summary>
        /// Queries the <see cref="Status"/> member for the provided permission name.
        /// </summary>
        /// <param name="permission">The name of the permission to query the <see cref="Status"/> for.</param>
        /// <returns>The permission's status.</returns>
        public PermissionStatus this[string permission]
        {
            get
            {
                return Status[permission];
            }
        }

        private static PermissionStatus ConvertPermissionStatus(string permissionStatus)
        {
            if (String.Equals(permissionStatus, FacebookPermissionGranted, StringComparison.OrdinalIgnoreCase))
            {
                return PermissionStatus.Granted;
            }
            else if (String.Equals(permissionStatus, FacebookPermissionDeclined, StringComparison.OrdinalIgnoreCase))
            {
                return PermissionStatus.Declined;
            }

            return PermissionStatus.Unknown;
        }

        private static IDictionary<string, PermissionStatus> ConvertApiResult(IList<IDictionary<string, string>> apiResults)
        {
            IDictionary<string, PermissionStatus> transformedPermissions =
                new Dictionary<string, PermissionStatus>(StringComparer.OrdinalIgnoreCase);

            if (apiResults != null && apiResults.Any())
            {
                foreach (IDictionary<string, string> permissionData in apiResults)
                {
                    PermissionStatus status = ConvertPermissionStatus(permissionData["status"]);
                    transformedPermissions.Add(permissionData["permission"], status);
                }
            }

            return transformedPermissions;
        }
    }
}
