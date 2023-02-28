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

#if Testing_NetStandard1_3 // InvariantCulture and InvariantCultureIgnoreCase case are not supported in netstandard1.3 project
        protected override bool ValueExistsForFramework(StringComparison value)
        {
            return !(value == StringComparison.InvariantCulture || value == StringComparison.InvariantCultureIgnoreCase);
        }
#endif
    }
}
