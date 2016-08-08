// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class HostNameComparisonModeHelperTest : EnumHelperTestBase<HostNameComparisonMode>
    {
        public HostNameComparisonModeHelperTest()
            : base(HostNameComparisonModeHelper.IsDefined, HostNameComparisonModeHelper.Validate, (HostNameComparisonMode)999)
        {
        }
    }
}
