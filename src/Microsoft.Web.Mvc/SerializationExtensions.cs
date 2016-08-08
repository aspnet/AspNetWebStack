// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    public static class SerializationExtensions
    {
        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name)
        {
            return SerializeInternal(htmlHelper, name, null, useViewData: true);
        }

        internal static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, MvcSerializer serializer)
        {
            return SerializeInternal(htmlHelper, name, null, useViewData: true, serializer: serializer);
        }

        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, object data)
        {
            return SerializeInternal(htmlHelper, name, data, useViewData: false);
        }

        internal static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, object data, MvcSerializer serializer)
        {
            return SerializeInternal(htmlHelper, name, data, useViewData: false, serializer: serializer);
        }

        private static MvcHtmlString SerializeInternal(HtmlHelper htmlHelper, string name, object data, bool useViewData)
        {
            return SerializeInternal(htmlHelper, name, data, useViewData, null);
        }

        private static MvcHtmlString SerializeInternal(HtmlHelper htmlHelper, string name, object data, bool useViewData, MvcSerializer serializer)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException("htmlHelper");
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            name = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (useViewData)
            {
                data = htmlHelper.ViewData.Eval(name);
            }

            string serializedData = (serializer ?? new MvcSerializer()).Serialize(data);

            TagBuilder builder = new TagBuilder("input");
            builder.Attributes["type"] = "hidden";
            builder.Attributes["name"] = name;
            builder.Attributes["value"] = serializedData;
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}
