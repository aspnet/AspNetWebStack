// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public class AuthorizationContext : ControllerContext
    {
        // parameterless constructor used for mocking
        public AuthorizationContext()
        {
        }

        [Obsolete("The recommended alternative is the constructor AuthorizationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor).")]
        public AuthorizationContext(ControllerContext controllerContext)
            : base(controllerContext)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public AuthorizationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            ActionDescriptor = actionDescriptor;
        }

        public virtual ActionDescriptor ActionDescriptor { get; set; }

        public ActionResult Result { get; set; }
    }
}
