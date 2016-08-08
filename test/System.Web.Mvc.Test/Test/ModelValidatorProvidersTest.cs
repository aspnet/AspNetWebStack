// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelValidatorProvidersTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Arrange
            Type[] expectedTypes = new Type[]
            {
                typeof(DataAnnotationsModelValidatorProvider),
                typeof(DataErrorInfoModelValidatorProvider),
                typeof(ClientDataTypeModelValidatorProvider)
            };

            // Act
            Type[] actualTypes = ModelValidatorProviders.Providers.Select(p => p.GetType()).ToArray();

            // Assert
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
