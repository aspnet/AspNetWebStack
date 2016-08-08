// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionContextCatchBlockTests
    {
        [Fact]
        public void Constructor_IfNameIsNull_Throws()
        {
            // Arrange
            string name = null;
            bool isTopLevel = false;
            bool callsHandler = false;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(name, isTopLevel, callsHandler), "name");
        }

        [Fact]
        public void Name_IsSpecifiedInstance()
        {
            // Arrange
            string expectedName = "TheName";
            bool isTopLevel = true;
            bool callsHandler = false;
            ExceptionContextCatchBlock product = CreateProductUnderTest(expectedName, isTopLevel, callsHandler);

            // Act
            string name = product.Name;

            // Assert
            Assert.Same(expectedName, name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsTopLevel_IsSpecifiedValue(bool expectedIsTopLevel)
        {
            // Arrange
            string name = "IgnoreName";
            bool callsHandler = false;
            ExceptionContextCatchBlock product = CreateProductUnderTest(name, expectedIsTopLevel, callsHandler);

            // Act
            bool isTopLevel = product.IsTopLevel;

            // Assert
            Assert.Equal(expectedIsTopLevel, isTopLevel);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CallsHandler_IsSpecifiedValue(bool expectedCallsHandler)
        {
            // Arrange
            string name = "IgnoreName";
            bool isTopLevel = true;
            ExceptionContextCatchBlock product = CreateProductUnderTest(name, isTopLevel, expectedCallsHandler);

            // Act
            bool callsHandler = product.CallsHandler;

            // Assert
            Assert.Equal(expectedCallsHandler, callsHandler);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            // Arrange
            string expectedName = "TheName";
            bool isTopLevel = false;
            bool callsHandler= false;
            object product = CreateProductUnderTest(expectedName, isTopLevel, callsHandler);

            // Act
            string value = product.ToString();

            // Assert
            Assert.Same(expectedName, value);
        }

        [Fact]
        public void DebuggerDisplayAttribute_IsSpecifiedValue()
        {
            // Act
            DebuggerDisplayAttribute attribute = (DebuggerDisplayAttribute)Attribute.GetCustomAttribute(
                typeof(ExceptionContextCatchBlock), typeof(DebuggerDisplayAttribute));

            // Assert
            Assert.NotNull(attribute);
            string value = attribute.Value;
            Assert.Equal("Name: {Name}, IsTopLevel: {IsTopLevel}", value);
        }

        private static ExceptionContextCatchBlock CreateProductUnderTest(string name, bool isTopLevel, bool callsHandler)
        {
            return new ExceptionContextCatchBlock(name, isTopLevel, callsHandler);
        }
    }
}
