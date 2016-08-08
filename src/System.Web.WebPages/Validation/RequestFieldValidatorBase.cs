// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages
{
    public abstract class RequestFieldValidatorBase : IValidator
    {
        private readonly string _errorMessage;
        private readonly bool _useUnvalidatedValues;

        protected RequestFieldValidatorBase(string errorMessage)
            : this(errorMessage, useUnvalidatedValues: false)
        {
        }

        protected RequestFieldValidatorBase(string errorMessage, bool useUnvalidatedValues)
        {
            if (String.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "errorMessage");
            }

            _errorMessage = errorMessage;
            _useUnvalidatedValues = useUnvalidatedValues;
        }

        public virtual ModelClientValidationRule ClientValidationRule
        {
            get { return null; }
        }

        /// <summary>
        /// Meant for unit tests that causes RequestFieldValidatorBase to basically ignore the unvalidated field requirement.
        /// </summary>
        internal static bool IgnoreUseUnvalidatedValues { get; set; }

        protected abstract bool IsValid(HttpContextBase httpContext, string value);

        public virtual ValidationResult Validate(ValidationContext validationContext)
        {
            var httpContext = GetHttpContext(validationContext);
            var field = validationContext.MemberName;
            var fieldValue = GetRequestValue(httpContext.Request, field);

            if (IsValid(httpContext, fieldValue))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(_errorMessage, memberNames: new[] { field });
        }

        protected static HttpContextBase GetHttpContext(ValidationContext validationContext)
        {
            Debug.Assert(validationContext.ObjectInstance is HttpContextBase, "For our validation context, ObjectInstance must be an HttpContextBase instance.");
            return (HttpContextBase)validationContext.ObjectInstance;
        }

        protected string GetRequestValue(HttpRequestBase request, string field)
        {
            if (IgnoreUseUnvalidatedValues)
            {
                // Make sure we do not set this when we are hosted since this is only meant for unit test scenarios.
                Debug.Assert(HttpContext.Current == null, "This flag should not be set when we are hosted.");
                return request.Form[field];
            }
            return _useUnvalidatedValues ? request.Unvalidated[field] : request.Form[field];
        }
    }
}
