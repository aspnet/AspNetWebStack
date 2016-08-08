// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Restricts the access to requests with valid Facebook signed request parameter and to users that have the required permissions.
    /// This attribute can be declared on a controller, an action or both.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The attribute argument is already defined as a ReadOnlyCollection.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class FacebookAuthorizeAttribute : Attribute
    {
        private ReadOnlyCollection<string> _permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeAttribute" /> class without requiring permissions.
        /// </summary>
        public FacebookAuthorizeAttribute()
            : this(new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeAttribute" /> class requiring permissions.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        public FacebookAuthorizeAttribute(params string[] permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }

            foreach (string permission in permissions)
            {
                if (permission.Contains(','))
                {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Resources.PermissionStringShouldNotContainComma,
                            permission),
                            "permissions");
                }
            }

            _permissions = new ReadOnlyCollection<string>(permissions);
        }

        /// <summary>
        /// Gets the required permissions.
        /// </summary>
        public ReadOnlyCollection<string> Permissions
        {
            get
            {
                return _permissions;
            }
        }
    }
}