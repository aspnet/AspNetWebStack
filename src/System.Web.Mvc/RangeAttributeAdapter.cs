// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.Mvc
{
    public class RangeAttributeAdapter : DataAnnotationsModelValidator<RangeAttribute>
    {
        public RangeAttributeAdapter(ModelMetadata metadata, ControllerContext context, RangeAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            string errorMessage = ErrorMessage; // Per Dev10 Bug #923283, need to make sure ErrorMessage is called before Minimum/Maximum
            return new[] { new ModelClientValidationRangeRule(errorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
    }
}
