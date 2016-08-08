// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Microsoft.Web.Mvc
{
    public static class FormExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcForm BeginForm<TController>(this HtmlHelper helper, Expression<Action<TController>> action) where TController : Controller
        {
            return BeginForm(helper, action, FormMethod.Post, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcForm BeginForm<TController>(this HtmlHelper helper, Expression<Action<TController>> action, FormMethod method) where TController : Controller
        {
            return BeginForm(helper, action, method, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcForm BeginForm<TController>(this HtmlHelper helper, Expression<Action<TController>> action, FormMethod method, object htmlAttributes) where TController : Controller
        {
            return BeginForm(helper, action, method, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcForm BeginForm<TController>(this HtmlHelper helper, Expression<Action<TController>> action, FormMethod method, IDictionary<string, object> htmlAttributes) where TController : Controller
        {
            TagBuilder tagBuilder = new TagBuilder("form");
            tagBuilder.MergeAttributes(htmlAttributes);
            string formAction = helper.BuildUrlFromExpression(action);
            tagBuilder.MergeAttribute("action", formAction);
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method));

            helper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            return new MvcForm(helper.ViewContext);
        }
    }
}
