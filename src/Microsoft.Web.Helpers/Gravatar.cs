// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using Microsoft.Internal.Web.Utils;
using Resources;

namespace Microsoft.Web.Helpers
{
    public static class Gravatar
    {
        private const string GravatarUrl = "http://www.gravatar.com/avatar/";

        // review - extract conversion of anonymous object to html attributes string into separate helper
        public static HtmlString GetHtml(string email, int imageSize = 80, string defaultImage = null,
                                         GravatarRating rating = GravatarRating.Default, string imageExtension = null, object attributes = null)
        {
            bool altSpecified = false;
            string url = GetUrl(email, imageSize, defaultImage, rating, imageExtension);
            StringBuilder html = new StringBuilder(String.Format(CultureInfo.InvariantCulture, "<img src=\"{0}\" ", url));
            if (attributes != null)
            {
                foreach (var p in attributes.GetType().GetProperties().OrderBy(p => p.Name))
                {
                    if (!p.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        object value = p.GetValue(attributes, null);
                        if (value != null)
                        {
                            string encodedValue = HttpUtility.HtmlAttributeEncode(value.ToString());
                            html.Append(String.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", p.Name, encodedValue));
                        }
                        if (p.Name.Equals("alt", StringComparison.OrdinalIgnoreCase))
                        {
                            altSpecified = true;
                        }
                    }
                }
            }
            if (!altSpecified)
            {
                html.Append("alt=\"gravatar\" ");
            }
            html.Append("/>");
            return new HtmlString(html.ToString());
        }

        // See: http://en.gravatar.com/site/implement/url
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Strings are easier to work with for Plan9 scenario")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Gravatar.com requires lowercase")]
        public static string GetUrl(string email, int imageSize = 80, string defaultImage = null,
                                    GravatarRating rating = GravatarRating.Default, string imageExtension = null)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "email");
            }
            if ((imageSize <= 0) || (imageSize > 512))
            {
                throw new ArgumentException(HelpersToolkitResources.Gravatar_InvalidImageSize, "imageSize");
            }

            StringBuilder url = new StringBuilder(GravatarUrl);
            email = email.Trim().ToLowerInvariant();
            url.Append(Crypto.Hash(email, algorithm: "md5").ToLowerInvariant());

            if (!String.IsNullOrEmpty(imageExtension))
            {
                if (!imageExtension.StartsWith(".", StringComparison.Ordinal))
                {
                    url.Append('.');
                }
                url.Append(imageExtension);
            }

            url.Append("?s=");
            url.Append(imageSize);

            if (rating != GravatarRating.Default)
            {
                url.Append("&r=");
                url.Append(rating.ToString().ToLowerInvariant());
            }

            if (!String.IsNullOrEmpty(defaultImage))
            {
                url.Append("&d=");
                url.Append(HttpUtility.UrlEncode(defaultImage));
            }

            return HttpUtility.HtmlAttributeEncode(url.ToString());
        }
    }
}
