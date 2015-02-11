// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            Assert.Equal(new Version(5, 2, 4, 0), mvcVersion);
        }
    }
}
