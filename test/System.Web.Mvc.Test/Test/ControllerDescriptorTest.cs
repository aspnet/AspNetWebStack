// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ControllerDescriptorTest
    {
        [Fact]
        public void ControllerNamePropertyReturnsControllerTypeName()
        {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor(typeof(object));

            // Act
            string name = cd.ControllerName;

            // Assert
            Assert.Equal("Object", name);
        }

        [Fact]
        public void ControllerNamePropertyReturnsControllerTypeNameWithoutControllerSuffix()
        {
            // Arrange
            Mock<Type> mockType = new Mock<Type>();
            mockType.Setup(t => t.Name).Returns("somecontroller");
            ControllerDescriptor cd = GetControllerDescriptor(mockType.Object);

            // Act
            string name = cd.ControllerName;

            // Assert
            Assert.Equal("some", name);
        }

        [Fact]
        public void GetCustomAttributesReturnsEmptyArrayOfAttributeType()
        {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act
            ObsoleteAttribute[] attrs = (ObsoleteAttribute[])cd.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetCustomAttributesThrowsIfAttributeTypeIsNull()
        {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { cd.GetCustomAttributes(null /* attributeType */, true); }, "attributeType");
        }

        [Fact]
        public void GetCustomAttributesWithoutAttributeTypeCallsGetCustomAttributesWithAttributeType()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<ControllerDescriptor> mockDescriptor = new Mock<ControllerDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(object), true)).Returns(expected);
            ControllerDescriptor cd = mockDescriptor.Object;

            // Act
            object[] returned = cd.GetCustomAttributes(true /* inherit */);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetFilterAttributes_CallsGetCustomAttributes()
        {
            // Arrange
            var mockDescriptor = new Mock<ControllerDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(FilterAttribute), true)).Returns(new object[] { new Mock<FilterAttribute>().Object }).Verifiable();

            // Act
            var result = mockDescriptor.Object.GetFilterAttributes(true).ToList();

            // Assert
            mockDescriptor.Verify();
            Assert.Single(result);
        }

        [Fact]
        public void IsDefinedReturnsFalse()
        {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act
            bool isDefined = cd.IsDefined(typeof(object), true);

            // Assert
            Assert.False(isDefined);
        }

        [Fact]
        public void IsDefinedThrowsIfAttributeTypeIsNull()
        {
            // Arrange
            ControllerDescriptor cd = GetControllerDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { cd.IsDefined(null /* attributeType */, true); }, "attributeType");
        }

        private static ControllerDescriptor GetControllerDescriptor()
        {
            return GetControllerDescriptor(null);
        }

        private static ControllerDescriptor GetControllerDescriptor(Type controllerType)
        {
            Mock<ControllerDescriptor> mockDescriptor = new Mock<ControllerDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.ControllerType).Returns(controllerType);
            return mockDescriptor.Object;
        }
    }
}
