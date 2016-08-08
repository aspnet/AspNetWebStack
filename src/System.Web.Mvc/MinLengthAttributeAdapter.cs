// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.Mvc
{
    public class MinLengthAttributeAdapter : DataAnnotationsModelValidator<MinLengthAttribute>
    {
        public MinLengthAttributeAdapter(ModelMetadata metadata, ControllerContext context, MinLengthAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationMinLengthRule(ErrorMessage, Attribute.Length) };
        }
    }
}
