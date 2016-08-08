// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelValidationResultTest
    {
        [Fact]
        public void MemberNameProperty()
        {
            // Arrange
            ModelValidationResult result = new ModelValidationResult();

            // Act & assert
            MemberHelper.TestStringProperty(result, "MemberName", String.Empty);
        }

        [Fact]
        public void MessageProperty()
        {
            // Arrange
            ModelValidationResult result = new ModelValidationResult();

            // Act & assert
            MemberHelper.TestStringProperty(result, "Message", String.Empty);
        }
    }
}
