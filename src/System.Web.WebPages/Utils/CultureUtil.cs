// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace System.Web.WebPages
{
    internal static class CultureUtil
    {
        internal static void SetCulture(Thread thread, HttpContextBase context, string cultureName)
        {
            Debug.Assert(!String.IsNullOrEmpty(cultureName));
            CultureInfo cultureInfo = GetCulture(context, cultureName);
            if (cultureInfo != null)
            {
                thread.CurrentCulture = cultureInfo;
            }
        }

        internal static void SetUICulture(Thread thread, HttpContextBase context, string cultureName)
        {
            Debug.Assert(!String.IsNullOrEmpty(cultureName));
            CultureInfo cultureInfo = GetCulture(context, cultureName);
            if (cultureInfo != null)
            {
                thread.CurrentUICulture = cultureInfo;
            }
        }

        private static CultureInfo GetCulture(HttpContextBase context, string cultureName)
        {
            if (cultureName.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return DetermineAutoCulture(context);
            }
            else
            {
                return CultureInfo.GetCultureInfo(cultureName);
            }
        }

        private static CultureInfo DetermineAutoCulture(HttpContextBase context)
        {
            HttpRequestBase request = context.Request;
            Debug.Assert(request != null); //This call is made from a WebPageExecutingBase. Request can never be null when inside a page.
            CultureInfo culture = null;

            if (request.UserLanguages != null)
            {
                string userLanguageEntry = request.UserLanguages.FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(userLanguageEntry))
                {
                    // Check if user language has q parameter. E.g. something like this: "as-IN;q=0.3"
                    int index = userLanguageEntry.IndexOf(';');
                    if (index != -1)
                    {
                        userLanguageEntry = userLanguageEntry.Substring(0, index);
                    }

                    try
                    {
                        culture = new CultureInfo(userLanguageEntry);
                    }
                    catch (CultureNotFoundException)
                    {
                        // There is no easy way to ask if a given culture is invalid so we have to handle exception.  
                    }
                }
            }
            return culture;
        }
    }
}
