// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelClientValidationMembershipPasswordRuleTest
    {
        [Fact]
        public void Constructor()
        {
            // Arrange & Act
            var rule = new ModelClientValidationMembershipPasswordRule("ErrorMessage", 10, 5, "regex-value");

            // Assert
            Assert.Equal("password", rule.ValidationType);
            Assert.Equal("ErrorMessage", rule.ErrorMessage);
            Assert.Equal(10, rule.ValidationParameters["min"]);
            Assert.Equal(5, rule.ValidationParameters["nonalphamin"]);
            Assert.Equal("regex-value", rule.ValidationParameters["regex"]);
        }
    }
}
