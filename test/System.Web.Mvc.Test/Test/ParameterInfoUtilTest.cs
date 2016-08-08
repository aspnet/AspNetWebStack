// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ParameterInfoUtilTest
    {
        [Fact]
        public void TryGetDefaultValue_FirstChecksDefaultValue()
        {
            // Arrange
            Mock<ParameterInfo> mockPInfo = new Mock<ParameterInfo>() { DefaultValue = DefaultValue.Mock };
            mockPInfo.Setup(p => p.DefaultValue).Returns(42);
            mockPInfo.Setup(p => p.Name).Returns("someParameter");

            // Act
            object defaultValue;
            bool retVal = ParameterInfoUtil.TryGetDefaultValue(mockPInfo.Object, out defaultValue);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, defaultValue);
        }

        [Fact]
        public void TryGetDefaultValue_SecondChecksDefaultValueAttribute()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("DefaultValues").GetParameters()[1]; // hasDefaultValue

            // Act
            object defaultValue;
            bool retVal = ParameterInfoUtil.TryGetDefaultValue(pInfo, out defaultValue);

            // Assert
            Assert.True(retVal);
            Assert.Equal("someValue", defaultValue);
        }

        [Fact]
        public void TryGetDefaultValue_RespectsNullDefaultValue()
        {
            // Arrange
            Mock<ParameterInfo> mockPInfo = new Mock<ParameterInfo>() { DefaultValue = DefaultValue.Mock };
            mockPInfo.Setup(p => p.DefaultValue).Returns(null);
            mockPInfo.Setup(p => p.Name).Returns("someParameter");
            mockPInfo
                .Setup(p => p.GetCustomAttributes(typeof(DefaultValueAttribute[]), false))
                .Returns(new DefaultValueAttribute[] { new DefaultValueAttribute(42) });

            // Act
            object defaultValue;
            bool retVal = ParameterInfoUtil.TryGetDefaultValue(mockPInfo.Object, out defaultValue);

            // Assert
            Assert.True(retVal);
            Assert.Null(defaultValue);
        }

        [Fact]
        public void TryGetDefaultValue_ReturnsFalseIfNoDefaultValue()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("DefaultValues").GetParameters()[0]; // noDefaultValue

            // Act
            object defaultValue;
            bool retVal = ParameterInfoUtil.TryGetDefaultValue(pInfo, out defaultValue);

            // Assert
            Assert.False(retVal);
            Assert.Equal(default(object), defaultValue);
        }

        [Fact]
        public void TryGetDefaultValue_DefaultValueAttributeParameters()
        {
            DefaultValueAttributeHelper<bool>(true, "boolParam");
            DefaultValueAttributeHelper<byte>(42, "byteParam");
            DefaultValueAttributeHelper<char>('a', "charParam");
            DefaultValueAttributeHelper<double>(1.0, "doubleParam");
            DefaultValueAttributeHelper<MyEnum>(MyEnum.All, "enumParam");
            DefaultValueAttributeHelper<float>((float)1.0, "floatParam");
            DefaultValueAttributeHelper<int>(42, "intParam");
            DefaultValueAttributeHelper<long>(42, "longParam");
            DefaultValueAttributeHelper<object>(null, "objectParam");
            DefaultValueAttributeHelper<short>(42, "shortParam");
            DefaultValueAttributeHelper<string>("abc", "stringParam");
            DefaultValueAttributeHelper<DateTime>(new DateTime(2010, 09, 27), "customParam");
        }

        [Fact]
        public void TryGetDefaultValue_OptionalParameters()
        {
            OptionalParamHelper<bool>(true, "boolParam");
            OptionalParamHelper<byte>(42, "byteParam");
            OptionalParamHelper<char>('a', "charParam");
            OptionalParamHelper<double>(1.0, "doubleParam");
            OptionalParamHelper<MyEnum>(MyEnum.All, "enumParam");
            OptionalParamHelper<float>((float)1.0, "floatParam");
            OptionalParamHelper<int>(42, "intParam");
            OptionalParamHelper<long>(42, "longParam");
            OptionalParamHelper<object>(null, "objectParam");
            OptionalParamHelper<short>(42, "shortParam");
            OptionalParamHelper<string>("abc", "stringParam");
        }

        private static void DefaultValueAttributeHelper<TParam>(TParam expectedValue, string paramName)
        {
            ParameterTestHelper<TParam>(expectedValue, paramName, "AttributeDefaultValues");
        }

        private static void OptionalParamHelper<TParam>(TParam expectedValue, string paramName)
        {
            ParameterTestHelper<TParam>(expectedValue, paramName, "OptionalParamDefaultValues");
        }

        private static void ParameterTestHelper<TParam>(TParam expectedValue, string paramName, string actionMethodName)
        {
            ParameterInfo pInfo = typeof(MyController).GetMethod(actionMethodName).GetParameters().Single(p => p.Name == paramName);
            object returnValueObject;
            bool result = ParameterInfoUtil.TryGetDefaultValue(pInfo, out returnValueObject);

            Assert.True(result);
            if (expectedValue != null)
            {
                Assert.IsType<TParam>(returnValueObject);
            }
            TParam returnValue = (TParam)returnValueObject;
            Assert.Equal<TParam>(expectedValue, returnValue);
        }

        private class MyController : Controller
        {
            public void DefaultValues(string noDefaultValue, [DefaultValue("someValue")] string hasDefaultValue)
            {
            }

            public void AttributeDefaultValues(
                [DefaultValue(true)] bool boolParam,
                [DefaultValue((byte)42)] byte byteParam,
                [DefaultValue('a')] char charParam,
                [DefaultValue(typeof(DateTime), "2010-09-27")] object customParam,
                [DefaultValue((double)1.0)] double doubleParam,
                [DefaultValue(MyEnum.All)] MyEnum enumParam,
                [DefaultValue((float)1.0)] float floatParam,
                [DefaultValue(42)] int intParam,
                [DefaultValue((long)42)] long longParam,
                [DefaultValue(null)] object objectParam,
                [DefaultValue((short)42)] short shortParam,
                [DefaultValue("abc")] string stringParam
                )
            {
            }

            public void OptionalParamDefaultValues(
                bool boolParam = true,
                byte byteParam = 42,
                char charParam = 'a',
                double doubleParam = 1.0,
                MyEnum enumParam = MyEnum.All,
                float floatParam = (float)1.0,
                int intParam = 42,
                long longParam = 42,
                object objectParam = null,
                short shortParam = 42,
                string stringParam = "abc"
                )
            {
            }
        }

        private enum MyEnum
        {
            None = 0,
            All = 1
        }
    }
}
