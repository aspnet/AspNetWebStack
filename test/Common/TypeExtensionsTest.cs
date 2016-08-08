// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.TestCommon;

namespace System
{
    public class TypeExtensionsTest
    {
        [Theory]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(int?), true)]
        [InlineData(typeof(IEnumerable), true)]
        [InlineData(typeof(int[]), true)]
        [InlineData(typeof(string[]), true)]
        public void IsNullable_Returns_ExpectedValue(Type type, bool expectedResult)
        {
            Assert.Equal(expectedResult, type.IsNullable());
        }
    }
}
