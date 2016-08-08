// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.WebPages;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    public static class ObjectInfo
    {
        private const int DefaultRecursionLimit = 10;
        private const int DefaultEnumerationLimit = 1000;

        public static HelperResult Print(object value, int depth = DefaultRecursionLimit, int enumerationLength = DefaultEnumerationLimit)
        {
            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "depth",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }
            if (enumerationLength <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "enumerationLength",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThan, 0));
            }

            HtmlObjectPrinter printer = new HtmlObjectPrinter(depth, enumerationLength);
            return new HelperResult(writer => printer.WriteTo(value, writer));
        }
    }
}
