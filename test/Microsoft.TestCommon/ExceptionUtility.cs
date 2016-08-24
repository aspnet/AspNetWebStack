// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

// Variation of Xunit.Sdk.ExceptionUtility which helps with Exceptions and their stack traces.
namespace Microsoft.TestCommon
{
    public static class ExceptionUtility
    {
        public static string FilterStackTrace(string stack)
        {
            if (stack == null)
            {
                return null;
            }

            var results = new List<string>();
            foreach (var line in SplitLines(stack))
            {
                var trimmedLine = line.TrimStart();
                if (!ExcludeStackFrame(trimmedLine))
                {
                    results.Add(line);
                }
            }

            return string.Join(Environment.NewLine, results);
        }

        private static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                var index = input.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (index < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, index);
                input = input.Substring(index + Environment.NewLine.Length);
            }
        }

        private static bool ExcludeStackFrame(string stackFrame)
        {
            if (stackFrame.StartsWith("at Microsoft.TestCommon.Assert.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
