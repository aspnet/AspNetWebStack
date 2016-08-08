// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Mvc.Filters
{
    /// <summary>Represents an authentication context containing information for performing authentication.</summary>
    public class AuthenticationContext : ControllerContext
    {
        /// <summary>Initializes a new instance of the <see cref="AuthenticationContext"/> class.</summary>
        /// <remarks>This constructor should only be used for unit testing purposes.</remarks>
        public AuthenticationContext()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AuthenticationContext"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="principal">The current principal.</param>
        public AuthenticationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor,
            IPrincipal principal)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            ActionDescriptor = actionDescriptor;
            Principal = principal;
        }

        /// <summary>Gets or sets the action descriptor.</summary>
        public ActionDescriptor ActionDescriptor { get; set; }

        /// <summary>Gets or sets the currently authenticated principal.</summary>
        public IPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets the error result, which indicates that authentication was attempted and failed.
        /// </summary>
        public ActionResult Result { get; set; }
    }
}
