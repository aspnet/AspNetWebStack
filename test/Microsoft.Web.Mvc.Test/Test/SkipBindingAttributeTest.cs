// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class SkipBindingAttributeTest
    {
        [Fact]
        public void GetBinderReturnsModelBinderWhichReturnsNull()
        {
            // Arrange
            CustomModelBinderAttribute attr = new SkipBindingAttribute();
            IModelBinder binder = attr.GetBinder();

            // Act
            object result = binder.BindModel(null, null);

            // Assert
            Assert.Null(result);
        }
    }
}
