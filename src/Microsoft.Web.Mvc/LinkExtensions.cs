// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ExpressionHelper = Microsoft.Web.Mvc.Internal.ExpressionHelper;

namespace Microsoft.Web.Mvc
{
    public static class LinkExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "This is a UI method and is required to use strings as Uri"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an Extension Method which allows the user to provide a strongly-typed argument via Expression")]
        public static string BuildUrlFromExpression<TController>(this HtmlHelper helper, Expression<Action<TController>> action) where TController : Controller
        {
            return LinkBuilder.BuildUrlFromExpression(helper.ViewContext.RequestContext, helper.RouteCollection, action);
        }

        /// <summary>
        /// Creates an anchor tag based on the passed in controller type and method
        /// </summary>
        /// <typeparam name="TController">The Controller Type</typeparam>
        /// <param name="helper">The <see cref="HtmlHelper"/> where to create the link.</param>
        /// <param name="action">The Method to route to</param>
        /// <param name="linkText">The linked text to appear on the page</param>
        /// <returns>The anchor tag.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString ActionLink<TController>(this HtmlHelper helper, Expression<Action<TController>> action, string linkText) where TController : Controller
        {
            return ActionLink(helper, action, linkText, null);
        }

        /// <summary>
        /// Creates an anchor tag based on the passed in controller type and method
        /// </summary>
        /// <typeparam name="TController">The Controller Type</typeparam>
        /// <param name="helper">The <see cref="HtmlHelper"/> where to create the link.</param>
        /// <param name="action">The Method to route to</param>
        /// <param name="linkText">The linked text to appear on the page</param>
        /// <param name="htmlAttributes">Any additional HTML attributes.</param>
        /// <returns>The anchor tag.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString ActionLink<TController>(this HtmlHelper helper, Expression<Action<TController>> action, string linkText, object htmlAttributes) where TController : Controller
        {
            RouteValueDictionary routingValues = ExpressionHelper.GetRouteValuesFromExpression(action);

            return helper.RouteLink(linkText, routingValues, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
    }
}
