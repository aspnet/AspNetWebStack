// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Async.Test
{
    public class SingleEntryGateTest
    {
        [Fact]
        public void TryEnterShouldBeTrueForFirstCallAndFalseForSubsequentCalls()
        {
            // Arrange
            SingleEntryGate gate = new SingleEntryGate();

            // Act
            bool firstCall = gate.TryEnter();
            bool secondCall = gate.TryEnter();
            bool thirdCall = gate.TryEnter();

            // Assert
            Assert.True(firstCall);
            Assert.False(secondCall);
            Assert.False(thirdCall);
        }
    }
}
