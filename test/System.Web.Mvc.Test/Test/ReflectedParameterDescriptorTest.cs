// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ReflectedParameterDescriptorTest
    {
        [Fact]
        public void ConstructorSetsActionDescriptorProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("Foo").GetParameters()[0];
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;

            // Act
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(pInfo, ad);

            // Assert
            Assert.Same(ad, pd.ActionDescriptor);
        }

        [Fact]
        public void ConstructorSetsParameterInfo()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("Foo").GetParameters()[0];

            // Act
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(pInfo, new Mock<ActionDescriptor>().Object);

            // Assert
            Assert.Same(pInfo, pd.ParameterInfo);
        }

        [Fact]
        public void ConstructorThrowsIfActionDescriptorIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedParameterDescriptor(new Mock<ParameterInfo>().Object, null); }, "actionDescriptor");
        }

        [Fact]
        public void ConstructorThrowsIfParameterInfoIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedParameterDescriptor(null, new Mock<ActionDescriptor>().Object); }, "parameterInfo");
        }

        [Fact]
        public void DefaultValuePropertyDefaultsToNull()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("DefaultValues").GetParameters()[0]; // noDefaultValue

            // Act
            ReflectedParameterDescriptor pd = GetParameterDescriptor(pInfo);

            // Assert
            Assert.Null(pd.DefaultValue);
        }

        [Fact]
        public void GetCustomAttributesCallsParameterInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<ParameterInfo> mockParameter = new Mock<ParameterInfo>();
            mockParameter.Setup(pi => pi.Member).Returns(new Mock<MemberInfo>().Object);
            mockParameter.Setup(pi => pi.GetCustomAttributes(true)).Returns(expected);
            ReflectedParameterDescriptor pd = GetParameterDescriptor(mockParameter.Object);

            // Act
            object[] returned = pd.GetCustomAttributes(true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetCustomAttributesWithAttributeTypeCallsParameterInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<ParameterInfo> mockParameter = new Mock<ParameterInfo>();
            mockParameter.Setup(pi => pi.Member).Returns(new Mock<MemberInfo>().Object);
            mockParameter.Setup(pi => pi.GetCustomAttributes(typeof(ObsoleteAttribute), true)).Returns(expected);
            ReflectedParameterDescriptor pd = GetParameterDescriptor(mockParameter.Object);

            // Act
            object[] returned = pd.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void IsDefinedCallsParameterInfoIsDefined()
        {
            // Arrange
            Mock<ParameterInfo> mockParameter = new Mock<ParameterInfo>();
            mockParameter.Setup(pi => pi.Member).Returns(new Mock<MemberInfo>().Object);
            mockParameter.Setup(pi => pi.IsDefined(typeof(ObsoleteAttribute), true)).Returns(true);
            ReflectedParameterDescriptor pd = GetParameterDescriptor(mockParameter.Object);

            // Act
            bool isDefined = pd.IsDefined(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.True(isDefined);
        }

        [Fact]
        public void ParameterNameProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("Foo").GetParameters()[0];

            // Act
            ReflectedParameterDescriptor pd = GetParameterDescriptor(pInfo);

            // Assert
            Assert.Equal("s1", pd.ParameterName);
        }

        [Fact]
        public void ParameterTypeProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("Foo").GetParameters()[0];

            // Act
            ReflectedParameterDescriptor pd = GetParameterDescriptor(pInfo);

            // Assert
            Assert.Equal(typeof(string), pd.ParameterType);
        }

        private static ReflectedParameterDescriptor GetParameterDescriptor(ParameterInfo parameterInfo)
        {
            return new ReflectedParameterDescriptor(parameterInfo, new Mock<ActionDescriptor>().Object);
        }

        private class MyController : Controller
        {
            public void Foo(string s1)
            {
            }

            public void DefaultValues(string noDefaultValue, [DefaultValue("someValue")] string hasDefaultValue)
            {
            }
        }
    }
}
