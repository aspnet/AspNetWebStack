// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "No compelling performance reason to seal this type.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ValidateInputAttribute : FilterAttribute, IAuthorizationFilter
    {
        public ValidateInputAttribute(bool enableValidation)
        {
            EnableValidation = enableValidation;
        }

        public bool EnableValidation { get; private set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            filterContext.Controller.ValidateRequest = EnableValidation;
        }
    }
}
