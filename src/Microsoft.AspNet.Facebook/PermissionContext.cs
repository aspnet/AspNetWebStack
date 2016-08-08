// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Provides access to permission information associated with a user.
    /// </summary>
    public class PermissionContext
    {
        /// <summary>
        /// Permissions that were previously requested for but not granted for the lifetime of this application. This can happen
        /// by a user revoking, skipping or choosing not to allow permissions in the Facebook login dialog.
        /// </summary>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        public IEnumerable<string> DeclinedPermissions { get; set; }

        /// <summary>
        /// Provides access to Facebook-specific information.
        /// </summary>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        public FacebookContext FacebookContext { get; set; }

        /// <summary>
        /// Provides access to filter information.
        /// </summary>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        public AuthorizationContext FilterContext { get; set; }

        /// <summary>
        /// The entire list of missing permissions for the current page, including <see cref="DeclinedPermissions"/> and 
        /// <see cref="SkippedPermissions"/>.
        /// </summary>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        public IEnumerable<string> MissingPermissions { get; set; }

        /// <summary>
        /// The entire list of requested permissions for the current page. Includes permissions that were already prompted for.
        /// </summary>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", 
            Justification = "This is public and can be set in tests.")]
        public HashSet<string> RequiredPermissions { get; set; }

        /// <summary>
        /// The <see cref="ActionResult" /> that should be used to control the login flow.  If value is null then we will continue
        /// onto the action that is intended to be invoked.  Non-null values short-circuit the action.
        /// </summary>
        public ActionResult Result { get; set; }

        /// <summary>
        /// Permissions that were previously requested for but skipped for the current page. This can happen from a user hitting
        /// the "skip" button when requesting permissions.
        /// </summary>
        /// <remarks>Skips are tracked via cookies. If cookies are cleared skips will not be detected.</remarks>
        /// <remarks>
        /// This should only ever be "set" or modified within tests.
        /// </remarks>
        public IEnumerable<string> SkippedPermissions { get; set; }

        internal string RedirectUrl { get; set; }
    }
}
