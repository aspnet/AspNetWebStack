// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProvider : NameValuePairsValueProvider
    {
        public QueryStringValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(actionContext.ControllerContext.Request.GetQueryNameValuePairs(), culture)
        {
        }
    }
}
