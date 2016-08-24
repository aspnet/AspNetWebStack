// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    // Xunit.MemberDataAttribute is unfortunately sealed. Duplicate its code here since method we need is protected.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    public class PropertyDataAttribute : MemberDataAttributeBase
    {
        public PropertyDataAttribute(string propertyName, params object[] parameters)
            : base(propertyName, parameters)
        {
        }

        public Type PropertyType
        {
            get
            {
                return MemberType;
            }
            set
            {
                MemberType = value;
            }
        }

        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            if (item == null)
            {
                return null;
            }

            var array = item as object[];
            if (array == null)
            {
                throw new ArgumentException(String.Format(
                    "Property {0} on {1} yielded an item that is not an object[].",
                    MemberName,
                    MemberType ?? testMethod.DeclaringType));
            }

            return array;
        }
    }
}
