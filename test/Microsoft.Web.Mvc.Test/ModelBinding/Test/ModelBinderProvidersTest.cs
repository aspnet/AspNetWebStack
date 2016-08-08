// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ModelBinderProvidersTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Arrange
            Type[] expectedTypes = new[]
            {
                typeof(TypeMatchModelBinderProvider),
                typeof(BinaryDataModelBinderProvider),
                typeof(KeyValuePairModelBinderProvider),
                typeof(ComplexModelDtoModelBinderProvider),
                typeof(ArrayModelBinderProvider),
                typeof(DictionaryModelBinderProvider),
                typeof(CollectionModelBinderProvider),
                typeof(TypeConverterModelBinderProvider),
                typeof(MutableObjectModelBinderProvider)
            };

            // Act
            Type[] actualTypes = ModelBinderProviders.Providers.Select(p => p.GetType()).ToArray();

            // Assert
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
