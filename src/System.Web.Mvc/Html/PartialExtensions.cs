// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;

namespace System.Web.Mvc.Html
{
    public static class PartialExtensions
    {
        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines);
                return MvcHtmlString.Create(writer.ToString());
            }
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, Type[] genericTypes)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, genericTypes);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData, Type[] genericTypes)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, genericTypes);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model, Type[] genericTypes)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, genericTypes);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData, Type[] genericTypes)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, genericTypes);
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        //----------------------- generic methods -----------------------
        #region generic methods
        public static MvcHtmlString Partial<T>(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, new Type[] { typeof(T) });
        }

        public static MvcHtmlString Partial<T>(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, new Type[] { typeof(T) });
        }

        public static MvcHtmlString Partial<T>(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, new Type[] { typeof(T) });
        }

        public static MvcHtmlString Partial<T>(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, new Type[] { typeof(T) });
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        // ------------------ Generic: 2 parameters
        public static MvcHtmlString Partial<T1, T2>(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2) });
        }

        public static MvcHtmlString Partial<T1, T2>(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, new Type[] { typeof(T1), typeof(T2) });
        }

        public static MvcHtmlString Partial<T1, T2>(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2) });
        }

        public static MvcHtmlString Partial<T1, T2>(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, new Type[] { typeof(T1), typeof(T2) });
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        // ------------------ Generic: 3 parameters
        public static MvcHtmlString Partial<T1, T2, T3>(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        public static MvcHtmlString Partial<T1, T2, T3>(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        public static MvcHtmlString Partial<T1, T2, T3>(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        public static MvcHtmlString Partial<T1, T2, T3>(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        // ------------------ Generic: 4 parameters
        public static MvcHtmlString Partial<T1, T2, T3, T4>(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4>(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4>(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4>(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        // ------------------ Generic: 5 parameters
        public static MvcHtmlString Partial<T1, T2, T3, T4, T5>(this HtmlHelper htmlHelper, string partialViewName)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4, T5>(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData)
        {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4, T5>(this HtmlHelper htmlHelper, string partialViewName, object model)
        {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }

        public static MvcHtmlString Partial<T1, T2, T3, T4, T5>(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        #endregion
    }
}
