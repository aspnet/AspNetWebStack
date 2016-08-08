// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class RouteAttributeTests
    {
        [Fact]
        public void Template_EmptyString_Ok()
        {
            new RouteAttribute("");
        }

        [Fact]
        public void Template_Null_NotOk()
        {
            Assert.Throws<ArgumentNullException>(() => new RouteAttribute(null));
        }

        [Fact]
        public void EmptyCtor_Is_EmptyString()
        {
            RouteAttribute attr = new RouteAttribute();

            Assert.Equal(String.Empty, attr.Template);
        }
    }
}