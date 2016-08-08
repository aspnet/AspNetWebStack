// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class MvcControlTest
    {
        [Fact]
        public void AttributesProperty()
        {
            // Setup
            DummyMvcControl c = new DummyMvcControl();

            // Execute
            IDictionary<string, string> attrs = c.Attributes;

            // Verify
            Assert.NotNull(attrs);
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetSetAttributes()
        {
            // Setup
            DummyMvcControl c = new DummyMvcControl();
            IAttributeAccessor attrAccessor = c;
            IDictionary<string, string> attrs = c.Attributes;

            // Execute and Verify
            string value;
            value = attrAccessor.GetAttribute("xyz");
            Assert.Null(value);

            attrAccessor.SetAttribute("a1", "v1");
            value = attrAccessor.GetAttribute("a1");
            Assert.Equal("v1", value);
            Assert.Single(attrs);
            value = c.Attributes["a1"];
            Assert.Equal("v1", value);
        }

        [Fact]
        public void EnableViewStateProperty()
        {
            DummyMvcControl c = new DummyMvcControl();
            Assert.True(c.EnableViewState);
            Assert.True((c).EnableViewState);

            c.EnableViewState = false;
            Assert.False(c.EnableViewState);
            Assert.False((c).EnableViewState);

            c.EnableViewState = true;
            Assert.True(c.EnableViewState);
            Assert.True((c).EnableViewState);
        }

        [Fact]
        public void ViewContextWithNoPageIsNull()
        {
            // Setup
            DummyMvcControl c = new DummyMvcControl();
            Control c1 = new Control();
            c1.Controls.Add(c);

            // Execute
            ViewContext vc = c.ViewContext;

            // Verify
            Assert.Null(vc);
        }

        private sealed class DummyMvcControl : MvcControl
        {
        }
    }
}
