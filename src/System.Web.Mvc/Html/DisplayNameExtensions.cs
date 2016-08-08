// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace System.Web.Mvc.Html
{
    public static class DisplayNameExtensions
    {
        public static MvcHtmlString DisplayName(this HtmlHelper html, string expression)
        {
            return DisplayNameInternal(html, expression, metadataProvider: null);
        }

        internal static MvcHtmlString DisplayNameInternal(this HtmlHelper html, string expression, ModelMetadataProvider metadataProvider)
        {
            return DisplayNameHelper(ModelMetadata.FromStringExpression(expression, html.ViewData, metadataProvider),
                                     expression);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString DisplayNameFor<TModel, TValue>(this HtmlHelper<IEnumerable<TModel>> html, Expression<Func<TModel, TValue>> expression)
        {
            return DisplayNameForInternal(html, expression, metadataProvider: null);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is an extension method")]
        internal static MvcHtmlString DisplayNameForInternal<TModel, TValue>(this HtmlHelper<IEnumerable<TModel>> html, Expression<Func<TModel, TValue>> expression, ModelMetadataProvider metadataProvider)
        {
            return DisplayNameHelper(ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<TModel>(), metadataProvider),
                                     ExpressionHelper.GetExpressionText(expression));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString DisplayNameFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return DisplayNameForInternal(html, expression, metadataProvider: null);
        }

        internal static MvcHtmlString DisplayNameForInternal<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, ModelMetadataProvider metadataProvider)
        {
            return DisplayNameHelper(ModelMetadata.FromLambdaExpression(expression, html.ViewData, metadataProvider),
                                     ExpressionHelper.GetExpressionText(expression));
        }

        public static MvcHtmlString DisplayNameForModel(this HtmlHelper html)
        {
            return DisplayNameHelper(html.ViewData.ModelMetadata, String.Empty);
        }

        internal static MvcHtmlString DisplayNameHelper(ModelMetadata metadata, string htmlFieldName)
        {
            // We don't call ModelMetadata.GetDisplayName here because we want to fall back to the field name rather than the ModelType.
            // This is similar to how the LabelHelpers get the text of a label.
            string resolvedDisplayName = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            return new MvcHtmlString(HttpUtility.HtmlEncode(resolvedDisplayName));
        }
    }
}
