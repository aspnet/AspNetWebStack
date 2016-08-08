// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    /// <summary>
    /// A validation adapter that is used to map <see cref="DataTypeAttribute"/>'s to a single client side validation rule.
    /// </summary>
    internal class DataTypeAttributeAdapter : DataAnnotationsModelValidator
    {
        public DataTypeAttributeAdapter(ModelMetadata metadata, ControllerContext context, DataTypeAttribute attribute, string ruleName)
            : base(metadata, context, attribute)
        {
            if (String.IsNullOrEmpty(ruleName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "ruleName");
            }
            RuleName = ruleName;
        }

        public string RuleName { get; set; }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationRule
            {
                ValidationType = RuleName,
                ErrorMessage = ErrorMessage
            };
        }
    }
}
