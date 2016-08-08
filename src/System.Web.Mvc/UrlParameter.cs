// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public sealed class UrlParameter
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly UrlParameter Optional = new UrlParameter();

        // singleton constructor
        private UrlParameter()
        {
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }
}
