// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class StringComparisonHelperTest : EnumHelperTestBase<StringComparison>
    {
        public StringComparisonHelperTest()
            : base(StringComparisonHelper.IsDefined, StringComparisonHelper.Validate, (StringComparison)999)
        {
        }

#if NETFX_CORE // InvariantCulture and InvarianteCultureIgnore case are not supported in portable library projects
        protected override void AssertForUndefinedValue(Action testCode, string parameterName, int invalidValue, Type enumType, bool allowDerivedExceptions = false)
        {
            Assert.ThrowsArgument(
                testCode,
                parameterName,
                allowDerivedExceptions);
        }

        protected override bool ValueExistsForFramework(StringComparison value)
        {
            return !(value == StringComparison.InvariantCulture || value == StringComparison.InvariantCultureIgnoreCase);
        }
#endif
    }
}
