// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class ViewResult : ViewResultBase
    {
        private string _masterName;

        public string MasterName
        {
            get { return _masterName ?? String.Empty; }
            set { _masterName = value; }
        }

        protected override ViewEngineResult FindView(ControllerContext context)
        {
            ViewEngineResult result = ViewEngineCollection.FindView(context, ViewName, MasterName);
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
                                                              MvcResources.Common_ViewNotFound, ViewName, locationsText));
        }
    }
}
