// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public sealed class MvcHtmlString : HtmlString
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MvcHtmlString is immutable")]
        public static readonly MvcHtmlString Empty = Create(String.Empty);

        private readonly string _value;

        public MvcHtmlString(string value)
            : base(value ?? String.Empty)
        {
            _value = value ?? String.Empty;
        }

        public static MvcHtmlString Create(string value)
        {
            return new MvcHtmlString(value);
        }

        public static bool IsNullOrEmpty(MvcHtmlString value)
        {
            return (value == null || value._value.Length == 0);
        }
    }
}
