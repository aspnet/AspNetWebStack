// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.TestCommon
{
    public class ForceGCAttribute : Xunit.Sdk.BeforeAfterTestAttribute
    {
        public override void After(MethodInfo methodUnderTest)
        {
            GC.Collect(99);
            GC.Collect(99);
            GC.Collect(99);
        }
    }
}
