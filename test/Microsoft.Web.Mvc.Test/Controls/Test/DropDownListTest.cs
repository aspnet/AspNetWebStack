// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class DropDownListTest
    {
        [Fact]
        public void NameProperty()
        {
            // TODO: This
        }

        [Fact]
        public void RenderWithNoNameNotInDesignModeThrows()
        {
            // TODO: This
        }

        [Fact]
        public void RenderWithNoNameInDesignModeRendersWithSampleData()
        {
            // Setup
            DropDownList c = new DropDownList();

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, true);

            // Verify
            Assert.Equal("<select>" + Environment.NewLine
                       + "\t<option>" + Environment.NewLine
                       + "\t\tSample Item" + Environment.NewLine
                       + "\t</option>" + Environment.NewLine
                       + "</select>",
                         html);
        }

        [Fact]
        public void RenderWithNoAttributes()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(new[] { "aaa", "bbb", "ccc" }, "bbb");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal("<select name=\"nameKey\">" + Environment.NewLine
                       + "\t<option>" + Environment.NewLine
                       + "\t\taaa" + Environment.NewLine
                       + "\t</option><option selected=\"selected\">" + Environment.NewLine
                       + "\t\tbbb" + Environment.NewLine
                       + "\t</option><option>" + Environment.NewLine
                       + "\t\tccc" + Environment.NewLine
                       + "\t</option>" + Environment.NewLine
                       + "</select>",
                         html);
        }

        [Fact]
        public void RenderWithTextsAndValues()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(
                new[]
                {
                    new { Text = "aaa", Value = "111" },
                    new { Text = "bbb", Value = "222" },
                    new { Text = "ccc", Value = "333" }
                },
                "Value",
                "Text",
                "222");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal("<select name=\"nameKey\">" + Environment.NewLine
                       + "\t<option value=\"111\">" + Environment.NewLine
                       + "\t\taaa" + Environment.NewLine
                       + "\t</option><option value=\"222\" selected=\"selected\">" + Environment.NewLine
                       + "\t\tbbb" + Environment.NewLine
                       + "\t</option><option value=\"333\">" + Environment.NewLine
                       + "\t\tccc" + Environment.NewLine
                       + "\t</option>" + Environment.NewLine
                       + "</select>", html);
        }

        [Fact]
        public void RenderWithNameAndIdRendersNameAndIdAttribute()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";
            c.ID = "someID";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(new[] { "aaa", "bbb", "ccc" }, "bbb");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal("<select id=\"someID\" name=\"nameKey\">" + Environment.NewLine
                       + "\t<option>" + Environment.NewLine
                       + "\t\taaa" + Environment.NewLine
                       + "\t</option><option selected=\"selected\">" + Environment.NewLine
                       + "\t\tbbb" + Environment.NewLine
                       + "\t</option><option>" + Environment.NewLine
                       + "\t\tccc" + Environment.NewLine
                       + "\t</option>" + Environment.NewLine
                       + "</select>",
                         html);
        }
    }
}
