// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace Microsoft.Web.WebPages.OAuth.Test
{
    public class PreAppStartCodeTest
    {
        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
