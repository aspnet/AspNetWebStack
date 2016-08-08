// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class BindingBehaviorAttributeTest
    {
        [Fact]
        public void Behavior_Property()
        {
            // Arrange
            BindingBehavior expectedBehavior = (BindingBehavior)(-20);

            // Act
            BindingBehaviorAttribute attr = new BindingBehaviorAttribute(expectedBehavior);

            // Assert
            Assert.Equal(expectedBehavior, attr.Behavior);
        }

        [Fact]
        public void TypeId_ReturnsSameValue()
        {
            // Arrange
            BindNeverAttribute neverAttr = new BindNeverAttribute();
            BindRequiredAttribute requiredAttr = new BindRequiredAttribute();

            // Act & assert
            Assert.Same(neverAttr.TypeId, requiredAttr.TypeId);
        }

        [Fact]
        public void BindNever_SetsBehavior()
        {
            // Act
            BindingBehaviorAttribute attr = new BindNeverAttribute();

            // Assert
            Assert.Equal(BindingBehavior.Never, attr.Behavior);
        }

        [Fact]
        public void BindRequired_SetsBehavior()
        {
            // Act
            BindingBehaviorAttribute attr = new BindRequiredAttribute();

            // Assert
            Assert.Equal(BindingBehavior.Required, attr.Behavior);
        }
    }
}
