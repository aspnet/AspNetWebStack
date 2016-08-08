// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace System.Web.Mvc.Html
{
    public static class DisplayTextExtensions
    {
        public static MvcHtmlString DisplayText(this HtmlHelper html, string name)
        {
            return DisplayTextHelper(html, ModelMetadata.FromStringExpression(name, html.ViewContext.ViewData));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString DisplayTextFor<TModel, TResult>(this HtmlHelper<TModel> html, Expression<Func<TModel, TResult>> expression)
        {
            return DisplayTextHelper(html, ModelMetadata.FromLambdaExpression(expression, html.ViewData));
        }

        private static MvcHtmlString DisplayTextHelper(HtmlHelper html, ModelMetadata metadata)
        {
            string text = metadata.SimpleDisplayText;
            if (metadata.HtmlEncode)
            {
                text = html.Encode(text);
            }

            return MvcHtmlString.Create(text);
        }
    }
}
