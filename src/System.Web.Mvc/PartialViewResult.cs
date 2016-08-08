// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class PartialViewResult : ViewResultBase
    {
        protected override ViewEngineResult FindView(ControllerContext context)
        {
            ViewEngineResult result = ViewEngineCollection.FindPartialView(context, ViewName);
            if (result.View != null)
            {
                return result;
            }

            // we need to generate an exception containing all the locations we searched
            StringBuilder locationsText = new StringBuilder();
            foreach (string location in result.SearchedLocations)
            {
                locationsText.AppendLine();
                locationsText.Append(location);
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                              MvcResources.Common_PartialViewNotFound, ViewName, locationsText));
        }
    }
}
