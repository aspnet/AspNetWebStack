// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using ExpressionHelper = Microsoft.Web.Mvc.Internal.ExpressionHelper;

namespace Microsoft.Web.Mvc
{
    public static class ControllerExtensions
    {
        // Shortcut to allow users to write this.RedirectToAction(x => x.OtherMethod()) to redirect
        // to a different method on the same controller.
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static RedirectToRouteResult RedirectToAction<TController>(this TController controller, Expression<Action<TController>> action) where TController : Controller
        {
            return RedirectToAction((Controller)controller, action);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static RedirectToRouteResult RedirectToAction<TController>(this Controller controller, Expression<Action<TController>> action) where TController : Controller
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            RouteValueDictionary routeValues = ExpressionHelper.GetRouteValuesFromExpression(action);
            return new RedirectToRouteResult(routeValues);
        }
    }
}
