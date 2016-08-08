// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelStateTest
    {
        [Fact]
        public void ErrorsProperty()
        {
            // Arrange
            ModelState modelState = new ModelState();

            // Act & Assert
            Assert.NotNull(modelState.Errors);
        }
    }
}
