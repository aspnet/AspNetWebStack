// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc.Filters;

namespace System.Web.Mvc.Test
{
    public class OverrideAuthorizationAttributeTests : OverrideFiltersAttributeTests
    {
        protected override Type ExpectedFiltersToOverride
        {
            get { return typeof(IAuthorizationFilter); }
        }
        
        protected override Type ProductUnderTestType
        {
            get { return typeof(OverrideAuthorizationAttribute); }
        }

        protected override IOverrideFilter CreateProductUnderTest()
        {
            return new OverrideAuthorizationAttribute();
        }
    }
}
