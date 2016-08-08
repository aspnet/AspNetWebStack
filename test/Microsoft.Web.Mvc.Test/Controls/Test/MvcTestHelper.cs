// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public static class MvcTestHelper
    {
        public static string GetControlRendering(Control c, bool designMode)
        {
            if (designMode)
            {
                c.Site = new DesignModeSite();
            }
            HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
            c.RenderControl(writer);
            return writer.InnerWriter.ToString();
        }
    }
}
