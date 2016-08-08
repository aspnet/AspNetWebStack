// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    public class TraceLevelHelperTest : EnumHelperTestBase<TraceLevel>
    {
        public TraceLevelHelperTest()
            : base(TraceLevelHelper.IsDefined, TraceLevelHelper.Validate, (TraceLevel)999)
        {
        }
    }
}
