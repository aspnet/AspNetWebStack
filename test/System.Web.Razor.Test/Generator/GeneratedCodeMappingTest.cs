// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Generator
{
    public class GeneratedCodeMappingTest
    {
        [Fact]
        public void GeneratedCodeMappingsAreEqualIfDataIsEqual()
        {
            GeneratedCodeMapping left = new GeneratedCodeMapping(12, 34, 56, 78);
            GeneratedCodeMapping right = new GeneratedCodeMapping(12, 34, 56, 78);
            Assert.True(left == right);
            Assert.True(left.Equals(right));
            Assert.True(right.Equals(left));
            Assert.True(Equals(left, right));
        }

        [Fact]
        public void GeneratedCodeMappingsAreNotEqualIfCodeLengthIsNotEqual()
        {
            GeneratedCodeMapping left = new GeneratedCodeMapping(12, 34, 56, 87);
            GeneratedCodeMapping right = new GeneratedCodeMapping(12, 34, 56, 78);
            Assert.False(left == right);
            Assert.False(left.Equals(right));
            Assert.False(right.Equals(left));
            Assert.False(Equals(left, right));
        }

        [Fact]
        public void GeneratedCodeMappingsAreNotEqualIfStartGeneratedColumnIsNotEqual()
        {
            GeneratedCodeMapping left = new GeneratedCodeMapping(12, 34, 56, 87);
            GeneratedCodeMapping right = new GeneratedCodeMapping(12, 34, 65, 87);
            Assert.False(left == right);
            Assert.False(left.Equals(right));
            Assert.False(right.Equals(left));
            Assert.False(Equals(left, right));
        }

        [Fact]
        public void GeneratedCodeMappingsAreNotEqualIfStartColumnIsNotEqual()
        {
            GeneratedCodeMapping left = new GeneratedCodeMapping(12, 34, 56, 87);
            GeneratedCodeMapping right = new GeneratedCodeMapping(12, 43, 56, 87);
            Assert.False(left == right);
            Assert.False(left.Equals(right));
            Assert.False(right.Equals(left));
            Assert.False(Equals(left, right));
        }

        [Fact]
        public void GeneratedCodeMappingsAreNotEqualIfStartLineIsNotEqual()
        {
            GeneratedCodeMapping left = new GeneratedCodeMapping(12, 34, 56, 87);
            GeneratedCodeMapping right = new GeneratedCodeMapping(21, 34, 56, 87);
            Assert.False(left == right);
            Assert.False(left.Equals(right));
            Assert.False(right.Equals(left));
            Assert.False(Equals(left, right));
        }
    }
}