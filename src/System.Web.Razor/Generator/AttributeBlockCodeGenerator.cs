// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Razor.Generator
{
    public class AttributeBlockCodeGenerator : BlockCodeGenerator
    {
        public AttributeBlockCodeGenerator(string name, LocationTagged<string> prefix, LocationTagged<string> suffix)
        {
            Name = name;
            Prefix = prefix;
            Suffix = suffix;
        }

        public string Name { get; private set; }
        public LocationTagged<string> Prefix { get; private set; }
        public LocationTagged<string> Suffix { get; private set; }

        public override void GenerateStartBlockCode(Block target, CodeGeneratorContext context)
        {
            if (context.Host.DesignTimeMode)
            {
                return; // Don't generate anything!
            }
            context.FlushBufferedStatement();
            context.AddStatement(context.BuildCodeString(cw =>
            {
                if (!String.IsNullOrEmpty(context.TargetWriterName))
                {
                    cw.WriteStartMethodInvoke(context.Host.GeneratedClassContext.WriteAttributeToMethodName);
                    cw.WriteSnippet(context.TargetWriterName);
                    cw.WriteParameterSeparator();
                }
                else
                {
                    cw.WriteStartMethodInvoke(context.Host.GeneratedClassContext.WriteAttributeMethodName);
                }
                cw.WriteStringLiteral(Name);
                cw.WriteParameterSeparator();
                cw.WriteLocationTaggedString(Prefix);
                cw.WriteParameterSeparator();
                cw.WriteLocationTaggedString(Suffix);

                // In VB, we need a line continuation
                cw.WriteLineContinuation();
            }));
        }

        public override void GenerateEndBlockCode(Block target, CodeGeneratorContext context)
        {
            if (context.Host.DesignTimeMode)
            {
                return; // Don't generate anything!
            }
            context.FlushBufferedStatement();
            context.AddStatement(context.BuildCodeString(cw =>
            {
                cw.WriteEndMethodInvoke();
                cw.WriteEndStatement();
            }));
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Attr:{0},{1:F},{2:F}", Name, Prefix, Suffix);
        }

        public override bool Equals(object obj)
        {
            AttributeBlockCodeGenerator other = obj as AttributeBlockCodeGenerator;
            return other != null &&
                   String.Equals(other.Name, Name, StringComparison.Ordinal) &&
                   Equals(other.Prefix, Prefix) &&
                   Equals(other.Suffix, Suffix);
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(Name)
                .Add(Prefix)
                .Add(Suffix)
                .CombinedHash;
        }
    }
}
