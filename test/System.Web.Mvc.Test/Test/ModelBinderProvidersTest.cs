// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelBinderProvidersTest
    {
        [Fact]
        public void CollectionDefaults()
        {
            // Act
            Type[] actualTypes = ModelBinderProviders.BinderProviders.Select(b => b.GetType()).ToArray();

            // Assert
            Assert.Equal(Enumerable.Empty<Type>(), actualTypes);
        }
    }
}
