// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class EmptyModelValidatorProviderTest
    {
        [Fact]
        public void ReturnsNoValidators()
        {
            // Arrange
            EmptyModelValidatorProvider provider = new EmptyModelValidatorProvider();

            // Act
            IEnumerable<ModelValidator> result = provider.GetValidators(null, null);

            // Assert
            Assert.Empty(result);
        }
    }
}
