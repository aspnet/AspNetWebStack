// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Text;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Text
{
    public class SourceLocationTest
    {
        [Fact]
        public void ConstructorWithLineAndCharacterIndexSetsAssociatedProperties()
        {
            // Act
            SourceLocation loc = new SourceLocation(0, 42, 24);

            // Assert
            Assert.Equal(0, loc.AbsoluteIndex);
            Assert.Equal(42, loc.LineIndex);
            Assert.Equal(24, loc.CharacterIndex);
        }
    }
}
