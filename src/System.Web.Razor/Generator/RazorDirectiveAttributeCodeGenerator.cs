// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.Razor.Parser.SyntaxTree;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Razor.Generator
{
    public class RazorDirectiveAttributeCodeGenerator : SpanCodeGenerator
    {
        public RazorDirectiveAttributeCodeGenerator(string name, string value)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            Name = name;
            Value = value ?? String.Empty; // Coerce to empty string if it was null.
        }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            var attributeType = new CodeTypeReference(typeof(RazorDirectiveAttribute));
            var attributeDeclaration = new CodeAttributeDeclaration(
                attributeType,
                new CodeAttributeArgument(new CodePrimitiveExpression(Name)),
                new CodeAttributeArgument(new CodePrimitiveExpression(Value)));
            context.GeneratedClass.CustomAttributes.Add(attributeDeclaration);
        }

        public override string ToString()
        {
            return "Directive: " + Name + ", Value: " + Value;
        }

        public override bool Equals(object obj)
        {
            RazorDirectiveAttributeCodeGenerator other = obj as RazorDirectiveAttributeCodeGenerator;
            return other != null &&
                   Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                   Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Name.ToUpperInvariant(), Value.ToUpperInvariant())
                .GetHashCode();
        }
    }
}
