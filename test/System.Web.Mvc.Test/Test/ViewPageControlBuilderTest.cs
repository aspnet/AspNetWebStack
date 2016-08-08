// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ViewPageControlBuilderTest
    {
        [Fact]
        public void BuilderWithoutInheritsDoesNothing()
        {
            // Arrange
            var builder = new ViewPageControlBuilder();
            var derivedType = new CodeTypeDeclaration();
            derivedType.BaseTypes.Add("basetype");

            // Act
            builder.ProcessGeneratedCode(null, null, derivedType, null, null);

            // Assert
            Assert.Equal("basetype", derivedType.BaseTypes.Cast<CodeTypeReference>().Single().BaseType);
        }

        [Fact]
        public void BuilderWithInheritsSetsBaseType()
        {
            // Arrange
            var builder = new ViewPageControlBuilder { Inherits = "inheritedtype" };
            var derivedType = new CodeTypeDeclaration();
            derivedType.BaseTypes.Add("basetype");

            // Act
            builder.ProcessGeneratedCode(null, null, derivedType, null, null);

            // Assert
            Assert.Equal("inheritedtype", derivedType.BaseTypes.Cast<CodeTypeReference>().Single().BaseType);
        }
    }
}
