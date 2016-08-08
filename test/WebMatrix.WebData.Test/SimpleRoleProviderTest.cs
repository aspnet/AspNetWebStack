// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.TestCommon;

namespace WebMatrix.WebData.Test
{
    public class SimpleRoleProviderTest
    {
        [Theory]
        [ReplaceCulture]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void SimpleRoleProvider_CasingBehavior_ValidatesRange(int value, bool isValid)
        {
            // Arrange
            var provider = new SimpleRoleProvider();

            var message =
                "The value of argument 'value' (" + value + ") is invalid for Enum type " +
                "'SimpleMembershipProviderCasingBehavior'." + Environment.NewLine +
                "Parameter name: value";

            // Act
            Exception exception = null;

            try
            {
                provider.CasingBehavior = (SimpleMembershipProviderCasingBehavior)value;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (isValid)
            {
                Assert.Equal((SimpleMembershipProviderCasingBehavior)value, provider.CasingBehavior);
            }
            else
            {
                Assert.NotNull(exception);
                Assert.IsAssignableFrom<InvalidEnumArgumentException>(exception);
                Assert.Equal(message, exception.Message);
            }
        }
    }
}
