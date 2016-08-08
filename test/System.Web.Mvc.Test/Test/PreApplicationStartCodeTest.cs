// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class PreApplicationStartCodeTest
    {
        [Fact]
        public void PreApplicationStartCodeIsNotBrowsableTest()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }

        [Fact]
        public void PreApplicationStartMethodAttributeTest()
        {
            Assembly assembly = typeof(Controller).Assembly;
            object[] attributes = assembly.GetCustomAttributes(typeof(PreApplicationStartMethodAttribute), true);
            var preAppStartMethodAttribute = Assert.Single(attributes.Cast<PreApplicationStartMethodAttribute>());
            Type preAppStartMethodType = preAppStartMethodAttribute.Type;
            Assert.Equal(typeof(PreApplicationStartCode), preAppStartMethodType);
        }
    }
}
