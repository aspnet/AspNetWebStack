// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    public static class ImageExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl)
        {
            return Image(helper, imageRelativeUrl, null, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Required for Extension Method")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl, string alt)
        {
            return Image(helper, imageRelativeUrl, alt, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl, string alt, object htmlAttributes)
        {
            return Image(helper, imageRelativeUrl, alt, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl, object htmlAttributes)
        {
            return Image(helper, imageRelativeUrl, null, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl, IDictionary<string, object> htmlAttributes)
        {
            return Image(helper, imageRelativeUrl, null, htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static MvcHtmlString Image(this HtmlHelper helper, string imageRelativeUrl, string alt, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(imageRelativeUrl))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "imageRelativeUrl");
            }

            string imageUrl = UrlHelper.GenerateContentUrl(imageRelativeUrl, helper.ViewContext.HttpContext);
            return MvcHtmlString.Create(Image(imageUrl, alt, htmlAttributes).ToString(TagRenderMode.SelfClosing));
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "The value is a not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public static TagBuilder Image(string imageUrl, string alt, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "imageUrl");
            }

            TagBuilder imageTag = new TagBuilder("img");

            if (!String.IsNullOrEmpty(imageUrl))
            {
                imageTag.MergeAttribute("src", imageUrl);
            }

            if (!String.IsNullOrEmpty(alt))
            {
                imageTag.MergeAttribute("alt", alt);
            }

            imageTag.MergeAttributes(htmlAttributes, true);

            if (imageTag.Attributes.ContainsKey("alt") && !imageTag.Attributes.ContainsKey("title"))
            {
                imageTag.MergeAttribute("title", (imageTag.Attributes["alt"] ?? String.Empty));
            }
            return imageTag;
        }
    }
}
