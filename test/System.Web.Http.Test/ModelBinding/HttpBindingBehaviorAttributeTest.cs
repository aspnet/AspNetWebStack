// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    public class HttpHttpBindingBehaviorAttributeTest
    {
        [Fact]
        public void Behavior_Property()
        {
            // Arrange
            HttpBindingBehavior expectedBehavior = (HttpBindingBehavior)(-20);

            // Act
            HttpBindingBehaviorAttribute attr = new HttpBindingBehaviorAttribute(expectedBehavior);

            // Assert
            Assert.Equal(expectedBehavior, attr.Behavior);
        }

        [Fact]
        public void TypeId_ReturnsSameValue()
        {
            // Arrange
            HttpBindNeverAttribute neverAttr = new HttpBindNeverAttribute();
            HttpBindRequiredAttribute requiredAttr = new HttpBindRequiredAttribute();

            // Act & assert
            Assert.Same(neverAttr.TypeId, requiredAttr.TypeId);
        }

        [Fact]
        public void BindNever_SetsBehavior()
        {
            // Act
            HttpBindingBehaviorAttribute attr = new HttpBindNeverAttribute();

            // Assert
            Assert.Equal(HttpBindingBehavior.Never, attr.Behavior);
        }

        [Fact]
        public void BindRequired_SetsBehavior()
        {
            // Act
            HttpBindingBehaviorAttribute attr = new HttpBindRequiredAttribute();

            // Assert
            Assert.Equal(HttpBindingBehavior.Required, attr.Behavior);
        }
    }
}
