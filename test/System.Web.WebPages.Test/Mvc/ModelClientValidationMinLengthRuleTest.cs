// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelClientValidationMinLengthRuleTest
    {
        [Fact]
        public void ModelClientValidationMinLengthRuleTestAddsMinLengthParameter()
        {
            // Arrange
            var clientValidationRule = new ModelClientValidationMinLengthRule("Min Length message", 2);

            // Assert
            Assert.Equal(1, clientValidationRule.ValidationParameters.Count);
            Assert.Equal(2, clientValidationRule.ValidationParameters["min"]);
            Assert.Equal("Min Length message", clientValidationRule.ErrorMessage);
        }
    }
}
