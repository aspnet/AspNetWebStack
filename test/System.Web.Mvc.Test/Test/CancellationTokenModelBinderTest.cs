// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class CancellationTokenModelBinderTest
    {
        [Fact]
        public void BinderReturnsDefaultCancellationToken()
        {
            // Arrange
            CancellationTokenModelBinder binder = new CancellationTokenModelBinder();

            // Act
            object binderResult = binder.BindModel(controllerContext: null, bindingContext: null);

            // Assert
            Assert.Equal(default(CancellationToken), binderResult);
        }
    }
}
