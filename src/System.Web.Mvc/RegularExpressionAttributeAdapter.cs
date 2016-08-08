// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.Mvc
{
    public class RegularExpressionAttributeAdapter : DataAnnotationsModelValidator<RegularExpressionAttribute>
    {
        public RegularExpressionAttributeAdapter(ModelMetadata metadata, ControllerContext context, RegularExpressionAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Attribute.Pattern) };
        }
    }
}
