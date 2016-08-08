// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Razor.Text;

namespace System.Web.Razor.Generator
{
    internal static class CodeWriterExtensions
    {
        public static void WriteLocationTaggedString(this CodeWriter writer, LocationTagged<string> value)
        {
            writer.WriteStartMethodInvoke("Tuple.Create");
            writer.WriteStringLiteral(value.Value);
            writer.WriteParameterSeparator();
            writer.WriteSnippet(value.Location.AbsoluteIndex.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndMethodInvoke();
        }
    }
}
