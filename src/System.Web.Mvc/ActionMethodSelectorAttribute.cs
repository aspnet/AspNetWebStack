// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class ActionMethodSelectorAttribute : Attribute
    {
        /// <summary>
        /// Determines whether the action method selection is valid for the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="methodInfo">Information about the action method.</param>
        /// <returns><see langword="true"/> if the action method selection is valid for the specified controller context; otherwise, <see langword="false"/>.</returns>
        public abstract bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo);
    }
}
