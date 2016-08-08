// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Security;

namespace System.Web.Mvc
{
    internal class MembershipPasswordAttributeAdapter : DataAnnotationsModelValidator<MembershipPasswordAttribute>
    {
        public MembershipPasswordAttributeAdapter(ModelMetadata metadata, ControllerContext context, MembershipPasswordAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationMembershipPasswordRule(ErrorMessage, Attribute.MinRequiredPasswordLength, Attribute.MinRequiredNonAlphanumericCharacters, Attribute.PasswordStrengthRegularExpression);
        }
    }
}
