// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class FieldValidationMetadataTest
    {
        [Fact]
        public void FieldNameProperty()
        {
            // Arrange
            FieldValidationMetadata metadata = new FieldValidationMetadata();

            // Act & assert
            MemberHelper.TestStringProperty(metadata, "FieldName", String.Empty);
        }

        [Fact]
        public void ValidationRulesProperty()
        {
            // Arrange
            FieldValidationMetadata metadata = new FieldValidationMetadata();

            // Act
            ICollection<ModelClientValidationRule> validationRules = metadata.ValidationRules;

            // Assert
            Assert.NotNull(validationRules);
            Assert.Empty(validationRules);
        }
    }
}
