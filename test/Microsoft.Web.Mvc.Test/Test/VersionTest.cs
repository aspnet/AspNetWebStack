// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Test
{
    public class VersionTest
    {
        [Fact]
        public void VerifyMVCVersionChangesAreIntentional()
        {
            Version mvcVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Mvc", typeof(Controller));
            Assert.Equal(new Version(5, 3, 0, 0), mvcVersion);
        }
    }
}
