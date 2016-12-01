// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    // Xunit.InlineDataAttribute is unfortunately sealed. Delegate to an instance to avoid duplicating its code.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DataDiscoverer("Xunit.Sdk.InlineDataDiscoverer", "xunit.core")]
    public class InlineDataAttribute : Xunit.Sdk.DataAttribute
    {
        Xunit.InlineDataAttribute _inner;

        public InlineDataAttribute(params object[] dataValues)
            : base()
        {
            _inner = new Xunit.InlineDataAttribute(dataValues);
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return _inner.GetData(testMethod);
        }
    }
}
