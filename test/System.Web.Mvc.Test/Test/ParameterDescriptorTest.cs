// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ParameterDescriptorTest
    {
        [Fact]
        public void BindingInfoProperty()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor(typeof(object), "someName");

            // Act
            ParameterBindingInfo bindingInfo = pd.BindingInfo;

            // Assert
            Assert.IsType(typeof(ParameterDescriptor).GetNestedType("EmptyParameterBindingInfo", BindingFlags.NonPublic),
                          bindingInfo);
        }

        [Fact]
        public void DefaultValuePropertyDefaultsToNull()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor();

            // Act
            object defaultValue = pd.DefaultValue;

            // Assert
            Assert.Null(defaultValue);
        }

        [Fact]
        public void GetCustomAttributesReturnsEmptyArrayOfAttributeType()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor();

            // Act
            ObsoleteAttribute[] attrs = (ObsoleteAttribute[])pd.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetCustomAttributesThrowsIfAttributeTypeIsNull()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { pd.GetCustomAttributes(null /* attributeType */, true); }, "attributeType");
        }

        [Fact]
        public void GetCustomAttributesWithoutAttributeTypeCallsGetCustomAttributesWithAttributeType()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<ParameterDescriptor> mockDescriptor = new Mock<ParameterDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.GetCustomAttributes(typeof(object), true)).Returns(expected);
            ParameterDescriptor pd = mockDescriptor.Object;

            // Act
            object[] returned = pd.GetCustomAttributes(true /* inherit */);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void IsDefinedReturnsFalse()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor();

            // Act
            bool isDefined = pd.IsDefined(typeof(object), true);

            // Assert
            Assert.False(isDefined);
        }

        [Fact]
        public void IsDefinedThrowsIfAttributeTypeIsNull()
        {
            // Arrange
            ParameterDescriptor pd = GetParameterDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { pd.IsDefined(null /* attributeType */, true); }, "attributeType");
        }

        private static ParameterDescriptor GetParameterDescriptor()
        {
            return GetParameterDescriptor(typeof(object), "someName");
        }

        private static ParameterDescriptor GetParameterDescriptor(Type type, string name)
        {
            Mock<ParameterDescriptor> mockDescriptor = new Mock<ParameterDescriptor>() { CallBase = true };
            mockDescriptor.Setup(d => d.ParameterType).Returns(type);
            mockDescriptor.Setup(d => d.ParameterName).Returns(name);
            return mockDescriptor.Object;
        }
    }
}
