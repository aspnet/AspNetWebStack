// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    // Xunit.MemberDataAttribute is unfortunately sealed. Duplicate its code here since there's nothing to it.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
    public class TraitAttribute : Attribute, ITraitAttribute
    {
        public TraitAttribute(string name, string value)
        {
        }
    }
}
