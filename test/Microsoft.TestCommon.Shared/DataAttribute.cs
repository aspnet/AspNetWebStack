// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class DataAttribute : Xunit.Sdk.DataAttribute
    {
        public abstract IEnumerable<Object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes);

        public override IEnumerable<Object[]> GetData(MethodInfo methodUnderTest)
        {
            return GetData(methodUnderTest, parameterTypes: null);
        }
    }
}
