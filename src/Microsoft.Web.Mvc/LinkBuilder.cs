// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using ExpressionHelper = Microsoft.Web.Mvc.Internal.ExpressionHelper;

namespace Microsoft.Web.Mvc
{
    public static class LinkBuilder
    {
        /// <summary>
        /// Builds a URL based on the Expression passed in
        /// </summary>
        /// <typeparam name="TController">Controller Type Only</typeparam>
        /// <param name="context">The current ViewContext</param>
        /// <param name="routeCollection">The <see cref="RouteCollection"/> to use for building the URL.</param>
        /// <param name="action">The action to invoke</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "The return value is not a regular URL since it may contain ~/ ASP.NET-specific characters"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an Extension Method which allows the user to provide a strongly-typed argument via Expression"), SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Need to be sure the passed-in argument is of type Controller::Action")]
        public static string BuildUrlFromExpression<TController>(RequestContext context, RouteCollection routeCollection, Expression<Action<TController>> action) where TController : Controller
        {
            RouteValueDictionary routeValues = ExpressionHelper.GetRouteValuesFromExpression(action);
            VirtualPathData vpd = routeCollection.GetVirtualPathForArea(context, routeValues);
            return (vpd == null) ? null : vpd.VirtualPath;
        }

        /// <summary>
        /// Creates a querystring as a Dictionary based on the passed-in Lambda
        /// </summary>
        /// <param name="call">The Lambda of the Controller method</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Allowing Lambda compilation to fail if it doesn't compile at run time - design-time compilation will not allow for runtime Exception")]
        public static RouteValueDictionary BuildParameterValuesFromExpression(MethodCallExpression call)
        {
            RouteValueDictionary result = new RouteValueDictionary();

            ParameterInfo[] parameters = call.Method.GetParameters();

            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression arg = call.Arguments[i];
                    object value;
                    ConstantExpression ce = arg as ConstantExpression;
                    if (ce != null)
                    {
                        // If argument is a constant expression, just get the value
                        value = ce.Value;
                    }
                    else
                    {
                        try
                        {
                            value = CachedExpressionCompiler.Evaluate(arg);
                        }
                        catch
                        {
                            // ?????
                            value = String.Empty;
                        }
                    }
                    // Code should be added here to appropriately escape the value string
                    result.Add(parameters[i].Name, value);
                }
            }
            return result;
        }
    }
}
