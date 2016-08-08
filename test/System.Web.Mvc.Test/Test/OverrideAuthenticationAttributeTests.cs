// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Filters;

namespace System.Web.Mvc.Test
{
    public class OverrideAuthenticationAttributeTests : OverrideFiltersAttributeTests
    {
        protected override Type ExpectedFiltersToOverride
        {
            get { return typeof(IAuthenticationFilter); }
        }
        
        protected override Type ProductUnderTestType
        {
            get { return typeof(OverrideAuthenticationAttribute); }
        }

        protected override IOverrideFilter CreateProductUnderTest()
        {
            return new OverrideAuthenticationAttribute();
        }
    }
}
